using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NoiseTexture))]
public class NoiseTextureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Save"))
        {
            ((NoiseTexture)target).SaveToAsset();
        }
        if (GUILayout.Button("Load"))
        {
            ((NoiseTexture)target).LoadAsset();
        }
        EditorGUILayout.EndVertical();

        //Called whenever the inspector is drawn for this object.
        DrawDefaultInspector();
    }

}
