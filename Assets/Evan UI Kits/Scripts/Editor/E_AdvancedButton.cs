using EvanUIKits;
using EvanUIKits.AdvancedButton;
using EvanUIKits.PanelController;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AdvancedButton))]
public class E_AdvancedButton : Editor
{
    private bool showConfig = false;
    private UINavigator cachedNav;
    private string[] panelOptions;

    private void OnEnable()
    {
        FetchNavigatorData();
    }

    private void FetchNavigatorData()
    {
        cachedNav = Object.FindAnyObjectByType<UINavigator>();
        if (cachedNav != null && cachedNav.panels != null)
        {
            panelOptions = cachedNav.panels.Select(p => p.name).ToArray();
        }
        else
        {
            panelOptions = new string[] { "No Navigator/Panels Found" };
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty animtype = serializedObject.FindProperty("animationType");
        SerializedProperty buttonType = serializedObject.FindProperty("buttonType");
        SerializedProperty customevent = serializedObject.FindProperty("isUsingCustomEvent");
        SerializedProperty buttondown = serializedObject.FindProperty("OnButtonDown");
        SerializedProperty buttonup = serializedObject.FindProperty("OnButtonUp");
        SerializedProperty buttonenter = serializedObject.FindProperty("OnButtonEnter");
        SerializedProperty buttonexit = serializedObject.FindProperty("OnButtonExit");

        SerializedProperty targetPanelName = serializedObject.FindProperty("targetPanelName");
        SerializedProperty selectedIndex = serializedObject.FindProperty("selectedPanelIndex");

        AdvancedButton script = (AdvancedButton)target;

        showConfig = EditorGUILayout.Foldout(showConfig, "Configuration", true);
        if (showConfig)
        {
            EditorGUILayout.PropertyField(buttonType);

            if (script.buttonType == ButtonType.PushPanel)
            {
                EditorGUI.indentLevel++;
                if (cachedNav != null && cachedNav.panels.Count() > 0)
                {
                    selectedIndex.intValue = EditorGUILayout.Popup("Target Panel", selectedIndex.intValue, panelOptions);
                    targetPanelName.stringValue = panelOptions[selectedIndex.intValue];
                }
                else
                {
                    EditorGUILayout.HelpBox("UINavigator Tidak Ada/Panels Kosong", MessageType.Warning);
                    if (GUILayout.Button("Retry Search")) FetchNavigatorData();
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(animtype);
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(customevent);

        if (script.isUsingCustomEvent)
        {
            EditorGUILayout.PropertyField(buttondown);
            EditorGUILayout.PropertyField(buttonup);
            EditorGUILayout.PropertyField(buttonenter);
            EditorGUILayout.PropertyField(buttonexit);
        }

        serializedObject.ApplyModifiedProperties();
    }
}