using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourAI;
using UnityEngine;

public class ObstacleAvoidanceController : MonoBehaviour
{
    [SerializeField] private DriverAI _driver;
    [SerializeField] private int _heatLineDimension = 50;
    [SerializeField] private float _heatLineLength = 10f;

    public float[] _heatLine;

    private Vector3 HeatLineLeftBorder => transform.position + transform.forward * _heatLineLength / 2;
    private Vector3 HeatLineRightBorder => transform.position - transform.forward * _heatLineLength / 2;
    
    private float SpaceBetweenHeatValues => _heatLineLength / _heatLineDimension;
    
    private readonly List<Collider> _observedObstacles = new List<Collider>();
    private readonly List<Vector2> _projectionPoints = new List<Vector2>();
    
    private int _obstacleColliderMask;
    
    private PathSegment DriverTargetSegment => (_driver.StateAI as RacingState)?.NextTargetPosition;

    public bool NeedAvoid => ObstacleAvoidIndex >= 0;
    public Vector3 FinalPosition => ConvertIndexToWorldPosition(ObstacleAvoidIndex);

    private Vector3 _gizmosOffset = new Vector3(0, 0.5f, 0f);
    
    private void Awake()
    {
        _obstacleColliderMask = LayerMask.GetMask("Car", "Obstacle");
        //_colliderToCheck = LayerMask.GetMask("Obstacle");
        
        _heatLine = new float[_heatLineDimension];
    }

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying)
            return;
        
        //Draw vector to next WP
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position + transform.up, 0.05f);
        
        Vector3 rayToTargetPosition = new Vector3(DriverTargetSegment.Waypoint.Center.x, 0, DriverTargetSegment.Waypoint.Center.z) - transform.position;
        Gizmos.DrawRay(transform.position + _gizmosOffset, rayToTargetPosition);
        
        //Draw heat line
        Gizmos.color = Color.red;
        
        Gizmos.DrawSphere(HeatLineLeftBorder, 0.1f);
        Gizmos.DrawSphere(HeatLineRightBorder, 0.1f);
        
        for (int i = 1; i < _heatLine.Length; i++)
        {
            Vector3 positionFrom = HeatLineLeftBorder - (transform.forward * SpaceBetweenHeatValues * (i - 1)) + (transform.right * _heatLine[i - 1]);
            Vector3 positionTo = HeatLineLeftBorder - (transform.forward * SpaceBetweenHeatValues * i) + (transform.right * _heatLine[i]);
            
            Gizmos.DrawLine(positionFrom + _gizmosOffset, positionTo + _gizmosOffset);
        }
        
        //Heat object
        // Gizmos.color = Color.blue;
        // Gizmos.DrawSphere(new Vector3(colliderPosOnLine.x, 1, colliderPosOnLine.y), 0.2f);
        //
        // Gizmos.color = Color.cyan;
        // foreach (var collider in _allColliders)
        // {
        //     Gizmos.DrawSphere(collider.bounds.max, 0.2f);
        //     Gizmos.DrawSphere(collider.bounds.min, 0.2f);
        // }

        Gizmos.color = Color.cyan;
        foreach (Vector2 value in _projectionPoints)
        {
            Vector3 v3 = ConvertFromXZ(value);
            Gizmos.DrawSphere(v3 + transform.up, 0.05f);
        }
        
        Gizmos.color = Color.yellow;
        
        Gizmos.DrawSphere(ConvertIndexToWorldPosition(ObstacleAvoidIndex), 0.2f);
    }

    private void LateUpdate()
    {
        UpdateTransform();
        ObstacleProjection();
        
        //Racing point projection
        Vector2 waypointPos = ConvertToXZ(DriverTargetSegment.CarRacingPoint);
        Vector2 waypointPosOnLine = _driver.Circuit.FindNearestPointOnLine(ConvertToXZ(HeatLineLeftBorder), ConvertToXZ(HeatLineRightBorder), waypointPos);

        _projectionPoints.Add(waypointPosOnLine);
        
        Vector3 rpPosition = ConvertFromXZ(waypointPosOnLine);
        rpPosition = transform.InverseTransformPoint(rpPosition);
        int racingPointIndex = ConvertPosOnLineToArrayIndex(rpPosition.z);

        
        //Ищем место в которое может поместится наш автомобиль
        int carNeededIndexCount = Mathf.CeilToInt(_driver.Car.CarSize.x / SpaceBetweenHeatValues);
        
        //Бежать влево и вправо и смотреть где ближе к точке будет
        int leftPoint = -1, rightPoint = -1;
        //Left check
        for (int pointIndex = racingPointIndex - carNeededIndexCount / 2; pointIndex < _heatLineDimension; pointIndex++)
        {
            if (CarFitsCheck(pointIndex, carNeededIndexCount))
            {
                leftPoint = pointIndex;
                break;
            }
        }
        
        //Right check
        for (int pointIndex = racingPointIndex - carNeededIndexCount / 2; pointIndex < _heatLineDimension; pointIndex--)
        {
            if (CarFitsCheck(pointIndex, carNeededIndexCount))
            {
                leftPoint = pointIndex;
                break;
            }
        }

        ObstacleAvoidIndex = carNeededIndexCount / 2 + Math.Abs(racingPointIndex - leftPoint) < Math.Abs(racingPointIndex - rightPoint)
            ? leftPoint
            : rightPoint;
        
    }

    private bool CarFitsCheck(int pointToCheck, int carWidth)
    {
        for (int i = pointToCheck; i < _heatLine.Length && i < pointToCheck + carWidth; i++)
        {
            if (_heatLine[i] != 0)
                return false;
        }

        return true;
    }

    private Vector3 ConvertIndexToWorldPosition(int index)
    {
        return HeatLineLeftBorder + transform.forward * (index * SpaceBetweenHeatValues) + transform.right * 15;
    }

    private int ObstacleAvoidIndex;

    private void UpdateTransform()
    {
        int driverCurrentWaypoint = (_driver.StateAI as RacingState).GetCurrentWaypointIndex;

        WayPoint wpFrom = _driver.Circuit.GetWaypointByIndex(driverCurrentWaypoint - 1);
        Vector2 lineStart = new Vector2(wpFrom.transform.position.x, wpFrom.transform.position.z);
        
        WayPoint wpTo = _driver.Circuit.GetWaypointByIndex(driverCurrentWaypoint);
        Vector2 lineFinish = new Vector2(wpTo.transform.position.x, wpTo.transform.position.z);

        Vector2 driverPos = new Vector2(_driver.Car.CarFrontBumperPos.x, _driver.Car.CarFrontBumperPos.z);
        
        Vector2 newPosV2 = _driver.Circuit.FindNearestPointOnLine(lineStart, lineFinish, driverPos);
        Vector3 newPosV3 = new Vector3(newPosV2.x, 0, newPosV2.y);

        transform.position = newPosV3;
        
        Vector3 vectorToNextWp = new Vector3(wpTo.transform.position.x, 0, wpTo.transform.position.z) - newPosV3;
        transform.rotation = Quaternion.LookRotation(vectorToNextWp,  Vector3.up);
        
        //TODO: REMOVE THIS SHIW
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y - 90, transform.eulerAngles.z);
    }

    private void ObstacleProjection()
    {
        for (int i = 0; i < _heatLine.Length; i++)
            _heatLine[i] = 0;
        
        _projectionPoints.Clear();
        
        foreach (Collider collider in _observedObstacles)
        {
            Vector2 posMin = _driver.Circuit.FindNearestPointOnLine(ConvertToXZ(HeatLineLeftBorder), ConvertToXZ(HeatLineRightBorder), ConvertToXZ(collider.bounds.min));
            Vector2 posMax = _driver.Circuit.FindNearestPointOnLine(ConvertToXZ(HeatLineLeftBorder), ConvertToXZ(HeatLineRightBorder), ConvertToXZ(collider.bounds.max));
            
            _projectionPoints.Add(posMin);
            _projectionPoints.Add(posMax);

            Vector3 posMin3 = ConvertFromXZ(posMin);
            Vector3 posMax3 = ConvertFromXZ(posMax);
            
            posMin3 = transform.InverseTransformPoint(posMin3);
            posMax3 = transform.InverseTransformPoint(posMax3);

            float OnLinePosMin = Mathf.Min(posMin3.z, posMax3.z);
            float OnLinePosMax = Mathf.Max(posMin3.z, posMax3.z);

            float pointMinPercentage = (OnLinePosMin + _heatLineLength) / (_heatLineLength * 2);
            float pointMaxPercentage = (OnLinePosMax + _heatLineLength) / (_heatLineLength * 2);

            int minIndex = Mathf.Clamp((int) (pointMinPercentage * _heatLineDimension), 0, _heatLineDimension - 1);
            int maxIndex = Mathf.Clamp((int) (pointMaxPercentage * _heatLineDimension), 0, _heatLineDimension - 1);
            
            for (int i = minIndex; i <= maxIndex; i++)
                _heatLine[i] = 0.5f;
        }   
    }

    private int ConvertPosOnLineToArrayIndex(float posZOnLine)
    {
        float pointMinPercentage = (posZOnLine + _heatLineLength) / (_heatLineLength * 2);
        return Mathf.Clamp((int) (pointMinPercentage * _heatLineDimension), 0, _heatLineDimension - 1);
    }

    private Vector2 ConvertToXZ(Vector3 vec)
    {
        return new Vector2(vec.x, vec.z);
    }

    private Vector3 ConvertFromXZ(Vector2 vec)
    {
        return new Vector3(vec.x, 0, vec.y);
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
