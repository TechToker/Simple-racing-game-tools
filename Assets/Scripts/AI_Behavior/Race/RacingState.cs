using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAI
{
    public class RacingState : BaseState
    {
        private RaceCircuit _circuit;
        private int currentWaypointIndex;

        private List<WayPoint> _analysisWaypoints;
        
        public RacingState(RaceCircuit circuit, DriverAI ai, BaseCar car) : base(ai, car)
        {
            _circuit = circuit;
        }

        public override void OnDrawGizmos()
        {
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

        private bool isBrakingWhileNotCorner;
        private float currentInput;
        private float _waypointCompleteRadius = 4.5f;

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            AnalysisWaypoints();
            if (Vector3.Distance(Car.transform.position, _analysisWaypoints[0].transform.position) < _waypointCompleteRadius)
            {
                isBrakingWhileNotCorner = false;
                currentWaypointIndex = _circuit.GetNextPoint(currentWaypointIndex);
            }

            float targetAngle = GetAngleToBetweenTransfors(Car.transform, _analysisWaypoints[0].transform.position);

            float lerpAngle = targetAngle;

            lerpAngle = Mathf.Lerp(Driver.Car.CurrentWheelAngle, targetAngle, Time.fixedDeltaTime * Driver.WheelAngleSpeed);
            Debug.LogError($"Cur ang: {Driver.Car.CurrentWheelAngle}; Target ang: {targetAngle}; lerp ang: {lerpAngle}");

            Car.SetSteerAngle(lerpAngle);

            //Debug.LogError($"Distance to turn start: {Vector3.Distance(Car.transform.position, _analysisWaypoints[0].transform.position)}; Turn mileage: {GetTurnMileage()}" +
            // $" Turn angle: {Vector3.Angle(Car.transform.forward * 20, _analysisWaypoints[_analysisWaypoints.Count - 1].transform.position - _analysisWaypoints[0].transform.position)}");

            float cornerAngle = Vector3.Angle(Car.transform.forward * 20, _analysisWaypoints[_analysisWaypoints.Count - 1].transform.position - _analysisWaypoints[0].transform.position);
            float distanceToCorner = Vector3.Distance(Car.transform.position, _analysisWaypoints[0].transform.position) - _waypointCompleteRadius;

            float targetSpeed = Driver.SpeedByCornerAnlge.Evaluate(cornerAngle);
            float brakingDistance = Driver.BrakingDistanceByDeltaSpeed.Evaluate(Mathf.Clamp(Driver.Car.CarSpeed - targetSpeed, 0, float.MaxValue));

            //Debug.LogError($"Distance to turn start: {Vector3.Distance(Car.transform.position, _analysisWaypoints[0].transform.position)} Angle: {cornerAngle}; DeltaSpeed: {Mathf.Clamp(Driver.Car.CarSpeed - targetSpeed, 0, float.MaxValue)} BDistance: {brakingDistance}");

            float targetMoveInput;
            if (distanceToCorner > brakingDistance)
            {
                targetMoveInput = 1f;
            } else
            {
                targetMoveInput = -1f;
            }

            float moveInput = Mathf.Lerp(currentInput, targetMoveInput, Time.fixedDeltaTime * 10);

            currentInput = moveInput;
            Car.SetMotorTorque(Mathf.Clamp(moveInput, 0, 1));
            Car.SetBrakeTorque(Mathf.Clamp(-moveInput, 0, 1));
        }

        private void AnalysisWaypoints()
        {
            //TODO: Remove another direction turn from list
            _analysisWaypoints = _circuit.GetWaypointsInDistance(Car.transform.position, 50, currentWaypointIndex);
           
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
