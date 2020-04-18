using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviourAI
{
    public enum Sides
    {
        LEFT,
        RIGHT,
    }
    
    public class RacingState : BaseState
    {
        private RaceCircuit _circuit;

        private WayPoint _currentWaypoint;
        private int _currentWaypointIndex;
        
        private float currentInput;

        //TODO: Replace by List<Vector3>; Car real driving line
        private List<WayPoint> _analysisWaypoints;

        //Turning
        private Vector3 _turningTargetPoint;
        private float _turningTargetDistance = 10f;

        //Braking
        private Vector3 _brakingPoint;
        
        private float _nextCornerAngle;
        private float _cornerTargetSpeed;
        private float _brakingDistance;
        
        //Waypoints
        private Sides _carWaypointApproachSide;

        public RacingState(RaceCircuit circuit, DriverAI ai, BaseCar car) : base(ai, car)
        {
            _circuit = circuit;

            _currentWaypointIndex = 0;
            _currentWaypoint = _circuit.GetWaypointByIndex(_currentWaypointIndex);
        }

        public override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (_analysisWaypoints == null)
                return;

            Gizmos.color = Color.green;
            foreach(WayPoint point in _analysisWaypoints)
                Gizmos.DrawSphere(point.FinalRacingPoint, 0.25f);
            
            Gizmos.color = new Color(0, 0.3f, 0);
            Gizmos.DrawLine(Car.transform.position, _analysisWaypoints[0].FinalRacingPoint);
            Gizmos.DrawRay(_analysisWaypoints[0].FinalRacingPoint, Car.transform.forward * 20);
            
            Gizmos.DrawRay(_analysisWaypoints[0].FinalRacingPoint, _analysisWaypoints[_analysisWaypoints.Count - 1].FinalRacingPoint - _analysisWaypoints[0].FinalRacingPoint);

            //Target turning point
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_turningTargetPoint, 0.2f);
            
            //Braking point
            if (_brakingDistance > 1)
            {
                Gizmos.color = Color.red;
                Vector3 brakingPosition = _analysisWaypoints[0].FinalRacingPoint + (Car.transform.position - _analysisWaypoints[0].FinalRacingPoint).normalized * _brakingDistance;
                Gizmos.DrawLine(brakingPosition - Car.transform.right * 3, brakingPosition + Car.transform.right * 3);
                Handles.Label(brakingPosition - Car.transform.right * 3, $"Braking: {Math.Round(_brakingDistance, 1)}");
            }

            //Corner info
            string cornerInfo = $"ang: {Math.Round(_nextCornerAngle, 1)}°{Environment.NewLine}" +
                                $"tSpeed: {Math.Round(_cornerTargetSpeed, 1)}";
            
            Handles.Label(_analysisWaypoints[0].FinalRacingPoint, cornerInfo);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //Steer analyst
            AnalysisWaypoints();

            SetSteering();
            SetMoveInput();
        }

        private void SetSteering()
        {
            float steerAngleToTracker = GetAngleToBetweenTransfors(Car.transform, _turningTargetPoint);
            
//            //FROM BASE STATE: уворот
//
//            if (_frontSensors[0].IsDetected)
//                steerAngleToTracker += Driver.Car.MaxWheelAngle * 0.33f;
//            else
//                steerAngleToTracker -= Driver.Car.MaxWheelAngle * 0.33f;
//
//            if (_frontSensors[2].IsDetected)
//                steerAngleToTracker -= Driver.Car.MaxWheelAngle * 0.33f;
//            else
//                steerAngleToTracker += Driver.Car.MaxWheelAngle * 0.33f;
//
//            //end 

            float lerpSteerAngle = Mathf.Lerp(Driver.Car.CurrentWheelAngle, steerAngleToTracker, Time.fixedDeltaTime * Driver.WheelAngleSpeed);

            Car.SetSteerAngle(lerpSteerAngle);
        }

        private void SetMoveInput()
        {
            //Move input analyst
            _nextCornerAngle = Vector3.Angle(Car.transform.forward * 20, _analysisWaypoints[_analysisWaypoints.Count - 1].FinalRacingPoint - _analysisWaypoints[0].FinalRacingPoint);
            float distanceToCorner = Vector3.Distance(Car.transform.position, _analysisWaypoints[0].FinalRacingPoint);

            _cornerTargetSpeed = Driver.SpeedByCornerAnlge.Evaluate(_nextCornerAngle);
            _brakingDistance = Driver.BrakingDistanceByDeltaSpeed.Evaluate(Mathf.Clamp(Driver.Car.CarSpeed - _cornerTargetSpeed, 0, float.MaxValue));

            float deltaSpeed = Mathf.Clamp(Driver.Car.CarSpeed - _cornerTargetSpeed, 0, float.MaxValue);

            if(Driver.Car.IsDebug)
                Debug.Log($"Distance to turn start: {Vector3.Distance(Car.transform.position, _analysisWaypoints[0].FinalRacingPoint)} Angle: {_nextCornerAngle}; DeltaSpeed: {deltaSpeed} BDistance: {_brakingDistance}");

            float targetMoveInput;

            if (deltaSpeed >= 0)
            {
                if (distanceToCorner > _brakingDistance)
                    targetMoveInput = 1f;
                else
                    targetMoveInput = -1f;
            }
            else
                targetMoveInput = 0.2f;

            float lerpMoveInput = Mathf.Lerp(currentInput, targetMoveInput, Time.fixedDeltaTime * 10);
            currentInput = lerpMoveInput;

            Car.SetMotorTorque(Mathf.Clamp(lerpMoveInput, 0, 1));
            Car.SetBrakeTorque(Mathf.Clamp(-lerpMoveInput, 0, 1));
        }

        private void AnalysisWaypoints()
        {
            //TODO: Remove another direction turn from list
            _analysisWaypoints = _circuit.GetWaypointsInDistance(Car.transform.position, 50, _currentWaypointIndex);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateCurrentWaypoint();
            UpdateTurningTarget();
        }

        private void UpdateCurrentWaypoint()
        {
            if (_carWaypointApproachSide != GetCarWaypointApproachSide(_currentWaypoint))
            {
                _currentWaypointIndex++;
                _currentWaypoint = _circuit.GetWaypointByIndex(_currentWaypointIndex);
                
                _carWaypointApproachSide = GetCarWaypointApproachSide(_currentWaypoint);
            }
        }

        //Возвращает сторону с которой автомобиль приближается к Waypoint
        //Вычисление со стороны правого края дороги
        private Sides GetCarWaypointApproachSide(WayPoint waypoint)
        {
            Vector3 waypointLineVector = waypoint.LeftBorder - waypoint.RightBorder;
            Vector3 toCarWaypointVector = Driver.transform.position + Driver.transform.forward * 2.5f - waypoint.RightBorder;
            float singledAngle = Vector3.SignedAngle(waypointLineVector, toCarWaypointVector, Vector3.up);
            
            return singledAngle < 0 ? Sides.LEFT : Sides.RIGHT;
        }

        private void UpdateTurningTarget()
        {
            float distance = _turningTargetDistance;
            
            Vector3 pointFrom = Driver.transform.position;
            Vector3 pointTo = _analysisWaypoints[0].FinalRacingPoint;

            for (int i = 0; i < _analysisWaypoints.Count - 1; i++)
            {
                float distToNextTarget = (pointFrom - pointTo).magnitude;
                
                if (distance < distToNextTarget)
                {
                    break;
                }
                else
                {
                    distance -= distToNextTarget;
                    pointFrom = _analysisWaypoints[i].FinalRacingPoint;
                    pointTo = _analysisWaypoints[i + 1].FinalRacingPoint;
                }
            }

            _turningTargetPoint = pointFrom + (pointTo - pointFrom).normalized * distance;
        }

        private float GetTurnMileage()
        {
            float distance = 0;

            //If is simple corner (only two points => не учитывать длину поворота)
            if (_analysisWaypoints.Count <= 2)
                return 0;

            for(int i = 0; i < _analysisWaypoints.Count - 1; i++)
                distance += Vector3.Distance(_analysisWaypoints[i].transform.position, _analysisWaypoints[i + 1].transform.position);
            
            return distance;
        }

        //DUPLICATE from chase state
        private float GetAngleToBetweenTransfors(Transform from, Vector3 to)
        {
            Vector3 deltaVec = from.InverseTransformPoint(to);
            return Mathf.Atan2(deltaVec.x, deltaVec.z) * Mathf.Rad2Deg;
        }
    }
}
