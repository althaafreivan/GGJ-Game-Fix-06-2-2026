using UnityEngine;
using UnityEditor;
using EvanGameKits.Mechanic;

namespace EvanGameKits.Mechanic
{
    [CustomEditor(typeof(InteractionTrigger))]
    [CanEditMultipleObjects]
    public class E_InteractionTrigger : UnityEditor.Editor
    {
        SerializedProperty targetObject;
        SerializedProperty triggerType;
        SerializedProperty state;
        SerializedProperty weightLayers;
        SerializedProperty buttonRenderer;
        SerializedProperty onColor;
        SerializedProperty offColor;
        SerializedProperty onStart;
        SerializedProperty onEnd;

        private void OnEnable()
        {
            targetObject = serializedObject.FindProperty("TargetObject");
            triggerType = serializedObject.FindProperty("triggerType");
            state = serializedObject.FindProperty("state");
            weightLayers = serializedObject.FindProperty("weightLayers");
            buttonRenderer = serializedObject.FindProperty("buttonRenderer");
            onColor = serializedObject.FindProperty("onColor");
            offColor = serializedObject.FindProperty("offColor");
            onStart = serializedObject.FindProperty("OnStart");
            onEnd = serializedObject.FindProperty("OnEnd");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(targetObject);
            EditorGUILayout.PropertyField(triggerType);
            EditorGUILayout.PropertyField(state);

            InteractionTrigger.TriggerType selectedType = (InteractionTrigger.TriggerType)triggerType.enumValueIndex;
            
            if (selectedType == InteractionTrigger.TriggerType.Weight)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Weight Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(weightLayers);
            }

            if (selectedType == InteractionTrigger.TriggerType.Weight || 
                selectedType == InteractionTrigger.TriggerType.Collider3D)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Visual Feedback Settings (Emission)", EditorStyles.boldLabel);
                
                EditorGUILayout.PropertyField(buttonRenderer);
                EditorGUILayout.PropertyField(onColor);
                EditorGUILayout.PropertyField(offColor);

                // Auto-assign renderer if null
                if (buttonRenderer.objectReferenceValue == null)
                {
                    InteractionTrigger trigger = (InteractionTrigger)target;
                    Renderer r = trigger.GetComponent<Renderer>();
                    if (r != null)
                    {
                        buttonRenderer.objectReferenceValue = r;
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onStart);
            EditorGUILayout.PropertyField(onEnd);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
