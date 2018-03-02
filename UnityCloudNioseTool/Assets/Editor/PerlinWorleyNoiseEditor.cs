using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PerlinWorleyNoise))]
public class PerlinWorleyNoiseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Save"))
        {
            ((PerlinWorleyNoise)target).SaveToAsset();
        }
        if (GUILayout.Button("Load"))
        {
            ((PerlinWorleyNoise)target).LoadAsset();
        }
        EditorGUILayout.EndVertical();

        //Called whenever the inspector is drawn for this object.
        DrawDefaultInspector();
    }

}
