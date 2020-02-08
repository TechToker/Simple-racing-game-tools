using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CarAI : BaseCar
{
    [SerializeField] private Transform _taget;
    [SerializeField] private float _targetPosUpdateTime = 8;

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
        StartCoroutine(MoveToTarget());
    }

    private void OnDrawGizmos()
    {
        if (_navMeshPath == null)
            return;

        Gizmos.color = Color.red;
        Vector3[] currentPath = _navMeshPath.corners;

        for(int i = 0; i < currentPath.Length - 1; i++)
        {
            Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
        }

    }

    protected override void Update()
    {
        float turningAngle = 0;

        if (_navMeshPath != null && _navMeshPath.corners.Length > 1)
        {
            Vector3 deltaVec = transform.InverseTransformPoint(_navMeshPath.corners[1]);

            float maxPossibleAngle = _turningAngleBySpeed.Evaluate(CarSpeed);
            turningAngle = Mathf.Clamp(Mathf.Atan2(deltaVec.x, deltaVec.z) * Mathf.Rad2Deg, -maxPossibleAngle, maxPossibleAngle);

            //Debug.Log(turningAngle);
        }

        SetSteerAngle(turningAngle);
        SetMotorTorque(150);

        base.Update();
    }
}
