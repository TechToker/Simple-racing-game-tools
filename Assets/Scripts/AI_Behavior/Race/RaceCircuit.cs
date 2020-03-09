using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceCircuit : MonoBehaviour
{
    [SerializeField] private WayPoint[] _waypoints;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        for(int i = 0; i < _waypoints.Length; i++)
        {
            if (i != _waypoints.Length - 1)
                Gizmos.DrawLine(_waypoints[i].transform.position, _waypoints[i + 1].transform.position);
            else
                Gizmos.DrawLine(_waypoints[i].transform.position, _waypoints[0].transform.position);
        }
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
