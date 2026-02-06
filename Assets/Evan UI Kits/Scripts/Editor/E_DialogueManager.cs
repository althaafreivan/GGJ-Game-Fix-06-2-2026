using UnityEditor;
using UnityEngine;
using EvanUIKits.Dialogue;
using System.Linq;

[CustomEditor(typeof(DialogueManager))]
public class E_DialogueManager : Editor
{
    private string[] options;
    private DialogueDatabase cachedDb;

    private void OnEnable()
    {
        RefreshDatabase();
    }

    private void RefreshDatabase()
    {
        // Try finding DB in children first, then scene
        DialogueManager manager = (DialogueManager)target;
        cachedDb = manager.GetComponentInChildren<DialogueDatabase>();

        if (cachedDb == null)
            cachedDb = Object.FindAnyObjectByType<DialogueDatabase>();

        if (cachedDb != null && cachedDb.dialogues != null)
        {
            options = cachedDb.dialogues.Select(d => string.IsNullOrEmpty(d.key) ? "Untitled" : d.key).ToArray();
        }
        else
        {
            options = new string[] { "No Database Found" };
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DialogueManager script = (DialogueManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("DIALOGUE CONTROLLER", EditorStyles.whiteLargeLabel);

        EditorGUILayout.BeginVertical("helpbox");
        EditorGUILayout.LabelField("Database Binding", EditorStyles.boldLabel);

        if (cachedDb != null)
        {
            GUI.color = Color.cyan;
            EditorGUILayout.LabelField($"Connected to: {cachedDb.gameObject.name}", EditorStyles.miniLabel);
            GUI.color = Color.white;

            SerializedProperty keyProp = serializedObject.FindProperty("dialogueKey");
            SerializedProperty indexProp = serializedObject.FindProperty("selectedKeyIndex");

            EditorGUILayout.BeginHorizontal();
            indexProp.intValue = EditorGUILayout.Popup("Target Dialogue", indexProp.intValue, options);

            if (GUILayout.Button("refresh", GUILayout.Width(60))) RefreshDatabase();
            EditorGUILayout.EndHorizontal();

            if (indexProp.intValue < options.Length)
            {
                keyProp.stringValue = options[indexProp.intValue];
            }
        }
        else
        {
            EditorGUILayout.HelpBox("DialogueDatabase not found! Ensure it is a child of this object or in the scene.", MessageType.Error);
            if (GUILayout.Button("Retry Search")) RefreshDatabase();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("UI Components", EditorStyles.boldLabel);
        DrawPropertiesExcluding(serializedObject, "m_Script", "dialogueKey", "selectedKeyIndex");

        serializedObject.ApplyModifiedProperties();
    }
}