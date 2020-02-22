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

    //MODE: AUTOBRAKING
    public Vector3 _finishBrakingPosition;

    public float _brakingPedalForce = 0.1f;
    public float _brakingPedalForceDelta = 0.05f;

    private void OnEnable()
    {
        _navMeshPath = new NavMeshPath();
        StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        yield return new WaitForSeconds(_targetPosUpdateTime);

        NavMesh.CalculatePath(transform.position, _taget.transform.position, NavMesh.AllAreas, _navMeshPath);
        CheckRoadInFront();

        StartCoroutine(MoveToTarget());
    }

    private void OnDrawGizmos()
    {
        if (_navMeshPath == null)
            return;

        Gizmos.color = Color.magenta;
        Vector3[] currentPath = _navMeshPath.corners;

        for (int i = 0; i < currentPath.Length - 1; i++)
        {
            Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
        }

        //Draw road raycast in front
        Gizmos.color = _roadExist ? Color.white : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * CheckRoadDistance);

        Gizmos.color = Color.yellow;

        if (_finishBrakingPosition != Vector3.zero)
            Gizmos.DrawLine(transform.position, _finishBrakingPosition);
    }

    private void CheckRoadInFront()
    {
        _roadExist = !NavMesh.Raycast(transform.position, transform.position + transform.forward * CheckRoadDistance, out NavMeshHit hit, NavMesh.AllAreas);
    }

    protected void Update()
    {
        if (_navMeshPath == null || _navMeshPath.corners.Length == 0)
            return;

        if (Input.GetKeyDown(KeyCode.B))
            ActivateBrakeAroundTarget();
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

    private void DOChase()
    {
        Car.SetMotorTorque(0.1f);

        if (_navMeshPath.corners.Length > 0)
            Car.SetSteerAngle(GetAngleToTarget());
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

    private float GetAngleToTarget()
    {
        Vector3 deltaVec = transform.InverseTransformPoint(_navMeshPath.corners[1]);
        return Mathf.Atan2(deltaVec.x, deltaVec.z) * Mathf.Rad2Deg;
    }
}
