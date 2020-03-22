using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAI
{
    public class RacingState : BaseState
    {
        private RaceCircuit _circuit;

        private int _carCurrentWaypoint;
        private int _trackerCurrentWaypoint;

        private float currentInput;
        private float _trackerLootAheadDistance = 15f;

        private List<WayPoint> _analysisWaypoints;
        
        public RacingState(RaceCircuit circuit, DriverAI ai, BaseCar car) : base(ai, car)
        {
            _circuit = circuit;
        }

        public override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (_analysisWaypoints == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(Car.transform.position, _analysisWaypoints[0].transform.position);
            foreach(WayPoint point in _analysisWaypoints)
            {
                Gizmos.DrawWireCube(point.transform.position, Vector3.one);
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(_analysisWaypoints[0].transform.position, Car.transform.forward * 20);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(_analysisWaypoints[0].transform.position, _analysisWaypoints[_analysisWaypoints.Count - 1].transform.position - _analysisWaypoints[0].transform.position);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //Steer analyst
            UpdateCarWaypoint();
            AnalysisWaypoints();

            SetSteering();
            SetMoveInput();
        }

        private void SetSteering()
        {
            float steerAngleToTracker = GetAngleToBetweenTransfors(Car.transform, Driver.Tracker.transform.position);

            //FROM BASE STATE: уворот

            if (_frontSensors[0].IsDetected)
                steerAngleToTracker += Driver.Car.MaxWheelAngle * 0.33f;
            else
                steerAngleToTracker -= Driver.Car.MaxWheelAngle * 0.33f;

            if (_frontSensors[2].IsDetected)
                steerAngleToTracker -= Driver.Car.MaxWheelAngle * 0.33f;
            else
                steerAngleToTracker += Driver.Car.MaxWheelAngle * 0.33f;

            //end 

            float lerpSteerAngle = Mathf.Lerp(Driver.Car.CurrentWheelAngle, steerAngleToTracker, Time.fixedDeltaTime * Driver.WheelAngleSpeed);

            Car.SetSteerAngle(lerpSteerAngle);
        }

        private void SetMoveInput()
        {
            //Move input analyst
            float cornerAngle = Vector3.Angle(Car.transform.forward * 20, _analysisWaypoints[_analysisWaypoints.Count - 1].transform.position - _analysisWaypoints[0].transform.position);
            float distanceToCorner = Vector3.Distance(Car.transform.position, _analysisWaypoints[0].transform.position);

            float targetSpeed = Driver.SpeedByCornerAnlge.Evaluate(cornerAngle);
            float brakingDistance = Driver.BrakingDistanceByDeltaSpeed.Evaluate(Mathf.Clamp(Driver.Car.CarSpeed - targetSpeed, 0, float.MaxValue));

            float deltaSpeed = Mathf.Clamp(Driver.Car.CarSpeed - targetSpeed, 0, float.MaxValue);

            if(Driver.Car.IsDebug)
                Debug.Log($"Distance to turn start: {Vector3.Distance(Car.transform.position, _analysisWaypoints[0].transform.position)} Angle: {cornerAngle}; DeltaSpeed: {deltaSpeed} BDistance: {brakingDistance}");

            float targetMoveInput;

            if (deltaSpeed >= 0)
            {
                if (distanceToCorner > brakingDistance)
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
            _analysisWaypoints = _circuit.GetWaypointsInDistance(Car.transform.position, 50, _carCurrentWaypoint);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateTrackerPosition();
        }

        private void UpdateTrackerPosition()
        {
            if (Vector3.Distance(Driver.Tracker.transform.position, Driver.transform.position) > _trackerLootAheadDistance)
                return;

            Driver.Tracker.transform.LookAt(_circuit.GetPoint(_trackerCurrentWaypoint).transform.position);
            Driver.Tracker.transform.transform.Translate(0f, 0f, 30.0f * Time.deltaTime);

            if (Vector3.Distance(_circuit.GetPoint(_trackerCurrentWaypoint).transform.position, Driver.Tracker.transform.transform.position) < 0.1f)
                _trackerCurrentWaypoint = _circuit.GetNextPoint(_trackerCurrentWaypoint);

        }

        private void UpdateCarWaypoint()
        {
            if (_analysisWaypoints == null || _analysisWaypoints.Count == 0)
                return;

            if (_trackerCurrentWaypoint != _carCurrentWaypoint && Vector3.Distance(Driver.Tracker.transform.position, _analysisWaypoints[0].transform.position) > _trackerLootAheadDistance)
                _carCurrentWaypoint = _circuit.GetNextPoint(_carCurrentWaypoint);
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
