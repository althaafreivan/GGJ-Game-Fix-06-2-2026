using UnityEditor;
using UnityEngine;
using EvanUIKits.Dialogue;

[CustomEditor(typeof(DialogueDatabase))]
public class E_DialogueDatabase : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty list = serializedObject.FindProperty("dialogues");

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("DIALOGUE DATABASE", EditorStyles.whiteLargeLabel);
        EditorGUILayout.HelpBox("Manage all game dialogues here. Use the 'Key' to trigger them from the DialogueManager.", MessageType.Info);
        EditorGUILayout.Space(10);

        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(i);
            SerializedProperty key = element.FindPropertyRelative("key");
            SerializedProperty isOneTime = element.FindPropertyRelative("isOneTime");
            SerializedProperty charName = element.FindPropertyRelative("characterName");
            SerializedProperty portrait = element.FindPropertyRelative("portrait");
            SerializedProperty charName2 = element.FindPropertyRelative("characterName2");
            SerializedProperty portrait2 = element.FindPropertyRelative("portrait2");
            SerializedProperty sentences = element.FindPropertyRelative("sentences");

            // --- Main Entry Container ---
            EditorGUILayout.BeginVertical("helpbox");

            // Header Row
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.cyan;
            EditorGUILayout.LabelField($"ID: {(string.IsNullOrEmpty(key.stringValue) ? "UNTITLED" : key.stringValue)}", EditorStyles.boldLabel);
            GUI.color = Color.white;

            GUILayout.FlexibleSpace();

            // One Time Toggle
            isOneTime.boolValue = EditorGUILayout.ToggleLeft("One-Time Show", isOneTime.boolValue, GUILayout.Width(110));

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Delete Entry", GUILayout.Width(100)))
            {
                list.DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
                return;
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            
            EditorGUILayout.PropertyField(key, new GUIContent("Unique Key (ID)"));
            EditorGUILayout.Space(5);

            // --- Characters Section ---
            EditorGUILayout.BeginHorizontal();
            
            // Character 1
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Character 1 (Default)", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(portrait, GUIContent.none, GUILayout.Width(64), GUILayout.Height(64));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Name", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(charName, GUIContent.none);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // Character 2
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Character 2", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(portrait2, GUIContent.none, GUILayout.Width(64), GUILayout.Height(64));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Name", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(charName2, GUIContent.none);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // --- Sentences Section ---
            EditorGUILayout.LabelField($"Sentences List ({sentences.arraySize})", EditorStyles.boldLabel);

            for (int j = 0; j < sentences.arraySize; j++)
            {
                SerializedProperty sentenceElement = sentences.GetArrayElementAtIndex(j);
                SerializedProperty text = sentenceElement.FindPropertyRelative("text");
                SerializedProperty useChar2 = sentenceElement.FindPropertyRelative("useCharacter2");
                SerializedProperty startEv = sentenceElement.FindPropertyRelative("OnSentenceStart");     
                SerializedProperty endEv = sentenceElement.FindPropertyRelative("OnSentenceEnd");

                EditorGUILayout.BeginVertical("box");

                // Line Header
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Line {j + 1}", EditorStyles.miniBoldLabel);
                
                GUILayout.FlexibleSpace();
                
                // Toggle Speaker
                GUI.color = useChar2.boolValue ? Color.yellow : Color.white;
                if (GUILayout.Button(useChar2.boolValue ? "Speaker: Char 2" : "Speaker: Char 1", EditorStyles.miniButton, GUILayout.Width(100)))
                {
                    useChar2.boolValue = !useChar2.boolValue;
                }
                GUI.color = Color.white;

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    sentences.DeleteArrayElementAtIndex(j);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                // Text Area
                EditorGUILayout.PropertyField(text, GUIContent.none);

                // Events Foldout
                sentenceElement.isExpanded = EditorGUILayout.Foldout(sentenceElement.isExpanded, "Line Events (Optional)", true);
                if (sentenceElement.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(startEv);
                    EditorGUILayout.PropertyField(endEv);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }

            // Add Sentence Button
            if (GUILayout.Button("+ Add Sentence Line", EditorStyles.miniButton))
            {
                sentences.arraySize++;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(20);
        }

        // --- Bottom Global Actions ---
        EditorGUILayout.Space(10);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("CREATE NEW DIALOGUE ENTRY", GUILayout.Height(40)))
        {
            list.arraySize++;
        }
        GUI.backgroundColor = Color.white;

        serializedObject.ApplyModifiedProperties();
    }
}
