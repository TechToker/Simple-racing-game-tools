using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private float _width = 2.5f;
    public float Width => _width;

    [Header("Gizmos")]
    [SerializeField] private Color _gateColor = Color.gray;
    [SerializeField] private float _circlePosY = 0.25f;

    public Vector3 Center => new Vector3(transform.position.x, transform.position.y + _circlePosY, transform.position.z);
    public Vector3 LeftBorder => transform.TransformPoint(-_width / 2, _circlePosY, 0);
    public Vector3 RightBorder => transform.TransformPoint(_width / 2, _circlePosY, 0);
    
    //Racing points
    [HideInInspector] [SerializeField] private float _shortestRacingLinePoint;
    public float LocalShortestRacingPoint => _shortestRacingLinePoint;
    public Vector3 ShortestRacingPoint { get; private set; }

    [HideInInspector] [SerializeField] private float _finalRacingPoint;
    public float LocalFinalRacingPoint => _finalRacingPoint;
    public Vector3 FinalRacingPoint { get; private set; }
    
    //Overtake racing point
    [HideInInspector] [SerializeField] private float _overtakeRacingPoint;
    public float LocalOvertakeRacingPoint => _overtakeRacingPoint;
    public Vector3 OvertakeRacingPoint { get; private set; }

    //Gizmos fields
    [HideInInspector] public float TurningAngle;
    [HideInInspector] public float DistanceToNextWaypoint;
    
    [HideInInspector] public float WaypointDifficulty;
    [HideInInspector] public float NextWaypointDifficulty;
    [HideInInspector] public float PrevWaypointDifficulty;

    [HideInInspector] public float PrevWpDirection;
    [HideInInspector] public float NextWpDirection;
    
    [HideInInspector] [SerializeField] private Vector3 _rightRotateControlPoint;
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
    
    public void Bake()
    {
        _shortestRacingLinePoint = 0;
        _finalRacingPoint = 0;

        TurningAngle = 0;
        DistanceToNextWaypoint = 0;

        WaypointDifficulty = 0;
        NextWaypointDifficulty = 0;
        PrevWaypointDifficulty = 0;

        PrevWpDirection = 0;
        NextWpDirection = 0;

        ResetControlPoint();
    }

    public void ResetControlPoint()
    {        
        _rightRotateControlPoint = transform.position + transform.right * _width / 2;
    }

    //TODO: Think about refactoring
    public Vector3 ConvertLocalPointToWorld(Vector3 localPoint)
    {
        return transform.TransformPoint(localPoint.x, _circlePosY + localPoint.y, localPoint.z);
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = _gateColor;
        Gizmos.DrawLine(LeftBorder, RightBorder);
    }

    public void SetShortestRacingPoint(float racingPoint)
    {
        _shortestRacingLinePoint = Mathf.Clamp(racingPoint, -0.5f, 0.5f);
        ShortestRacingPoint = transform.TransformPoint(_shortestRacingLinePoint * Width, _circlePosY, 0);
    }

    public void SetFinalRacingPoint(float racingPoint)
    {
        _finalRacingPoint = Mathf.Clamp(racingPoint, -0.5f, 0.5f);
        FinalRacingPoint = transform.TransformPoint(_finalRacingPoint * Width, _circlePosY, 0);
        
        //Set overtake line = racing line as default 
        _overtakeRacingPoint = _finalRacingPoint;
        OvertakeRacingPoint = FinalRacingPoint;
    }
    
    public void SetFinalOvertakeRacingPoint(float racingPoint)
    {
        _overtakeRacingPoint = Mathf.Clamp(racingPoint, -0.5f, 0.5f);
        OvertakeRacingPoint = transform.TransformPoint(_overtakeRacingPoint * Width, _circlePosY, 0);
    }
}
