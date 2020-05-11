using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourAI;
using UnityEngine;

public class ObstacleAvoidanceController : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool _enableGizmos;
    
    [Header("Main")]
    [SerializeField] private DriverAI _driver;
    [SerializeField] private int _heatLineDimension = 50;
    [SerializeField] private float _heatLineLength = 10f;

    public float[] _heatLine;

    private Vector3 HeatLineLeftBorder => transform.position - transform.right * _heatLineLength / 2;
    private Vector3 HeatLineRightBorder => transform.position + transform.right * _heatLineLength / 2;
    
    private float SpaceBetweenHeatValues => _heatLineLength / _heatLineDimension;
    
    private readonly List<Collider> _observedObstacles = new List<Collider>();
    private readonly List<Vector3> _projectionPoints = new List<Vector3>();
    
    private int _obstacleColliderMask;
    
    private PathSegment DriverTargetSegment => (_driver.StateAI as RacingState)?.NextTargetPosition;

    public bool NeedAvoid => _obstacleAvoidIndex >= 0;
    private int _obstacleAvoidIndex;
    public Vector3 ObstacleAvoidPosition => ConvertIndexToWorldPosition(_obstacleAvoidIndex) + transform.up;

    private Vector3 _gizmosOffset = new Vector3(0, 0.5f, 0f);

    private void Awake()
    {
        _obstacleColliderMask = LayerMask.GetMask("Car", "Obstacle");
        //_colliderToCheck = LayerMask.GetMask("Obstacle");
        
        _heatLine = new float[_heatLineDimension];
    }

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying || !_enableGizmos)
            return;

        DrawRayToNextWaypoint();
        DrawHeatLineGizmos();
        
        //DrawProjectionPoints();
        //DrawObstacleProjectionPoints();
        
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawSphere(ConvertIndexToWorldPosition(ObstacleAvoidIndex), 0.2f);
    }

    private void DrawRayToNextWaypoint()
    {
        //Draw vector to next WP
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + _gizmosOffset, 0.05f);
        
        Gizmos.color = new Color(0.75f, 0f, 0f);
        Vector3 rayToTargetPosition = new Vector3(DriverTargetSegment.Waypoint.Center.x, 0, DriverTargetSegment.Waypoint.Center.z) - transform.position;
        Gizmos.DrawRay(transform.position + _gizmosOffset, rayToTargetPosition);
    }

    private void DrawHeatLineGizmos()
    {
        //Draw heat line
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(HeatLineLeftBorder + _gizmosOffset, 0.05f);
        
        Gizmos.DrawSphere(HeatLineRightBorder + _gizmosOffset, 0.05f);
        
        for (int i = 1; i < _heatLine.Length; i++)
        {
            Vector3 prevHeatValuePos = ConvertIndexToWorldPosition(i - 1) + (transform.forward * _heatLine[i - 1]);
            Vector3 currentHeatValuePos = ConvertIndexToWorldPosition(i) + (transform.forward * _heatLine[i]);
            
            Gizmos.DrawLine(prevHeatValuePos + _gizmosOffset, currentHeatValuePos + _gizmosOffset);
        }
    }

    private void DrawObstacleColliderPoints()
    {
        Gizmos.color = Color.cyan;
        foreach (Collider colider in _observedObstacles)
        {
            Gizmos.DrawSphere(colider.bounds.max, 0.2f);
            Gizmos.DrawSphere(colider.bounds.min, 0.2f);
        }
    }

    private void DrawObstacleProjectionPoints()
    {
        Gizmos.color = Color.cyan;
        foreach (Vector3 value in _projectionPoints)
            Gizmos.DrawSphere(value + _gizmosOffset, 0.05f);
    }

    private void LateUpdate()
    {
        UpdateTransform();
        WriteObstacleToHeatLine();
        CalculateObstacleAvoidancePoint();
    }

    private void UpdateTransform()
    {
        int currentTargetWaypoint = (_driver.StateAI as RacingState).GetCurrentWaypointIndex;
        
        Vector2 prevWaypointPos = MathfExtensions.ConvertToXZ(_driver.Circuit.GetWaypointByIndex(currentTargetWaypoint - 1).transform.position);
        Vector2 targetWaypointPos = MathfExtensions.ConvertToXZ(_driver.Circuit.GetWaypointByIndex(currentTargetWaypoint).transform.position);

        Vector2 carPos = MathfExtensions.ConvertToXZ(_driver.Car.CarFrontBumperPos);
        Vector3 carPosOnLine = MathfExtensions.ConvertFromXZ(MathfExtensions.FindNearestPointOnLine(prevWaypointPos, targetWaypointPos, carPos));

        Vector3 vectorToNextWp = MathfExtensions.ConvertFromXZ(targetWaypointPos) - carPosOnLine;
        
        transform.position = carPosOnLine;
        transform.rotation = Quaternion.LookRotation(vectorToNextWp,  Vector3.up);
    }
    
    private void WriteObstacleToHeatLine()
    {
        for (int i = 0; i < _heatLine.Length; i++)
            _heatLine[i] = 0;
        
        _projectionPoints.Clear();

        Vector2 heatLineLeftBorderXZ = MathfExtensions.ConvertToXZ(HeatLineLeftBorder);
        Vector2 heatLineRightBorderXZ = MathfExtensions.ConvertToXZ(HeatLineRightBorder);
        
        foreach (Collider obstacle in _observedObstacles)
        {
            Vector3 obstacleMinPosOnLine = MathfExtensions.FindNearestPointOnLine(heatLineLeftBorderXZ, heatLineRightBorderXZ, MathfExtensions.ConvertToXZ(obstacle.bounds.min));
            Vector3 obstacleMaxPosOnLine = MathfExtensions.FindNearestPointOnLine(heatLineLeftBorderXZ, heatLineRightBorderXZ, MathfExtensions.ConvertToXZ(obstacle.bounds.max));
            
            obstacleMinPosOnLine = MathfExtensions.ConvertFromXZ(obstacleMinPosOnLine);
            obstacleMaxPosOnLine = MathfExtensions.ConvertFromXZ(obstacleMaxPosOnLine);
            
            _projectionPoints.Add(obstacleMinPosOnLine);
            _projectionPoints.Add(obstacleMaxPosOnLine);
            
            obstacleMinPosOnLine = transform.InverseTransformPoint(obstacleMinPosOnLine);
            obstacleMaxPosOnLine = transform.InverseTransformPoint(obstacleMaxPosOnLine);
            
            int obstacleMinIndex = ConvertPosOnLineToArrayIndex(obstacleMinPosOnLine.x);
            int obstacleMaxIndex = ConvertPosOnLineToArrayIndex(obstacleMaxPosOnLine.x);
            
            //Obstacle bounds min & max does not mean that the min index is less than max index
            for (int i = Mathf.Min(obstacleMinIndex, obstacleMaxIndex); i <=  Mathf.Max(obstacleMinIndex, obstacleMaxIndex); i++)
                _heatLine[i] = 1f;
        }   
    }
    
    //Z-index range [-heatLineLength / 2; heatLineLength / 2]
    private int ConvertPosOnLineToArrayIndex(float zLinePos)
    {
        float zPosInPercent = Mathf.InverseLerp(-_heatLineLength / 2, _heatLineLength / 2, zLinePos);
        return Mathf.Clamp(Mathf.RoundToInt(zPosInPercent * _heatLineDimension), 0, _heatLineDimension - 1);
    }

    private Vector3 ProjectPositionOnLine(Vector3 position)
    {
        Vector2 position2D = MathfExtensions.ConvertToXZ(position);
        return MathfExtensions.ConvertFromXZ(MathfExtensions.FindNearestPointOnLine(
            MathfExtensions.ConvertToXZ(HeatLineLeftBorder), MathfExtensions.ConvertToXZ(HeatLineRightBorder), position2D));
    }
    
    private Vector3 ConvertIndexToWorldPosition(int index)
    {
        return HeatLineLeftBorder + transform.right * (index * SpaceBetweenHeatValues);
    }

    private void CalculateObstacleAvoidancePoint()
    {
        Vector3 projectionWorldPosition = ProjectPositionOnLine(DriverTargetSegment.CarRacingPoint);
        _projectionPoints.Add(projectionWorldPosition);

        projectionWorldPosition = transform.InverseTransformPoint(projectionWorldPosition);
        int racingPointIndex = ConvertPosOnLineToArrayIndex(projectionWorldPosition.x);
        
        //Make needed space even number
        int carNeededIndexesWidth = Mathf.CeilToInt(_driver.Car.CarSize.x / SpaceBetweenHeatValues / 2) * 2;

        int leftSpaceIndex = GetCarFreeSpaceIndex(racingPointIndex, carNeededIndexesWidth, Sides.LEFT);
        int rightSpaceIndex = GetCarFreeSpaceIndex(racingPointIndex, carNeededIndexesWidth, Sides.RIGHT);

        int nearestAvoidIndex = leftSpaceIndex >= 0 && Math.Abs(racingPointIndex - leftSpaceIndex) < Math.Abs(racingPointIndex - rightSpaceIndex)
                ? leftSpaceIndex
                : rightSpaceIndex;
        
        _obstacleAvoidIndex = carNeededIndexesWidth / 2 + nearestAvoidIndex;
    }

    private int GetCarFreeSpaceIndex(int startFindIndex, int neededSpace, Sides moveSide)
    {
        //Find starts from left side of car
        //TODO: Find start side car may be depended from 'moveSize'; To minimize for-cycles
        
        int movingDelta = moveSide == Sides.LEFT ? -1: 1;
        for (int index = startFindIndex; moveSide == Sides.LEFT ? index >= 0: index < _heatLineDimension - neededSpace / 2; index += movingDelta)
        {
            if (IsCarFits(index, neededSpace))
                return index;
        }

        return -1;
    }

    private bool IsCarFits(int pointToCheck, int neededSpace)
    {
        for (int index = pointToCheck; index < pointToCheck + neededSpace && index < _heatLine.Length; index++)
        {
            if (_heatLine[index] > float.Epsilon)
                return false;
        }

        return true;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (_obstacleColliderMask == (_obstacleColliderMask | (1 << other.gameObject.layer)) && other != _driver.Car.MainCollider)
        {
            Debug.LogError($"Enter: {other.gameObject.name}");
            _observedObstacles.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_obstacleColliderMask == (_obstacleColliderMask | (1 << other.gameObject.layer)) && other != _driver.Car.MainCollider)
        {
            Debug.LogError($"Exit: {other.gameObject.name}");
            _observedObstacles.Remove(other);
        }
    }
}
