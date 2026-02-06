using UnityEditor;
using UnityEngine;
using EvanUIKits.PanelController;
using System.Collections.Generic;

[CustomEditor(typeof(UINavigator))]
public class E_UINavigator : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UINavigator script = (UINavigator)target;

        EditorGUILayout.Space(10);
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Refresh Panels from Children", GUILayout.Height(30)))
        {
            RefreshPanels(script);
        }
        GUI.backgroundColor = Color.white;
    }

    private void RefreshPanels(UINavigator script)
    {
        Undo.RecordObject(script, "Refresh UI Panels");

        UIPanel[] childrenPanels = script.GetComponentsInChildren<UIPanel>(true);
        script.panels = new List<UIPanel>(childrenPanels);

        EditorUtility.SetDirty(script);

        Debug.Log($"<color=cyan>UINavigator:</color> Successfully found and assigned {script.panels.Count} panels.");
    }
}