using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CarAI : BaseCar
{
    [Header("AI params")]
    [SerializeField] private Transform _taget;
    [SerializeField] private float _targetPosUpdateTime = 8;

    public float CheckRoadDistance = 3f;
    public bool _roadExist;

    private NavMeshPath _navMeshPath;

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

    }

    private void CheckRoadInFront()
    {
        _roadExist = !NavMesh.Raycast(transform.position, transform.position + transform.forward * CheckRoadDistance, out NavMeshHit hit, NavMesh.AllAreas);
    }

    public float BrakingByTargetMultyply = 1;
    public float BrakingAgression = 0.6f;

    protected override void Update()
    {
        if (_navMeshPath == null || _navMeshPath.corners.Length == 0)
            return;

        RotateWheelsToTarget();

        //float sum = 0;
        //foreach(WheelCollider wc in _allWheels)
        //{
        //    WheelHit hit;
        //    wc.GetGroundHit(out hit);

        //    sum += hit.
        //}

        //Debug.LogError($"{_allWheels[0].rpm}; {_allWheels[0].motorTorque / _allWheels[0].radius / _allWheels[0].mass}");
        //StopAroundTarget(_navMeshPath.corners[_navMeshPath.corners.Length - 1]);

        base.Update();
    }

    public bool AmBraking = false;

    private void StopAroundTarget(Vector3 target)
    {
        if (!AmBraking)
        {
            SetMotorTorque(_torqueBySpeed.Evaluate(CarSpeed));
            SetBrakeTorque(0, 0);
        }

        float distanceToTarget = Vector3.Distance(target, transform.position);
        float aiStartBrakingDistance = CarSpeed * BrakingByTargetMultyply;

        Debug.Log($"Distance: {distanceToTarget}; startBrakingDistance: {aiStartBrakingDistance}");

        if (distanceToTarget < aiStartBrakingDistance)
        {
            Debug.LogError($"Start brake from: {aiStartBrakingDistance}; Speed: {CarSpeed}");

            float SpeedAtStartBraking = CarSpeed;
            AmBraking = true;
        }

        if(AmBraking)
        {
            Debug.LogError($"Brake force: {BrakingAgression}");
            SetBrakeTorque(_brakeTorque * BrakingAgression, 0);
        }
    }

    private void RotateWheelsToTarget()
    {
        float maxPossibleAngle = _turningAngleBySpeed.Evaluate(CarSpeed);
        float turningAngle = Mathf.Clamp(GetAngleToTarget(), -maxPossibleAngle, maxPossibleAngle);
        SetSteerAngle(turningAngle);
    }

    private float GetAngleToTarget()
    {
        Vector3 deltaVec = transform.InverseTransformPoint(_navMeshPath.corners[1]);
        return Mathf.Atan2(deltaVec.x, deltaVec.z) * Mathf.Rad2Deg;
    }
}
