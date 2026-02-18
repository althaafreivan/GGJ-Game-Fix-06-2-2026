using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using EvanGameKits.Mechanic;

    [CustomEditor(typeof(GuideSystem))]
    public class E_GuideSystem : Editor
    {
        private ReorderableList stepsList;
        private SerializedProperty mainCanvasGroup;
        private SerializedProperty guideImage;
        private SerializedProperty guideText;
        private SerializedProperty nextButton;
        private SerializedProperty prevButton;
        private SerializedProperty closeButton;
        private SerializedProperty pageIndicator;
        private SerializedProperty fadeDuration;
        private SerializedProperty moveDuration;
        private SerializedProperty easeType;

        private void OnEnable()
        {
            mainCanvasGroup = serializedObject.FindProperty("mainCanvasGroup");
            guideImage = serializedObject.FindProperty("guideImage");
            guideText = serializedObject.FindProperty("guideText");
            nextButton = serializedObject.FindProperty("nextButton");
            prevButton = serializedObject.FindProperty("prevButton");
            closeButton = serializedObject.FindProperty("closeButton");
            pageIndicator = serializedObject.FindProperty("pageIndicator");
            fadeDuration = serializedObject.FindProperty("fadeDuration");
            moveDuration = serializedObject.FindProperty("moveDuration");
            easeType = serializedObject.FindProperty("easeType");

            stepsList = new ReorderableList(serializedObject, serializedObject.FindProperty("steps"), true, true, true, true);

            stepsList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Guide Steps");
            };

            stepsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = stepsList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                
                float spriteWidth = 60;
                float spacing = 5;

                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, spriteWidth, EditorGUIUtility.singleLineHeight * 2),
                    element.FindPropertyRelative("image"), GUIContent.none);

                EditorGUI.PropertyField(
                    new Rect(rect.x + spriteWidth + spacing, rect.y, rect.width - spriteWidth - spacing, EditorGUIUtility.singleLineHeight * 2),
                    element.FindPropertyRelative("description"), GUIContent.none);
            };

            stepsList.elementHeightCallback = (index) => {
                return EditorGUIUtility.singleLineHeight * 2 + 10;
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("EVAN GUIDE SYSTEM", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("helpBox");
            EditorGUILayout.LabelField("UI References", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(mainCanvasGroup);
            EditorGUILayout.PropertyField(guideImage);
            EditorGUILayout.PropertyField(guideText);
            EditorGUILayout.PropertyField(nextButton);
            EditorGUILayout.PropertyField(prevButton);
            EditorGUILayout.PropertyField(closeButton);
            EditorGUILayout.PropertyField(pageIndicator);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("helpBox");
            EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(fadeDuration);
            EditorGUILayout.PropertyField(moveDuration);
            EditorGUILayout.PropertyField(easeType);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            stepsList.DoLayoutList();

            EditorGUILayout.Space();

            if (GUILayout.Button("Open Guide (In-Game)", GUILayout.Height(30)))
            {
                if (Application.isPlaying)
                {
                    ((GuideSystem)target).ShowGuide();
                }
                else
                {
                    Debug.LogWarning("You must be in Play Mode to test the guide UI.");
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
