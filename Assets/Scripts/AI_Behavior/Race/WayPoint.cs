using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    [SerializeField] private float _radius;

    public float Radius => _radius;

    [Header("New")]
    [SerializeField] private float _width = 2.5f;
    public float Width => _width;

    [Header("Gizmos")]
    [SerializeField] private Color _gateColor = Color.gray;
    [SerializeField] private float _gizmosScaleY = 2f;
    [SerializeField] private float _gizmosScaleZ = 0.1f;

    public Vector3 Center => new Vector3(transform.position.x, transform.position.y + _gizmosScaleY / 2, transform.position.z);
    public Vector3 LeftBorder => transform.TransformPoint(-_width / 2, _gizmosScaleY / 2, 0);
    public Vector3 RightBorder => transform.TransformPoint(_width / 2, _gizmosScaleY / 2, 0);


    private Vector3 _rightRotateControlPoint;
    public Vector3 RightRotateControlPoint
    {
        get => _rightRotateControlPoint;
        set
        {
            _rightRotateControlPoint = value;
            UpdateRotation();
        }
    }

    private void UpdateRotation()
    {
        Vector3 controlVector = transform.position - _rightRotateControlPoint;
            
        float angle = Vector3.Angle(controlVector, -transform.right);
        angle *= Vector3.Cross(controlVector, -transform.right).y > 0? 1: -1;
            
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y - angle, 0);
    }

    public Vector3 RacingPoint => transform.TransformPoint(_racingLinePoint * Width, _gizmosScaleY / 2, 0);
    public float LocalRacingPoint => _racingLinePoint;
    private float _racingLinePoint = 0;

    private RaceCircuit _circuit;
    public void SetCircuit(RaceCircuit circuit)
    {
        _circuit = circuit;
        _racingLinePoint = 0;

        ResetControlPoint();
    }

    public void ResetControlPoint()
    {        
        _rightRotateControlPoint = transform.position + transform.right * 5;
    }

    //Think about refactoring
    public Vector3 ConvertLocalPointToWorld(Vector3 localPoint)
    {
        return transform.TransformPoint(localPoint.x, _gizmosScaleY / 2 + localPoint.y, localPoint.z);
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = _gateColor;
        
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(new Vector3(0, _gizmosScaleY / 2, 0), new Vector3(_width, _gizmosScaleY, _gizmosScaleZ));
        
//        Gizmos.color = Color.green;
//        Gizmos.DrawRay(transform.position, transform.position + transform.right * 5 - transform.position);
//
//        Gizmos.color = Color.blue;
//        Gizmos.DrawRay(transform.position, _leftRotateControlPoint - transform.position);
    }

    public void SetRacingPoint(float racingPoint)
    {
        _racingLinePoint = Mathf.Clamp(racingPoint, -0.5f, 0.5f);
    }
}
