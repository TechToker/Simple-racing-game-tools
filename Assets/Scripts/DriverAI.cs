using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CurrentDriverMode
{
    None = 0,
    Chase = 1,
    BrakeAroundTarget = 2,
}

public class DriverAI : BaseDriver
{
    [Header("AI params")]
    [SerializeField] private Transform _taget;
    [SerializeField] private float _targetPosUpdateTime = 8;

    public float CheckRoadDistance = 3f;
    public bool _roadExist;

    private NavMeshPath _navMeshPath;

    [SerializeField] private AnimationCurve _targetBrakingSpeedByTargetDistance;

    public CurrentDriverMode CurrentMode;

    [Header("Autobraking")]
    public Vector3 _finishBrakingPosition;

    public float _brakingPedalForce = 0.1f;
    public float _brakingPedalForceDelta = 0.05f;

    [Header("TurningBraking")]
    public AnimationCurve _targetSpeedByTurningAngle;

    private void OnEnable()
    {
        _navMeshPath = new NavMeshPath();
        StartCoroutine(CalculatePathToTarget());
    }

    private IEnumerator CalculatePathToTarget()
    {
        yield return new WaitForSeconds(_targetPosUpdateTime);

        NavMesh.CalculatePath(transform.position, _taget.transform.position, NavMesh.AllAreas, _navMeshPath);
        CheckRoadInFront();

        StartCoroutine(CalculatePathToTarget());
    }

    private void CheckRoadInFront()
    {
        _roadExist = !NavMesh.Raycast(transform.position, transform.position + transform.forward * CheckRoadDistance, out NavMeshHit hit, NavMesh.AllAreas);
    }

    private void OnDrawGizmos()
    {
        if (_navMeshPath == null)
            return;

        DrawNavPath();
        DrawCarSensors();
        DrawCarBraking();
    }

    private void DrawNavPath()
    {
        Gizmos.color = Color.magenta;
        Vector3[] currentPath = _navMeshPath.corners;

        for (int i = 0; i < currentPath.Length - 1; i++)
        {
            Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
        }

        for(int i = 0; i < AnalystTurningPositions.Count; i++)
        {
            Gizmos.DrawWireCube(AnalystTurningPositions[i], Vector3.one);
        }
    }

