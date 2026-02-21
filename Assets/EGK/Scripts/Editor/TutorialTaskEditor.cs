using UnityEngine;
using UnityEditor;
using EvanGameKits.Tutorial;

    [CustomEditor(typeof(TutorialTask))]
    public class TutorialTaskEditor : Editor
    {
        SerializedProperty requirements;
        SerializedProperty autoStart;
        SerializedProperty deactivateOnComplete;
        SerializedProperty fadeDuration;
        SerializedProperty canvasGroup;
        SerializedProperty onTaskStart;
        SerializedProperty onTaskComplete;

        private void OnEnable()
        {
            requirements = serializedObject.FindProperty("requirements");
            autoStart = serializedObject.FindProperty("autoStart");
            deactivateOnComplete = serializedObject.FindProperty("deactivateOnComplete");
            fadeDuration = serializedObject.FindProperty("fadeDuration");
            canvasGroup = serializedObject.FindProperty("canvasGroup");
            onTaskStart = serializedObject.FindProperty("onTaskStart");
            onTaskComplete = serializedObject.FindProperty("onTaskComplete");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(autoStart);
            EditorGUILayout.PropertyField(deactivateOnComplete);
            EditorGUILayout.PropertyField(fadeDuration);
            EditorGUILayout.PropertyField(canvasGroup);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onTaskStart);
            EditorGUILayout.PropertyField(onTaskComplete);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tutorial Requirements", EditorStyles.boldLabel);

            for (int i = 0; i < requirements.arraySize; i++)
            {
                SerializedProperty req = requirements.GetArrayElementAtIndex(i);
                SerializedProperty type = req.FindPropertyRelative("type");
                SerializedProperty key = req.FindPropertyRelative("key");
                SerializedProperty actionName = req.FindPropertyRelative("actionName");
                SerializedProperty keyUI = req.FindPropertyRelative("keyUI");

                EditorGUILayout.BeginVertical(GUI.skin.box);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Requirement {i + 1}", EditorStyles.miniBoldLabel);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    requirements.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(type);

                if (type.enumValueIndex == (int)RequirementType.Key)
                {
                    EditorGUILayout.PropertyField(key);
                }
                else
                {
                    EditorGUILayout.PropertyField(actionName);
                }

                EditorGUILayout.PropertyField(keyUI);

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add New Requirement"))
            {
                requirements.arraySize++;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
