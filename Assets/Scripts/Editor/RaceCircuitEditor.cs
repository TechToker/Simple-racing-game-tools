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
        Draw();        
        Input();
    }

    private void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            Undo.RecordObject(_circuit, "Add segment");
            _circuit.AddNewWaypoint(mousePos);
        }
    }

    private void Draw()
    {
        for (int i = 0; i < _circuit.Waypoints.Count; i++)
        {
            Quaternion newRotation = Handles.RotationHandle(_circuit.Waypoints[i].transform.rotation, _circuit.Waypoints[i].transform.position);
            _circuit.Waypoints[i].transform.rotation = newRotation;
        }
        
        Handles.color = Color.green;
        for (int i = 0; i < _circuit.Waypoints.Count; i++)
        {
            Vector3 newPos = Handles.FreeMoveHandle(_circuit.Waypoints[i].transform.position, Quaternion.identity, 2f, Vector3.zero, Handles.CylinderHandleCap);

            if (_circuit.Waypoints[i].transform.position != newPos)
            {
                Undo.RecordObject(_circuit, "Move point");
                _circuit.SetPointPosition(i, newPos);
            }
        }
    }
}