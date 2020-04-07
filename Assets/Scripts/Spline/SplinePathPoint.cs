using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplinePathPoint
{
    public Vector2 AnchorPosition;
    
    public Vector2? ControlPointToPrev;
    public Vector2? ControlPointToNext;


    public Vector2 LeftBorder;
    public Vector2 RightBorder;

    public SplinePathPoint(Vector2 anchorPos, Vector2? controlToPrev, Vector2? controlToNext)
    {
        AnchorPosition = anchorPos;
        ControlPointToPrev = controlToPrev;
        ControlPointToNext = controlToNext;
        
        
    }
}
