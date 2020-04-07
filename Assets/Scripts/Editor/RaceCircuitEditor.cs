using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RaceCircuit))]
[CanEditMultipleObjects]
public class RaceCircuitEditor : Editor
{
    private RaceCircuit _circuit;

    private const float _pointInsertTriggerDistance = 15f;
    private const float _pointDeleteTriggerDistance = 5f;
    private int _closestWaypoint = -1;
    
    private void OnEnable()
    {
        _circuit = (RaceCircuit) target;
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Bake"))
        {
            RaceCircuit rc = target as RaceCircuit;
            rc.Bake();
        }
    }
    
    private void OnSceneGUI()
    {
        Input();
        Draw();
    }

    private void Input()
    {
        Event guiEvent = Event.current;
        Vector3 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
        Vector3 mouseXZProjection = new Vector3(mousePos.x, 0, mousePos.z);

        AddWaypointHandler(guiEvent, mouseXZProjection);
        InsertWaypointHandler(guiEvent, mouseXZProjection);
        DeleteWaypointHandler(guiEvent, mouseXZProjection);
        
        HandleUtility.Repaint();
    }

    private void AddWaypointHandler(Event guiEvent, Vector3 toPosition)
    {
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            Undo.RecordObject(_circuit, "Add waypoint");
            _circuit.AddNewWaypoint(toPosition);
        }
    }

    private void DeleteWaypointHandler(Event guiEvent, Vector3 fromPosition)
    {
        _closestWaypoint = -1;
        float minDistance = _pointDeleteTriggerDistance;
        for (int i = 0; i < _circuit.Waypoints.Count; i++)
        {
            Vector3 wpPos = _circuit.Waypoints[i].transform.position;
            float dist = (new Vector2(fromPosition.x, fromPosition.z) - new Vector2(wpPos.x, wpPos.z)).magnitude;

            if (dist < minDistance)
            {
                _closestWaypoint = i;
                minDistance = dist;
            }
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            if (_closestWaypoint >= 0)
            {
                Undo.RecordObject(_circuit, "Delete waypoint");
                _circuit.DeleteWaypoint(_closestWaypoint);
            }
        }
    }

    private void InsertWaypointHandler(Event guiEvent, Vector3 mousePos)
    {
        float minDistance = float.MaxValue;
        Vector2 nearestPoint = Vector2.zero;
        int insertIndex = -1;
        
        Vector2 mousePosXZ = new Vector2(mousePos.x, mousePos.z);
        
        for (int i = 0; i < _circuit.Waypoints.Count - 1; i++)
        {
            Vector2 lineStartPos = new Vector2(_circuit.Waypoints[i].Center.x, _circuit.Waypoints[i].Center.z);
            Vector2 lineFinishPos = new Vector2(_circuit.Waypoints[i + 1].Center.x, _circuit.Waypoints[i + 1].Center.z);
            
            Vector2 nearestPointOnLine = FindNearestPointOnLine(lineStartPos,lineFinishPos, mousePosXZ);
            float distanceToLine = Vector2.Distance(nearestPointOnLine, mousePosXZ);

            if (distanceToLine < minDistance)
            {
                minDistance = distanceToLine;
                nearestPoint = nearestPointOnLine;
                insertIndex = i + 1;
            }
        }

        Vector3 waypointInsertPos = new Vector3(nearestPoint.x, 0, nearestPoint.y);
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.control && minDistance < _pointInsertTriggerDistance)
        {
            _circuit.InsertWaypoint(insertIndex, waypointInsertPos);
        }
        
        if (minDistance < _pointInsertTriggerDistance)
        {
            Handles.color = Color.red;
            Handles.FreeMoveHandle(waypointInsertPos, Quaternion.identity, 2f,Vector3.zero, Handles.CylinderHandleCap);
        }
    }

    private void Draw()
    {
        Handles.color = Color.green;
        for (int i = 0; i < _circuit.Waypoints.Count - 1; i++)
        {
            Handles.DrawLine(_circuit.Waypoints[i].Center, _circuit.Waypoints[i + 1].Center);
            
            Handles.color = Color.black;
            Handles.DrawLine(_circuit.Waypoints[i].Center, _circuit.Waypoints[i].RightRotateControlPoint);
        }

        for (int i = 0; i < _circuit.Waypoints.Count && _circuit.ShowRotationHandles; i++)
        {
            Quaternion newRotation = Handles.RotationHandle(_circuit.Waypoints[i].transform.rotation,
                _circuit.Waypoints[i].transform.position);
            _circuit.Waypoints[i].transform.rotation = newRotation;
        }
        
        for (int i = 0; i < _circuit.Waypoints.Count; i++)
        {
            Handles.color = i != _closestWaypoint? Color.green : Color.magenta;
            Vector3 newPos = Handles.FreeMoveHandle(_circuit.Waypoints[i].transform.position, Quaternion.identity, 2f, Vector3.zero, Handles.CylinderHandleCap);

            //Rotate control
            Vector3 rotateControlPos = Handles.FreeMoveHandle(_circuit.Waypoints[i].RightRotateControlPoint, Quaternion.identity, 1f, Vector3.zero, Handles.CylinderHandleCap);
            if (_circuit.Waypoints[i].RightRotateControlPoint != rotateControlPos)
            {
                _circuit.Waypoints[i].RightRotateControlPoint = rotateControlPos;
            }

            //Handles.FreeMoveHandle(_circuit.Waypoints[i].RightBorder, Quaternion.identity, 1f, Vector3.zero, Handles.CylinderHandleCap);

            //Move control
            if (_circuit.Waypoints[i].transform.position != newPos)
            {
                Undo.RecordObject(_circuit, "Move point");
                _circuit.SetPointPosition(i, newPos);
            }
        }
    }
    
    public Vector2 FindNearestPointOnLine(Vector2 origin, Vector2 end, Vector2 point)
    {
        //Get heading
        Vector2 heading = (end - origin);
        float magnitudeMax = heading.magnitude;
        heading.Normalize();

        //Do projection from the point but clamp it
        Vector2 lhs = point - origin;
        float dotP = Vector2.Dot(lhs, heading);
        dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
        return origin + heading * dotP;
    }
}