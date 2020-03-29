using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceCircuit : MonoBehaviour
{
    [SerializeField] private WayPoint[] _waypoints;

    [Header("Racing line")]
    [Range(-1, 1)]
    [SerializeField] private float _mainStartingLineRacingPoint = 0;

    [SerializeField] private float _shortDistancePriority = 0.75f;
    [SerializeField] private float _smallestAnglePriority = 0.25f;

    [SerializeField] private float _racingLinerGradientDescentStep = 0.2f;

    [Header("Gizmos : borders")]
    [SerializeField] private Color _trackCenterLineColor = Color.gray;
    [SerializeField] private Color _leftBorderColor = Color.gray;
    [SerializeField] private Color _rightBorderColor = Color.gray;

    [Header("Gizmos : racing line")]
    [SerializeField] private Color _racingLineColor = Color.red;

    [Header("Debug")]
    public bool _enableShortestDistanceCalculations;
    public bool _enableSmallestAnglePriority;

    private enum GradientDirections
    {
        LEFT,
        RIGHT,
    };


    private void OnDrawGizmosSelected()
    {
        DrawCircuit();
    }

    public void Bake()
    {
        _waypoints = GetComponentsInChildren<WayPoint>();

        foreach (WayPoint wp in _waypoints)
            wp.SetCircuit(this);

        //_waypoints[0].SetRacingPoint(_mainStartingLineStartRacingPoint * _waypoints[0].Width / 2);
        //_waypoints[1].SetRacingPoint(_mainStartingLineFinishRacingPoint * _waypoints[1].Width / 2);

        for (int i = 0; i < _waypoints.Length; i++)
        {
            if (i == 0)
            {
                _waypoints[i].SetRacingPoint(_mainStartingLineRacingPoint * _waypoints[i].Width / 2);
                continue;
            }

            if(_enableShortestDistanceCalculations)
                _waypoints[i].SetRacingPoint(GetRacingPointBetweenWaypoints(_waypoints[i - 1], _waypoints[i]));
            else if (_enableSmallestAnglePriority)
            {
                _waypoints[i].SetRacingPoint(GetRacingPointBetweenWaypointsAngle(_waypoints[i - 1], _waypoints[i]));
            }
        }
    }

    private void DrawCircuit()
    {
        for (int i = 0; i < _waypoints.Length; i++)
        {
            WayPoint wpFrom = _waypoints[i];
            WayPoint wpTo = (i != _waypoints.Length - 1) ? _waypoints[i + 1] : _waypoints[0];

            Gizmos.color = _trackCenterLineColor;
            Gizmos.DrawLine(wpFrom.Center, wpTo.Center);

            Gizmos.color = _leftBorderColor;
            Gizmos.DrawLine(wpFrom.LeftBorder, wpTo.LeftBorder);

            Gizmos.color = _rightBorderColor;
            Gizmos.DrawLine(wpFrom.RightBorder, wpTo.RightBorder);

            Gizmos.color = _racingLineColor;
            Gizmos.DrawLine(wpFrom.RacingPoint, wpTo.RacingPoint);
        }

        //Gizmos.color = _racingLineColor;
        //Gizmos.DrawLine(_waypoints[0].RacingPoint, _waypoints[1].RacingPoint);
        //for (int i = 2; i < _waypoints.Length; i++)
        //{
        //    WayPoint wpFrom = _waypoints[i - 1];
        //    WayPoint wpTo = _waypoints[i];
        //    WayPoint wpPrev = _waypoints[i - 2];

        //    if (_enableSmallestAnglePriority)
        //    {
        //        Gizmos.color = _racingLineColor;
        //        _waypoints[i].SetRacingPoint(GetRacingPointBetweenWaypointsAngle(wpFrom, wpTo, wpPrev));
        //        Gizmos.DrawLine(wpFrom.RacingPoint, wpTo.RacingPoint);
        //    }
        //}

        //Gizmos.color = _racingLineColor;
        //Gizmos.DrawLine(_waypoints[0].RacingPoint, _waypoints[1].RacingPoint);

        //_waypoints[2].SetRacingPoint(GetRacingPointBetweenWaypointsAngle(_waypoints[1], _waypoints[2], _waypoints[0]));
        //Gizmos.DrawRay(_waypoints[1].RacingPoint, _waypoints[1].RacingPoint - _waypoints[0].RacingPoint);


    }

    private float GetRacingPointBetweenWaypointsAngle(WayPoint from, WayPoint to, WayPoint prev)
    {
        Vector3 prevVector = from.RacingPoint - prev.RacingPoint;
        bool isMoveToLeft = true;

        float currentRacingPoint = 0;

        Vector3 probRacingPointWorldPos = to.Center;
        Vector3 probRacingVector = probRacingPointWorldPos - from.RacingPoint;

        float minAngle = Vector3.Angle(prevVector, probRacingVector);

        for (int i = 0; i < 10; i++)
        {
            float probRacingPoint = isMoveToLeft ? 
                currentRacingPoint - _racingLinerGradientDescentStep 
                : currentRacingPoint + _racingLinerGradientDescentStep;

            probRacingPointWorldPos = to.ConvertLocalPointToWorld(new Vector3(probRacingPoint, 0, 0));
            probRacingVector = probRacingPointWorldPos - from.RacingPoint;

            float probAngle = Vector3.Angle(prevVector, probRacingVector);

            if (probAngle < minAngle)
            {
                minAngle = probAngle;
                currentRacingPoint = probRacingPoint;

            }
            else if (i == 0)
            {
                isMoveToLeft = false;
            }
        }

        Gizmos.DrawRay(from.RacingPoint, probRacingVector);
        return currentRacingPoint;
    }

    private float GetRacingPointBetweenWaypoints(WayPoint from, WayPoint to)
    {
        GradientDirections gradientDirection = GradientDirections.LEFT;
        bool canChangeGradientDirection = true;

        float currentRacingPoint = 0;
        float minWaypointsDistance = Vector3.Distance(from.RacingPoint, to.Center);

        while(Mathf.Abs(currentRacingPoint) + _racingLinerGradientDescentStep < to.Width / 2)
        {
            float racingPointCandidate = gradientDirection == GradientDirections.LEFT ?
                currentRacingPoint - _racingLinerGradientDescentStep 
                : currentRacingPoint + _racingLinerGradientDescentStep;

            Vector3 pointCandidateWorldPos = to.ConvertLocalPointToWorld(new Vector3(racingPointCandidate, 0, 0));
            float waypointsDistance = Vector3.Distance(from.RacingPoint, pointCandidateWorldPos);

            if (waypointsDistance < minWaypointsDistance)
            {
                minWaypointsDistance = waypointsDistance;
                currentRacingPoint = racingPointCandidate;

            }
            else
            {
                if (canChangeGradientDirection)
                    gradientDirection = GradientDirections.RIGHT;
                else
                    return currentRacingPoint;

                canChangeGradientDirection = false;
            }
        }

        return currentRacingPoint;
    }

    public WayPoint GetPoint(int index)
    {
        return _waypoints[index];
    }

    public int GetNextPoint(int point)
    {
        if (point < _waypoints.Length - 1)
            return point + 1;
        else
            return 0;
    }


    public List<WayPoint> GetWaypointsInDistance(Vector3 startPosition, float analysisDistance, int currentWaypoint)
    {
        float distance = Vector3.Distance(startPosition, GetPoint(currentWaypoint).transform.position);
        List<WayPoint> points = new List<WayPoint>();
        points.Add(GetPoint(currentWaypoint));

        do
        {
            WayPoint currentPoint = GetPoint(currentWaypoint);

            currentWaypoint = GetNextPoint(currentWaypoint);
            WayPoint nextPoint = _waypoints[currentWaypoint];

            distance += Vector3.Distance(currentPoint.transform.position, nextPoint.transform.position);

            points.Add(nextPoint);

        } while (distance < analysisDistance);

        return points;
    }
}
