using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourAI
{
    public class ChaseState : BaseState
    {
        public enum TurningDirections
        {
            NONE,
            LEFT,
            RIGHT,
        }

        private bool _roadExist;
        private NavMeshPath _navMeshPath;

        public List<Vector3> AnalystTurningPositions = new List<Vector3>();

        public ChaseState(DriverAI driver, BaseCar car): base(driver, car)
        {
            _navMeshPath = new NavMeshPath();
            GameManager.Instance.StartCoroutine(CalculatePathToTarget());
        }

        public override void OnEveryGizmosDraw()
        {
            if (_navMeshPath == null)
                return;

            DrawNavPath();
            DrawCarSensors();
        }

        private void DrawNavPath()
        {
            Gizmos.color = Color.magenta;
            Vector3[] currentPath = _navMeshPath.corners;

            for (int i = 0; i < currentPath.Length - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }

            for (int i = 0; i < AnalystTurningPositions.Count; i++)
            {
                Gizmos.DrawWireCube(AnalystTurningPositions[i], Vector3.one);
            }
        }

        private void DrawCarSensors()
        {
            //Draw road raycast in front
            Gizmos.color = _roadExist ? Color.white : Color.red;
            Gizmos.DrawLine(Driver.transform.position, Driver.transform.position + Driver.transform.forward * Driver.CarSensorsDistance);
        }

        public override void OnEveryUpdate()
        {

        }

        private IEnumerator CalculatePathToTarget()
        {
            yield return new WaitForSeconds(Driver.TargetPositionUpdateDelay);

            Vector3 predictCarPosition = Driver.TargetCar.transform.position + Driver.TargetCar.RelativeForce;
            Debug.LogError($"{Driver.TargetCar.RelativeForce}");
            NavMesh.CalculatePath(Driver.transform.position, predictCarPosition, NavMesh.AllAreas, _navMeshPath);
            CheckRoadInFront();

            GameManager.Instance.StartCoroutine(CalculatePathToTarget());
        }

        private void CheckRoadInFront()
        {
            _roadExist = !NavMesh.Raycast(Driver.transform.position, Driver.transform.position + Driver.transform.forward * Driver.CarSensorsDistance, out NavMeshHit hit, NavMesh.AllAreas);
        }

        public override void OnEveryFixedUpdate()
        {
            if (_navMeshPath.corners.Length == 0)
                return;

            //Calculate distance to turn
            //Plus: calculate actual turning points

            AnalystTurningPositions = GetActualTurningPoints(_navMeshPath.corners, Driver.TurningAlanystDistance);

            //Calculate turn angle

            Vector3 cornerExitPoint = AnalystTurningPositions[AnalystTurningPositions.Count != 0 ? AnalystTurningPositions.Count - 1 : 0];
            float turnAngle = GetAngleToBetweenTransfors(Driver.transform, cornerExitPoint);
            TurningDirections direction = turnAngle > 0 ? TurningDirections.RIGHT : TurningDirections.LEFT;

            SetCarExpectedSpeed(Driver.TargetSpeedByTurnAngle.Evaluate(Mathf.Abs(turnAngle)));
            Debug.Log($"{turnAngle}");

            if (Driver._brakingPedalForce == 0)
                Car.SetMotorTorque(1);

            Car.SetSteerAngle(GetAngleToBetweenTransfors(Driver.transform, AnalystTurningPositions[0]));
        }

        private void SetCarExpectedSpeed(float expectedSpeed)
        {
            if (Car.CarSpeed > expectedSpeed)
            {
                Driver._brakingPedalForce += Driver._brakingPedalForceDelta;
            }
            else
            {
                Driver._brakingPedalForce -= Driver._brakingPedalForceDelta;
            }

            Debug.Log($"Current Speed: {Car.CarSpeed}; Expected speed: {expectedSpeed};");
            Driver._brakingPedalForce = Mathf.Clamp(Driver._brakingPedalForce, 0, 1f);
            Car.SetBrakeTorque(Driver._brakingPedalForce);
        }

        private float GetAngleToBetweenTransfors(Transform from, Vector3 to)
        {
            Vector3 deltaVec = from.InverseTransformPoint(to);
            return Mathf.Atan2(deltaVec.x, deltaVec.z) * Mathf.Rad2Deg;
        }

        private List<Vector3> GetActualTurningPoints(Vector3[] navPath, float analysisDistance)
        {
            List<Vector3> actualPoints = new List<Vector3>();
            float _currentAnalysisDistance = 0;

            //! TODO: Может быть проблема
            //Пушо может возникнуть ситуация когда точку уже проехали, но она уже за радиусом позади нас
            navPath[0] = Driver.transform.position;

            for (int i = 1; i < navPath.Length; i++)
            {
                //Берем список точек которые находятся на некотором минимальном удалении от авто. Не распространяется на последнюю точку пути (цель)
                if (Vector3.Distance(Driver.transform.position, navPath[i]) > 1 || (i == navPath.Length - 1 && actualPoints.Count == 0))
                {
                    float distanceBetweenCorners = Vector3.Distance(actualPoints.Count == 0 ? Driver.transform.position : actualPoints[actualPoints.Count - 1], navPath[i]);
                    _currentAnalysisDistance += distanceBetweenCorners;

                    actualPoints.Add(navPath[i]);

                    if (_currentAnalysisDistance > analysisDistance)
                        return actualPoints;
                }
            }


            return actualPoints;
        }

        public override string GetStateName()
        {
            return "[Chase]";
        }
    }
}
