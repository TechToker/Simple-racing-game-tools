﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceCircuit : MonoBehaviour
{
    [SerializeField] private WayPoint[] _waypoints;

    [Header("Racing line")]
    [Range(-1, 1)]
    [SerializeField] private float _mainStartingLineRacingPoint = 0;
    
    [Tooltip("Percent from waypoint width")]
    [Range(0, 1)]
    [SerializeField] private float _racingLineGradientStep = 0.1f;

    [Header("Gizmos : borders")]
    [SerializeField] private Color _trackCenterLineColor = Color.gray;
    [SerializeField] private Color _leftBorderColor = Color.gray;
    [SerializeField] private Color _rightBorderColor = Color.gray;

    [Header("Gizmos : racing line")]
    [SerializeField] private Color _racingLineColor = Color.red;

    [Header("Debug")]
    [SerializeField] private bool DrawGizmo;
    public bool _enableShortestDistanceCalculations;
    public bool _enableSmallestAnglePriority;

    private enum GradientDirections
    {
        LEFT,
        RIGHT,
    };


    private void OnDrawGizmos()
    {
        if(!DrawGizmo)
            return;
        
        DrawCircuit();
    }

    public void Bake()
    {
        _waypoints = GetComponentsInChildren<WayPoint>();

        foreach (WayPoint wp in _waypoints)
            wp.SetCircuit(this);
        
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
                WayPoint nextWp = _waypoints[(i + 1) % _waypoints.Length];
                WayPoint prevWp = _waypoints[(i - 1 + _waypoints.Length) % _waypoints.Length];

                float currentRp = GetBestRacingPointByAngle(_waypoints[i], prevWp, nextWp);
                
//                nextWp.SetRacingPoint(nextWp.LocalRacingPoint - currentRp * 0.5f);
//                prevWp.SetRacingPoint(prevWp.LocalRacingPoint - currentRp * 0.5f);
                
                _waypoints[i].SetRacingPoint(currentRp);
            }
        }
    }

    private void DrawCircuit()
    {
//        for (int i = 0; i < 2; i++)
//        {
//            Gizmos.color = _racingLineColor;
//            Gizmos.DrawLine(_waypoints[i].RacingPoint, _waypoints[i + 1].RacingPoint);
//        }
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(_waypoints[0].Center, _waypoints[1].Center - _waypoints[0].Center);
        Gizmos.DrawRay(_waypoints[1].Center, _waypoints[2].Center - _waypoints[1].Center);
        
        for (int i = 0; i < _waypoints.Length; i++)
        {
            WayPoint wpFrom = _waypoints[i];
            WayPoint wpTo = (i != _waypoints.Length - 1) ? _waypoints[i + 1] : _waypoints[0];

//            Gizmos.color = _trackCenterLineColor;
//            Gizmos.DrawLine(wpFrom.Center, wpTo.Center);

            Gizmos.color = _leftBorderColor;
            Gizmos.DrawLine(wpFrom.LeftBorder, wpTo.LeftBorder);

            Gizmos.color = _rightBorderColor;
            Gizmos.DrawLine(wpFrom.RightBorder, wpTo.RightBorder);

            Gizmos.color = _racingLineColor;
            Gizmos.DrawLine(wpFrom.RacingPoint, wpTo.RacingPoint);
        }
    }

    private float GetBestRacingPointByAngle(WayPoint current, WayPoint prev, WayPoint next)
    {
        GradientDirections gradientDirection = GradientDirections.LEFT;
        
        //Angle through current waypoint center (Racing point on init = center)
        float minAngle = Vector3.Angle(current.RacingPoint - prev.RacingPoint, next.RacingPoint - current.RacingPoint);
        float currentRacingPoint = 0;

        //While next offset not cross the road border
        while(Mathf.Abs(currentRacingPoint) + _racingLineGradientStep <= 0.501f)
        {
            //Move racing point left or right
            float racingPointCandidate = gradientDirection == GradientDirections.LEFT ?
                currentRacingPoint - _racingLineGradientStep
                : currentRacingPoint + _racingLineGradientStep;

            Vector3 racingPointCandidateWorldPos = current.ConvertLocalPointToWorld(new Vector3(racingPointCandidate * current.Width, 0, 0));
            
            //Vector from prev point to current racing point candidate
            Vector3 beforeWaypointVec = racingPointCandidateWorldPos - prev.RacingPoint;
            //Vector from current racing point candidate to next point
            Vector3 afterWaypointVec = next.RacingPoint - racingPointCandidateWorldPos;
            
            //Calculate angle between vectors
            float minAngleCandidate = Vector3.Angle(beforeWaypointVec, afterWaypointVec);

            if (minAngleCandidate < minAngle)
            {
                minAngle = minAngleCandidate;
                currentRacingPoint = racingPointCandidate;
            }
            else
            {
                //Move racing point next candidate to right only if before we not move to left
                if (Math.Abs(currentRacingPoint) < float.Epsilon && gradientDirection != GradientDirections.RIGHT)
                    gradientDirection = GradientDirections.RIGHT;
                else
                    break;
            }
        }

        return currentRacingPoint;
    }

    private float GetRacingPointBetweenWaypoints(WayPoint from, WayPoint to)
    {
        GradientDirections gradientDirection = GradientDirections.LEFT;
        bool canChangeGradientDirection = true;

        float currentRacingPoint = 0;
        float minWaypointsDistance = Vector3.Distance(from.RacingPoint, to.Center);

        while(Mathf.Abs(currentRacingPoint) + _racingLineGradientStep < to.Width / 2)
        {
            float racingPointCandidate = gradientDirection == GradientDirections.LEFT ?
                currentRacingPoint - _racingLineGradientStep 
                : currentRacingPoint + _racingLineGradientStep;

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