    private void DrawCarSensors()
    {
        //Draw road raycast in front
        Gizmos.color = _roadExist ? Color.white : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * CheckRoadDistance);
    }

    private void DrawCarBraking()
    {
        Gizmos.color = Color.yellow;

        if (_finishBrakingPosition != Vector3.zero)
            Gizmos.DrawLine(transform.position, _finishBrakingPosition);
    }

    protected void Update()
    {
        if (_navMeshPath == null || _navMeshPath.corners.Length == 0)
            return;
    }

    private void ActivateBrakeAroundTarget()
    {
        CurrentMode = CurrentDriverMode.BrakeAroundTarget;
        Debug.LogError("Braking");

        _finishBrakingPosition = _navMeshPath.corners[_navMeshPath.corners.Length - 1];
    }

    protected void FixedUpdate()
    {
        if(CurrentMode == CurrentDriverMode.Chase)
            DOChase();
        if (CurrentMode == CurrentDriverMode.BrakeAroundTarget)
            DOBrakingByTarget();
    }

    public float MotorTorque = 1f;
    public float TurningAlanystDistance = 30f;
    public List<Vector3> AnalystTurningPositions = new List<Vector3>();

    public enum TurningDirections
    {
        NONE,
        LEFT,
        RIGHT,
    }

    public float _distanceToLocalTarget = 4f;
    public Vector2 _localTarget;
    public float _carRadius = 1.5f;

    private void DOChase()
    {
        if (_navMeshPath.corners.Length == 0)
            return;

        //Calculate distance to turn
        //Plus: calculate actual turning points

        AnalystTurningPositions = GetActualTurningPoints(_navMeshPath.corners, TurningAlanystDistance);

        //Calculate turn angle

        Vector3 cornerExitPoint = AnalystTurningPositions[AnalystTurningPositions.Count != 0 ? AnalystTurningPositions.Count - 1 : 0];
        float turnAngle = GetAngleToBetweenTransfors(transform, cornerExitPoint);
        TurningDirections direction = turnAngle > 0 ? TurningDirections.RIGHT : TurningDirections.LEFT;

        SetCarExpectedSpeed(_targetSpeedByTurningAngle.Evaluate(Mathf.Abs(turnAngle)));
        Debug.Log($"{turnAngle}");

        if(_brakingPedalForce == 0)
            Car.SetMotorTorque(MotorTorque);

        Car.SetSteerAngle(GetAngleToBetweenTransfors(transform.transform, AnalystTurningPositions[0]));
    }

    private List<Vector3> GetActualTurningPoints(Vector3[] navPath, float analysisDistance)
    {
        List<Vector3> actualPoints = new List<Vector3>();
        float _currentAnalysisDistance = 0;

        //! TODO: Может быть проблема
        //Пушо может возникнуть ситуация когда точку уже проехали, но она уже за радиусом позади нас
        navPath[0] = transform.position;

        for (int i = 1; i < navPath.Length; i++)
        {
            //Берем список точек которые находятся за радиусом автомобиля. Не распространяется на последнюю точку пути (цель)
            if (Vector3.Distance(transform.position, navPath[i]) > _carRadius || (i == navPath.Length - 1 && actualPoints.Count == 0))
            {
                float distanceBetweenCorners = Vector3.Distance(actualPoints.Count == 0 ? transform.position : actualPoints[actualPoints.Count - 1], navPath[i]);
                _currentAnalysisDistance += distanceBetweenCorners;

                actualPoints.Add(navPath[i]);

                if (_currentAnalysisDistance > analysisDistance)
                    return actualPoints;
            }
        }


        return actualPoints;
    }

    private void SetCarExpectedSpeed(float expectedSpeed)
    {
        if (Car.CarSpeed > expectedSpeed)
        {
            _brakingPedalForce += _brakingPedalForceDelta;
        }
        else
        {
            _brakingPedalForce -= _brakingPedalForceDelta;
        }

        Debug.LogError($"Current Speed: {Car.CarSpeed}; Expected speed: {expectedSpeed};");
        _brakingPedalForce = Mathf.Clamp(_brakingPedalForce, 0, 1f);
        Car.SetBrakeTorque(_brakingPedalForce);
    }

    private void DOBrakingByTarget()
    {
        float distanceToTarget = Vector2.Distance(transform.position, _finishBrakingPosition) - 3f;
        float expectedSpeed = _targetBrakingSpeedByTargetDistance.Evaluate(distanceToTarget);

        if (Car.CarSpeed > expectedSpeed)
        {
            Debug.LogError($"Brake with {_brakingPedalForce}; Current Speed: {Car.CarSpeed}; Expected speed: {expectedSpeed}; Brake more!");
            _brakingPedalForce += _brakingPedalForceDelta;
        }
        else
        {
            Debug.LogError($"Brake with {_brakingPedalForce}; Current Speed: {Car.CarSpeed}; Expected speed: {expectedSpeed}; Brake lower!");
            _brakingPedalForce -= _brakingPedalForceDelta;
        }

        _brakingPedalForce = Mathf.Clamp(_brakingPedalForce, 0, 1f);

        Car.SetBrakeTorque(_brakingPedalForce);
        Car.SetMotorTorque(0);
    }

    private float GetAngleToBetweenTransfors(Transform from, Vector3 to)
    {
        Vector3 deltaVec = from.InverseTransformPoint(to);
        return Mathf.Atan2(deltaVec.x, deltaVec.z) * Mathf.Rad2Deg;
    }
}
