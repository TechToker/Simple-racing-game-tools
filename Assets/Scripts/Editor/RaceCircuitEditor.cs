using System;
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
        
        GUILayout.Space(15);
        
        GUILayout.Label("Hotkeys: ",  EditorStyles.boldLabel);
        GUILayout.Label("Shift + LMB : Add waypoint at the end");
        GUILayout.Label("Ctrl + LMB : Insert waypoint in sequence");
        GUILayout.Label("RMB : Delete waypoint");
        
        if (GUILayout.Button("Bake"))
        {
            RaceCircuit rc = target as RaceCircuit;
            rc.Bake();
        }
    }
    
    private void OnSceneGUI()
    {
        InputProcessing();
        DrawCircuit();
    }

    private void InputProcessing()
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
            Undo.RecordObject(_circuit, "Insert waypoint");
            _circuit.InsertWaypoint(insertIndex, waypointInsertPos);
        }
        
        if (minDistance < _pointInsertTriggerDistance)
        {
            Handles.color = Color.red;
            Handles.FreeMoveHandle(waypointInsertPos, Quaternion.identity, 2f,Vector3.zero, Handles.CylinderHandleCap);
        }
    }

    private void DrawCircuit()
    {
        Handles.color = Color.black;
        int maxIndex = _circuit.IsClosed? _circuit.Waypoints.Count : _circuit.Waypoints.Count - 1;
        
        for (int i = 0; i < maxIndex; i++)
        {
            WayPoint current = _circuit.GetWaypointByIndex(i);
            WayPoint next = _circuit.GetWaypointByIndex(i + 1);
            
            DrawWaypointInfo(current, i);

            if(_circuit.ShowRoadCenterLine)
                Handles.DrawLine(current.Center, next.Center);

            if (_circuit.ShowRoadBorder)
            {
                Handles.DrawLine(current.LeftBorder, next.LeftBorder);
                Handles.DrawLine(current.RightBorder, next.RightBorder);
            }
        }

        if(_circuit.EnableWaypointsHandles)
            DrawWaypointsHandles();
    }

    private void DrawWaypointInfo(WayPoint wp, int index)
    {
        if (_circuit.ShowWaypointsNames)
        {
            Handles.Label(new Vector3(wp.RightBorder.x - 1, 0, wp.RightBorder.z - 2),
                $"WP {index}{Environment.NewLine}", EditorStyles.boldLabel);
        }

        string info = string.Empty;
        float zOffset = 4.5f;

        if (_circuit.ShowWaypointsData)
        {
            info += $"Ag: {_circuit.Waypoints[index].TurningAngle}° " +
                    $"Dist: {_circuit.Waypoints[index].DistanceToNextWaypoint}m {Environment.NewLine}";

            zOffset += 0.7f;
        }

        if (_circuit.ShowWaypointsDifficulty)
        {
            info += $"Dif: {_circuit.Waypoints[index].WaypointDifficulty}{Environment.NewLine}" +
                    $"nDif: {_circuit.Waypoints[index].NextWaypointDifficulty}{Environment.NewLine}" +
                    $"pDif: {_circuit.Waypoints[index].PrevWaypointDifficulty}{Environment.NewLine}";
                
            zOffset += 1.5f;
        }

        if (_circuit.ShowWaypointsDirections)
        {
            info += $"nDir:{_circuit.Waypoints[index].NextWpDirection}{Environment.NewLine}" +
                    $"pDir:{_circuit.Waypoints[index].PrevWpDirection}{Environment.NewLine}";
                
            zOffset += 1.2f;
        }
        
        Vector3 labelPosition = new Vector3(wp.RightBorder.x, 0, wp.RightBorder.z - zOffset);
        Handles.Label(labelPosition, info);
    }

    private void DrawWaypointsHandles()
    {
        for (int i = 0; i < _circuit.Waypoints.Count; i++)
        {
            Handles.color = i != _closestWaypoint? i == 0? Color.red : Color.green : Color.magenta;
            Vector3 newPos = Handles.FreeMoveHandle(_circuit.Waypoints[i].transform.position, Quaternion.identity, 2f, Vector3.zero, Handles.CylinderHandleCap);
            
            //Rotate control
            Handles.DrawLine(_circuit.Waypoints[i].Center, _circuit.Waypoints[i].RightRotateControlPoint);
            
            Vector3 rotateControlPos = Handles.FreeMoveHandle(_circuit.Waypoints[i].RightRotateControlPoint, Quaternion.identity, 1f, Vector3.zero, Handles.CylinderHandleCap);
            if (_circuit.Waypoints[i].RightRotateControlPoint != rotateControlPos)
            {
                _circuit.Waypoints[i].RightRotateControlPoint = rotateControlPos;
            }
            
            //Move control
            if (_circuit.Waypoints[i].transform.position != newPos)
            {
                Undo.RecordObject(_circuit, "Move point");
                _circuit.SetPointPosition(i, newPos);
            }
        }
    }
    
    private Vector2 FindNearestPointOnLine(Vector2 origin, Vector2 end, Vector2 point)
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