using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplinePathCreator))]
public class SplinePathEditor : Editor {

    SplinePathCreator creator;
    SplinePath path;

    void OnSceneGUI()
    {
        Input();
        Draw();
    }

    void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            Undo.RecordObject(creator, "Add segment");
            path.AddSegment(mousePos);
        }
    }

    void Draw()
    {

        for (int i = 0; i < path.NumSegments; i++)
        {
            Vector2[] points = path.GetPointsInSegment(i);
            Handles.color = Color.black;
            Handles.DrawLine(points[1], points[0]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.green, null, 2);
        }

        Handles.color = Color.red;
        for (int i = 0; i < path.NumSegments + 1; i++)
        {
            Vector2 left = Handles.FreeMoveHandle(path[i].AnchorPosition, Quaternion.identity, .1f, Vector2.zero, Handles.CylinderHandleCap);
            Vector2 right = Handles.FreeMoveHandle(path[i].AnchorPosition, Quaternion.identity, .1f, Vector2.zero, Handles.CylinderHandleCap);
            
            Vector2 newPos = Handles.FreeMoveHandle(path[i].AnchorPosition, Quaternion.identity, .1f, Vector2.zero, Handles.CylinderHandleCap);
            if (path[i].AnchorPosition != newPos)
            {
                Undo.RecordObject(creator, "Move point");
                path[i].AnchorPosition = newPos;
            }
        }
    }

    void OnEnable()
    {
        creator = (SplinePathCreator)target;
        if (creator.path == null)
        {
            creator.CreatePath();
        }
        path = creator.path;
    }
}
