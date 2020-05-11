using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathfExtensions
{
    public static Vector2 FindNearestPointOnLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
    {
        //Get heading
        Vector2 heading = (lineEnd - lineStart);
        float magnitudeMax = heading.magnitude;
        heading.Normalize();

        //Do projection from the point but clamp it
        Vector2 lhs = point - lineStart;
        float dotP = Vector2.Dot(lhs, heading);
        dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
        return lineStart + heading * dotP;
    }
    
    public static Vector2 ConvertToXZ(Vector3 vec)
    {
        return new Vector2(vec.x, vec.z);
    }

    public static Vector3 ConvertFromXZ(Vector2 vec)
    {
        return new Vector3(vec.x, 0, vec.y);
    }
}
