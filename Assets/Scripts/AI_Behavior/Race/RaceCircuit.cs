using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceCircuit : MonoBehaviour
{
    public List<WayPoint> Waypoints;

    [Header("Racing line")]
    [Range(-0.5f, 0.5f)]
    [SerializeField] private float _mainStartingLineRacingPoint = 0;

    [Tooltip("Percent from waypoint width")]
    [Range(0, 1)]
    [SerializeField] private float _racingLineGradientStep = 0.1f;
    
    public bool _enableShortestDistanceCalculations;
    public bool _enableSmallestAnglePriority;

    [SerializeField] private AnimationCurve _waypointDifficultyByAngle;
    [SerializeField] private AnimationCurve _waypointDifficultyByDistance;


    [Header("Debug: Road")]
    public bool IsClosed;

    public bool ShowRoadCenterLine;
    public bool ShowRoadBorder;
    
    [Header("Debug: Racing line")]
    [SerializeField] private bool ShowShortestRacingLine;
    [SerializeField] private bool ShowFinalRacingLine;

    [Header("Debug: Waypoints")]
    public bool EnableWaypointsHandles;
    
    [Header("Debug: Waypoints data")]
    public bool ShowWaypointsNames;
    public bool ShowWaypointsData;
    public bool ShowWaypointsDifficulty;
    public bool ShowWaypointsDirections;
    
    private const string WaypointFormatName = "Waypoint {0}";

    private enum GradientDirections
    {
        LEFT,
        RIGHT,
    };

    public WayPoint GetWaypointByIndex(int index)
    {
        return Waypoints[(index + Waypoints.Count) % Waypoints.Count];
    }

    private void OnDrawGizmosSelected()
    {
        DrawRacingLine();
    }
    
    private void DrawRacingLine()
    {
        for (int i = 0; i < Waypoints.Count; i++)
        {
            WayPoint wpFrom = Waypoints[i];
            WayPoint wpTo = (i != Waypoints.Count - 1) ? Waypoints[i + 1] : Waypoints[0];

            if (ShowShortestRacingLine)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(wpFrom.ShortestRacingPoint, wpTo.ShortestRacingPoint);
            }

            if (ShowFinalRacingLine)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(wpFrom.FinalRacingPoint, wpTo.FinalRacingPoint);

                if (wpFrom.OvertakeRacingPoint != wpFrom.FinalRacingPoint || wpTo.OvertakeRacingPoint != wpTo.FinalRacingPoint)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(wpFrom.OvertakeRacingPoint, wpTo.OvertakeRacingPoint);
                }
            }
        }
    }

    public void Bake()
    {
        Waypoints = GetComponentsInChildren<WayPoint>().ToList();
        
        //Bake elements and build shortest racing line
        for (int i = 0; i < Waypoints.Count; i++)
        {
            Waypoints[i].Bake();
            Waypoints[i].gameObject.name = string.Format(WaypointFormatName, i);
            
            if (i == 0)
            {
                Waypoints[i].SetShortestRacingPoint(_mainStartingLineRacingPoint);
                continue;
            }

            if (_enableShortestDistanceCalculations)
            {
                Waypoints[i].SetShortestRacingPoint(GetBestRacingPointByDistance(Waypoints[i - 1], Waypoints[i]));
            }
            else if (_enableSmallestAnglePriority)
            {
                WayPoint nextWp = GetWaypointByIndex(i + 1);
                WayPoint prevWp = GetWaypointByIndex(i - 1);

                float currentRp = GetBestRacingPointByAngle(Waypoints[i], prevWp, nextWp);
                Waypoints[i].SetShortestRacingPoint(currentRp);
            }
        }
        
        //Calculate waypoint angles & distance between them
        for (int i = 0; i < Waypoints.Count; i++)
        {
            Vector3 toPrevWaypointVector = Waypoints[i].ShortestRacingPoint - GetWaypointByIndex(i - 1).ShortestRacingPoint;
            Vector3 toNextWaypointVector = GetWaypointByIndex(i + 1).ShortestRacingPoint - Waypoints[i].ShortestRacingPoint;
            
            float currentWaypointAngle = Vector3.Angle(toPrevWaypointVector, toNextWaypointVector);
            float distanceToNextWp = Vector3.Distance(Waypoints[i].transform.position, GetWaypointByIndex(i + 1).transform.position);
            
            Waypoints[i].TurningAngle = Mathf.RoundToInt(currentWaypointAngle);
            Waypoints[i].DistanceToNextWaypoint = Mathf.RoundToInt(distanceToNextWp);
        }

        //Build final racing line by formula
        for (int i = 0; i < Waypoints.Count; i++)
        {
            WayPoint nextWp = GetWaypointByIndex(i + 1);
            WayPoint prevWp = GetWaypointByIndex(i - 1);

            nextWp.WaypointDifficulty = (float) Math.Round(_waypointDifficultyByAngle.Evaluate(nextWp.TurningAngle), 2);

            float toNextWpDistanceCoef = _waypointDifficultyByDistance.Evaluate(Waypoints[i].DistanceToNextWaypoint);
            float toPrevWpDistanceCoef = _waypointDifficultyByDistance.Evaluate(prevWp.DistanceToNextWaypoint);

            Waypoints[i].NextWaypointDifficulty = (float) Math.Round(nextWp.WaypointDifficulty * toNextWpDistanceCoef, 2);
            Waypoints[i].PrevWaypointDifficulty = (float) Math.Round(prevWp.WaypointDifficulty * toPrevWpDistanceCoef, 2);
            
            Waypoints[i].NextWpDirection = nextWp.LocalShortestRacingPoint * 2;
            Waypoints[i].PrevWpDirection = prevWp.LocalShortestRacingPoint * 2;
            
            float offset = (Waypoints[i].NextWaypointDifficulty * -Waypoints[i].NextWpDirection + Waypoints[i].PrevWaypointDifficulty * -Waypoints[i].PrevWpDirection)
                           * (1 - Waypoints[i].WaypointDifficulty);
            
            Waypoints[i].SetFinalRacingPoint(Waypoints[i].LocalShortestRacingPoint + offset);
        }
        
        //Build overtake line
        for (int i = 1; i < Waypoints.Count; i++)
        {
            WayPoint nextWp = GetWaypointByIndex(i + 1);
            WayPoint prevWp = GetWaypointByIndex(i - 1);

            if (prevWp.WaypointDifficulty < 0.01 && Waypoints[i].WaypointDifficulty > 0.45f)
            {
                Waypoints[i].SetFinalOvertakeRacingPoint(Waypoints[i].LocalFinalRacingPoint + 0.5f);
            }
        }
    }

    private float GetBestRacingPointByAngle(WayPoint current, WayPoint prev, WayPoint next)
    {
        GradientDirections gradientDirection = GradientDirections.LEFT;
        
        //Angle through current waypoint center (Racing point on init = center)
        float minAngle = Vector3.Angle(current.ShortestRacingPoint - prev.ShortestRacingPoint, next.ShortestRacingPoint - current.ShortestRacingPoint);
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
            Vector3 beforeWaypointVec = racingPointCandidateWorldPos - prev.ShortestRacingPoint;
            //Vector from current racing point candidate to next point
            Vector3 afterWaypointVec = next.ShortestRacingPoint - racingPointCandidateWorldPos;
            
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

    private float GetBestRacingPointByDistance(WayPoint from, WayPoint to)
    {
        GradientDirections gradientDirection = GradientDirections.LEFT;
        
        float minWaypointsDistance = Vector3.Distance(from.ShortestRacingPoint, to.Center);
        float currentRacingPoint = 0;
        
        //While next offset not cross the road border
        while(Mathf.Abs(currentRacingPoint) + _racingLineGradientStep <= 0.501f)
        {
            float racingPointCandidate = gradientDirection == GradientDirections.LEFT ?
                currentRacingPoint - _racingLineGradientStep 
                : currentRacingPoint + _racingLineGradientStep;

            Vector3 pointCandidateWorldPos = to.ConvertLocalPointToWorld(new Vector3(racingPointCandidate, 0, 0));
            float waypointsDistance = Vector3.Distance(from.ShortestRacingPoint, pointCandidateWorldPos);

            if (waypointsDistance < minWaypointsDistance)
            {
                minWaypointsDistance = waypointsDistance;
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

    public List<WayPoint> GetWaypointsInDistance(Vector3 startPosition, float analysisDistance, int currentWaypoint)
    {
        float distance = Vector3.Distance(startPosition, GetWaypointByIndex(currentWaypoint).transform.position);
        List<WayPoint> points = new List<WayPoint>();
        points.Add(GetWaypointByIndex(currentWaypoint));

        do
        {
            WayPoint currentPoint = GetWaypointByIndex(currentWaypoint);
            WayPoint nextPoint = GetWaypointByIndex(currentWaypoint + 1);
            currentWaypoint += 1; 
            
            distance += Vector3.Distance(currentPoint.transform.position, nextPoint.transform.position);

            points.Add(nextPoint);

        } while (distance < analysisDistance);

        return points;
    }

    public void SetPointPosition(int index, Vector3 newPos)
    {
        Waypoints[index].transform.position = new Vector3(newPos.x, Waypoints[index].transform.position.y, newPos.z);
        Waypoints[index].ResetControlPoint();
    }

    public void AddNewWaypoint(Vector3 position)
    {
        GameObject copyFrom = Waypoints[Waypoints.Count - 1].gameObject;
        GameObject newWaypoint = Instantiate(copyFrom, new Vector3(position.x, copyFrom.transform.position.y, position.z), copyFrom.transform.rotation, transform);
        newWaypoint.name = string.Format(WaypointFormatName, Waypoints.Count);
        
        Waypoints.Add(newWaypoint.GetComponent<WayPoint>());
    }

    public void InsertWaypoint(int insertIndex, Vector3 insertPosition)
    {
        GameObject copyFrom = Waypoints[Waypoints.Count - 1].gameObject;
        GameObject newWaypoint = Instantiate(copyFrom, new Vector3(insertPosition.x, copyFrom.transform.position.y, insertPosition.z), copyFrom.transform.rotation, transform);

        newWaypoint.transform.SetSiblingIndex(insertIndex);
        Waypoints.Insert(insertIndex, newWaypoint.GetComponent<WayPoint>());

        UpdateWaypointsIndex(insertIndex);
    }

    public void DeleteWaypoint(int deleteIndex)
    {
        DestroyImmediate(Waypoints[deleteIndex].gameObject);
        Waypoints.RemoveAt(deleteIndex);

        UpdateWaypointsIndex(deleteIndex);
    }

    private void UpdateWaypointsIndex(int fromIndex)
    {
        for (int i = fromIndex; i < Waypoints.Count; i++)
            Waypoints[i].name = string.Format(WaypointFormatName, i);
    }
}
