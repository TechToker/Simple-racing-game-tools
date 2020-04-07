using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SplinePath
{

    [SerializeField, HideInInspector]
    private List<SplinePathPoint> points;
    
    
    public SplinePath(Vector2 centre)
    {
        points = new List<SplinePathPoint>
        {
            new SplinePathPoint(centre + Vector2.left, null, centre + (Vector2.left+Vector2.up)*.5f),
            new SplinePathPoint(centre + Vector2.right, centre + (Vector2.right+Vector2.down)*.5f, null),
        };
    }

    public SplinePathPoint this[int i]
    {
        get
        {
            return points[i];
        }
    }

    public int NumPoints
    {
        get
        {
            return points.Count * 3 - 2;
        }
    }

    public int NumSegments
    {
        get
        {
            return points.Count - 1;
        }
    }

    public void AddSegment(Vector2 anchorPos)
    {
        SplinePathPoint prevPoint = points[points.Count - 1];
        prevPoint.ControlPointToNext = prevPoint.AnchorPosition * 2 - prevPoint.ControlPointToPrev;
        points.Add(new SplinePathPoint(anchorPos, (prevPoint.ControlPointToNext + anchorPos) * 0.5f, null));
    }

    public Vector2[] GetPointsInSegment(int i)
    {
        return new Vector2[]
        {
            points[i].AnchorPosition,
            points[i].ControlPointToNext.Value,
            points[i + 1].ControlPointToPrev.Value,
            points[i + 1].AnchorPosition
        };
    }

    public void MovePoint(int i, Vector2 pos)
    {
//        Vector2 deltaMove = pos - points[i];
//        points[i] = pos;
//
//        if (i % 3 == 0)
//        {
//            if (i + 1 < points.Count)
//            {
//                points[i + 1] += deltaMove;
//            }
//            if (i - 1 >= 0)
//            {
//                points[i - 1] += deltaMove;
//            }
//        }
//        else
//        {
//            bool nextPointIsAnchor = (i + 1) % 3 == 0;
//            int correspondingControlIndex = (nextPointIsAnchor) ? i + 2 : i - 2;
//            int anchorIndex = (nextPointIsAnchor) ? i + 1 : i - 1;
//
//            if (correspondingControlIndex >= 0 && correspondingControlIndex < points.Count)
//            {
//                float dst = (points[anchorIndex] - points[correspondingControlIndex]).magnitude;
//                Vector2 dir = (points[anchorIndex] - pos).normalized;
//                points[correspondingControlIndex] = points[anchorIndex] + dir * dst;
//            }
//        }
    }

}