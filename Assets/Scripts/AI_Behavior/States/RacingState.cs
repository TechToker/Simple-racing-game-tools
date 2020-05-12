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

    public enum BrakeAlgorithm
    {
        PID,
        BANGBANG,
    }

    public class RacingState : BaseState
    {
        private readonly RaceCircuit _circuit;
        private List<PathSegment> _racingPath;

        private float _steerInput;
        private float _accelerationInput;
        
        public int GetCurrentWaypointIndex => _currentWaypointIndexOnTrack;
        private int _currentWaypointIndexOnTrack;
        public PathSegment NextTargetPosition => _racingPath[0];

        //Turning
        private Vector3 _turningTargetPoint;

        //Braking
        private Vector3 _brakingPoint;
        
        private float _nextCornerAngle;
        private float _cornerTargetSpeed;
        private float _brakingDistance;
        
        public BrakingData BrakingData { get; private set; }
        
        //Waypoints
        private Sides _carWaypointApproachSide;

        public RacingState(RaceCircuit circuit, DriverAI ai, BaseCar car) : base(ai, car)
        {
            _circuit = circuit;
            _currentWaypointIndexOnTrack = 0;
        }

        public override void OnEveryGizmosDraw()
        {
            base.OnEveryGizmosDraw();

            if (_racingPath == null)
                return;

            //Local racing line
            if (Driver.ShowTurnDataGizmos)
                DrawTurnDataGizmos();
        }

        private void DrawTurnDataGizmos()
        {
            Gizmos.color = Color.green;
            foreach (PathSegment point in _racingPath)
                Gizmos.DrawSphere(point.CarRacingPoint, 0.25f);

            Gizmos.color = new Color(0, 0.3f, 0);
            Gizmos.DrawLine(Car.transform.position, _racingPath[0].CarRacingPoint);
            Gizmos.DrawRay(_racingPath[0].CarRacingPoint, Car.transform.forward * 20);

            Gizmos.DrawRay(_racingPath[0].CarRacingPoint,
                _racingPath[_racingPath.Count - 1].CarRacingPoint - _racingPath[0].CarRacingPoint);

            //Target turning point
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(Car.CarFrontBumperPos, _turningTargetPoint);
            Gizmos.DrawSphere(_turningTargetPoint, 0.1f);
            Gizmos.DrawSphere(Car.CarFrontBumperPos, 0.05f);

            //Braking point
            if (_brakingDistance > 0.5f)
            {
                Gizmos.color = Color.red;
                Vector3 brakingPosition = _racingPath[0].CarRacingPoint + (Car.transform.position - _racingPath[0].CarRacingPoint).normalized * _brakingDistance;
                Gizmos.DrawLine(brakingPosition - Car.transform.right * 3, brakingPosition + Car.transform.right * 3);
                Handles.Label(brakingPosition - Car.transform.right * 3,$"Braking: {Math.Round(_brakingDistance, 1)}");
            }

            //Corner info
            string cornerInfo = $"ang: {Math.Round(_nextCornerAngle, 1)}°{Environment.NewLine} tSpeed: {Math.Round(_cornerTargetSpeed, 1)}";

            Handles.Label(_racingPath[0].CarRacingPoint, cornerInfo);
        }

        public override void OnEveryFixedUpdate()
        {
            base.OnEveryFixedUpdate();

            //Waypoints update
            CheckIsWaypointReached();
            UpdateNextWaypointsList();

            //Steer analyst
            UpdateTurningTarget();

            //Calculate inputs
            _steerInput = CalculateSteeringInput();
            _accelerationInput = CalculateMoveInput();

            float rbExtraAcceleration = (Driver.RubberbandingController.RbTorqueMultiplyer - 1) * Driver.RubberbandingController.RBTorqueMultiplyerBySpeed.Evaluate(Car.CarSpeed);
            
            Car.SetSteerAngle(_steerInput);
            
            Car.SetMotorTorque(Mathf.Clamp(_accelerationInput, 0, 1) * 1f + rbExtraAcceleration);
            Car.SetBrakeTorque(Mathf.Clamp(-_accelerationInput, 0, 1));
        }
        
        private void CheckIsWaypointReached()
        {
            if(_racingPath == null || _racingPath.Count == 0)
                return;
            
            if (_carWaypointApproachSide != GetCarWaypointApproachSide(_racingPath[0].Waypoint))
            {
                //Reset braking debug
                if (BrakingData != null)
                {
                    BrakingData.SetCurrentData(0, Car.CarSpeed);
                    BrakingData.IsRecording = false;
                }

                _currentWaypointIndexOnTrack++;
                
                WayPoint nextWp = _circuit.GetWaypointByIndex(_currentWaypointIndexOnTrack);
                _carWaypointApproachSide = GetCarWaypointApproachSide(nextWp);
            }
        }

        private float CalculateSteeringInput()
        {
            float steerAngleToTracker = MathfExtensions.GetAngleToBetweenTransfors(Car.transform, _turningTargetPoint);
            return Mathf.Lerp(Driver.Car.CurrentWheelAngle, steerAngleToTracker, Time.fixedDeltaTime * Driver.WheelAngleSpeed);
        }

        private float CalculateMoveInput()
        {
            //Move input analyst
            _nextCornerAngle = Vector3.Angle(Car.RigidbodyTransform.forward * 20, _racingPath[_racingPath.Count - 1].CarRacingPoint - _racingPath[0].CarRacingPoint);
            _cornerTargetSpeed = Driver.SpeedByCornerAnlge.Evaluate(_nextCornerAngle);
            //_cornerTargetSpeed = 8;
            
            float distanceToCorner = Vector3.Distance(Car.CarFrontBumperPos, _racingPath[0].CarRacingPoint);
            _brakingDistance = Driver.BrakingDistanceByDeltaSpeed.Evaluate(Mathf.Clamp(Driver.Car.CarSpeed - _cornerTargetSpeed, 0, float.MaxValue));
            _brakingDistance *= Driver.RubberbandingController.RbBrakingDistanceMultiplyer;
            //_brakingDistance = 40;

            WriteBrakingDebugData(distanceToCorner);
            
            float moveInput;
            if (BrakingData == null || !BrakingData.IsRecording)
                moveInput = Mathf.Lerp(_accelerationInput, 1, Time.fixedDeltaTime * Driver.RubberbandingController.RbAccelerationSpeedMultiplyer);
            else
            {
                float distanceProgressPercentage = distanceToCorner / BrakingData.StartBrakingDistance;
                float targetBrakingSpeed = BrakingData.CornerExitSpeed + distanceProgressPercentage * (BrakingData.CornerEnterSpeed - BrakingData.CornerExitSpeed);
                float speedError = targetBrakingSpeed - Car.CarSpeed;
             
                //Brake algorithm
                switch (Driver.BrakeAlgorithm)
                {
                    case (BrakeAlgorithm.BANGBANG): 
                        moveInput = speedError > 0 ? 1 : -1;
                        break;
                    default: //PID
                        moveInput = Driver.ProportionalCoef * speedError;
                        break;
                }
            }
            
            return Mathf.Clamp(moveInput, -1, 1);
        }

        private void WriteBrakingDebugData(float distanceToCorner)
        {
            if ((BrakingData == null || !BrakingData.IsRecording) && distanceToCorner < _brakingDistance)
                BrakingData = new BrakingData(Car.CarSpeed, _cornerTargetSpeed, distanceToCorner);

            if(BrakingData != null && BrakingData.IsRecording)
                BrakingData.SetCurrentData(distanceToCorner, Car.CarSpeed);
        }

        private void UpdateNextWaypointsList()
        {
            //TODO: Remove another direction turn from list
            List<WayPoint> wpList = _circuit.GetWaypointsInDistance(Car.CarFrontBumperPos, 35, _currentWaypointIndexOnTrack);

            _racingPath = new List<PathSegment>();
            foreach (WayPoint wp in wpList)
                _racingPath.Add(new PathSegment(Driver, wp));
        }

        //Возвращает сторону с которой автомобиль приближается к Waypoint
        //Вычисление со стороны правого края дороги
        private Sides GetCarWaypointApproachSide(WayPoint waypoint)
        {
            Vector3 waypointLineVector = waypoint.LeftBorder - waypoint.RightBorder;
            Vector3 toCarWaypointVector = Driver.Car.CarFrontBumperPos - waypoint.RightBorder;
            float singledAngle = Vector3.SignedAngle(waypointLineVector, toCarWaypointVector, Vector3.up);
            
            return singledAngle < 0 ? Sides.LEFT : Sides.RIGHT;
        }

        private void UpdateTurningTarget()
        {
            float distance = Driver.TurningTargetDistanceBySpeed.Evaluate(Car.CarSpeed);
            
            Vector3 pointFrom = Car.RigidbodyTransform.position;
            Vector3 pointTo = _racingPath[0].CarRacingPoint;

            for (int i = 0; i < _racingPath.Count - 1; i++)
            {
                float distToNextTarget = (pointFrom - pointTo).magnitude;
                
                if (distance < distToNextTarget)
                {
                    break;
                }
                else
                {
                    distance -= distToNextTarget;
                    pointFrom = _racingPath[i].CarRacingPoint;
                    pointTo = _racingPath[i + 1].CarRacingPoint;
                }
            }
            
            if (Driver.ObstacleAvoidController.NeedAvoid)
                _turningTargetPoint = Driver.ObstacleAvoidController.ObstacleAvoidPosition + Driver.ObstacleAvoidController.transform.forward * 15;
            else
                _turningTargetPoint =  pointFrom + (pointTo - pointFrom).normalized * distance;
        }

        public override string GetStateName()
        {
            return "[Racing state]";
        }
    }
}
