using UnityEditor;
using UnityEngine;
using EvanUIKits.PanelController;
using System.Collections.Generic;

[CustomEditor(typeof(UIPanel))]
public class E_UIPanel : Editor
{
    public override void OnInspectorGUI()
    {
        UIPanel panel = (UIPanel)target;

        EditorGUILayout.Space(5);

        Color originalColor = GUI.contentColor;

        GUI.contentColor = Color.green;
        EditorGUILayout.LabelField("Panel Initiated", panel.name, EditorStyles.boldLabel);
        GUI.contentColor = originalColor;

        DrawDefaultInspector();
    }
}