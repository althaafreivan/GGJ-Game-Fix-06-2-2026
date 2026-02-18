using UnityEditor;
using UnityEngine;
using EvanGameKits.Mechanic;

namespace EvanGameKits.Mechanic
{
    [CustomEditor(typeof(TransformStateTweener))]
    public class E_TransformStateTweener : UnityEditor.Editor
    {
        bool showEvents = false;
        bool showConfig = false;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty startPos = serializedObject.FindProperty("startPosition");
            SerializedProperty endPos = serializedObject.FindProperty("endPosition");
            SerializedProperty timePropStart = serializedObject.FindProperty("invokeDuration");
            SerializedProperty timePropEnd = serializedObject.FindProperty("revokeDuration");
            SerializedProperty invokeEvt = serializedObject.FindProperty("invokeTrigger");
            SerializedProperty revokeEvt = serializedObject.FindProperty("revokeTrigger");
            SerializedProperty ease = serializedObject.FindProperty("ease");
            SerializedProperty ucPosition = serializedObject.FindProperty("useCurrentPosition");

            EditorGUILayout.Space();

            showConfig = EditorGUILayout.Foldout(showConfig, "Configuration", true);
            if (showConfig)
            {
                EditorGUILayout.PropertyField(timePropStart);
                EditorGUILayout.PropertyField(timePropEnd);
                EditorGUILayout.PropertyField(ease);
                EditorGUILayout.PropertyField(ucPosition);
            }

            EditorGUILayout.Space();

            showEvents = EditorGUILayout.Foldout(showEvents, "Events", true);
            if (showEvents)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(invokeEvt);
                EditorGUILayout.PropertyField(revokeEvt);
                EditorGUI.indentLevel--;
            }

            TransformStateTweener script = (TransformStateTweener)target;
            if(script.m_Target==null) script.m_Target = script.transform;

            GUILayout.Space(10);
            GUILayout.Label("Assign", EditorStyles.boldLabel);
            GUILayout.Label(("From " + script.startPosition + (" To End Position = " + script.endPosition)), EditorStyles.whiteMiniLabel);

            if (!script.useCurrentPosition)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Set Start Position"))
                {
                    Undo.RecordObject(script, "Set Start Value");
                    script.startPosition = script.m_Target.position;
                    script.startRotation = script.m_Target.eulerAngles;
                    script.startScale = script.m_Target.localScale;
                    EditorUtility.SetDirty(script);
                }
                if (GUILayout.Button("Move To Start Position"))
                {
                    Undo.RecordObject(script, "Set To Start Position");
                    script.m_Target.transform.position = script.startPosition;
                    script.m_Target.transform.localEulerAngles = script.startRotation;
                    script.m_Target.transform.localScale = script.startScale;
                    EditorUtility.SetDirty(script);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Set End Position"))
            {
                Undo.RecordObject(script, "Set End Value");
                script.endPosition = script.m_Target.position;
                script.endRotation = script.m_Target.eulerAngles;
                script.endScale = script.m_Target.localScale;
                EditorUtility.SetDirty(script);
            }

            if (GUILayout.Button("Move To End Position"))
            {
                Undo.RecordObject(script, "Move To End Position");
                script.m_Target.gameObject.transform.position = script.endPosition;
                script.m_Target.gameObject.transform.localEulerAngles = script.endRotation;
                script.m_Target.gameObject.transform.localScale = script.endScale;
                EditorUtility.SetDirty(script);
            }

            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
