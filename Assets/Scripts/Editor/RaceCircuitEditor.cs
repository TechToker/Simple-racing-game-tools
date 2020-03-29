using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RaceCircuit))]
[CanEditMultipleObjects]
public class RaceCircuitEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Bake"))
        {
            RaceCircuit rc = target as RaceCircuit;
            rc.Bake();
        }

    }
}