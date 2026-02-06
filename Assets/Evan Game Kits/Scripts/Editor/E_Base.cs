using EvanGameKits.Entity;
using EvanGameKits.Entity.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(Base), true)]
public class E_Base : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Base entity = (Base)target;

        EditorGUILayout.LabelField("EVAN ENTITY KIT", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        DrawDefaultInspector();

        var modules = entity.GetComponents<MonoBehaviour>();
        foreach (var m in modules)
        {
            if (m == null || m.GetType().Namespace != "EvanGameKits.Entity.Module") continue;

            SerializedObject moduleSerializedObject = new SerializedObject(m);
            SerializedProperty enabledProperty = moduleSerializedObject.FindProperty("m_Enabled");
            moduleSerializedObject.Update();

            string typeName = m.GetType().Name;
            bool isExpanded = SessionState.GetBool(typeName + m.GetInstanceID(), true);

            EditorGUILayout.BeginVertical("helpBox");

            Rect headerRect = EditorGUILayout.GetControlRect();

            Rect toggleRect = new Rect(headerRect.x, headerRect.y, 20, headerRect.height);
            Rect foldoutRect = new Rect(headerRect.x + 30, headerRect.y, headerRect.width - 20, headerRect.height);

            EditorGUI.PropertyField(toggleRect, enabledProperty, GUIContent.none);
            moduleSerializedObject.ApplyModifiedProperties();

            bool newExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, typeName.Replace("M_", "").Replace("_", " "), true, EditorStyles.foldoutHeader);

            if (newExpanded != isExpanded)
                SessionState.SetBool(typeName + m.GetInstanceID(), newExpanded);

            if (newExpanded)
            {
                EditorGUI.indentLevel++;
                Editor moduleEditor = CreateEditor(m);
                moduleEditor.OnInspectorGUI();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = new Color(1, 0.5f, 0.5f);
                if (GUILayout.Button("Remove Module", GUILayout.Width(120)))
                    Undo.DestroyObjectImmediate(m);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        EditorGUILayout.Space(10);

        Rect rect = GUILayoutUtility.GetRect(new GUIContent("Add Module"), "Button");
        if (EditorGUI.DropdownButton(rect, new GUIContent("Add Module (+)"), FocusType.Passive))
        {
            var types = Assembly.GetAssembly(typeof(Base)).GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract &&
                       t.Namespace == "EvanGameKits.Entity.Module" &&
                       t.Name.StartsWith("M_"));

            var dropdown = new ModuleSelectorDropdown(new AdvancedDropdownState(), types, (type) =>
            {
                Type baseType = type.BaseType;
                bool isKitCategory = baseType != typeof(MonoBehaviour) && baseType != typeof(AIBehaviourModule) && baseType.Namespace == "EvanGameKits.Entity.Module";
                var current = isKitCategory ? entity.gameObject.GetComponent(baseType) : null;

                if (current != null) Undo.DestroyObjectImmediate(current);
                Undo.AddComponent(entity.gameObject, type);
            });

            dropdown.Show(rect);
        }

        serializedObject.ApplyModifiedProperties();
    }
}

public class ModuleSelectorDropdown : AdvancedDropdown
{
    private Action<Type> onItemSelected;
    private IEnumerable<Type> moduleTypes;

    public ModuleSelectorDropdown(AdvancedDropdownState state, IEnumerable<Type> types, Action<Type> callback) : base(state)
    {
        moduleTypes = types;
        onItemSelected = callback;
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        var root = new AdvancedDropdownItem("Entity Modules");

        foreach (var type in moduleTypes)
        {
            string category = type.BaseType.Name;
            if (category == "MonoBehaviour") category = "Misc";

            var categoryFolder = root.children.FirstOrDefault(c => c.name == category);
            if (categoryFolder == null)
            {
                categoryFolder = new AdvancedDropdownItem(category);
                root.AddChild(categoryFolder);
            }

            string cleanName = type.Name.Replace("M_", "").Replace(category + "_", "");
            categoryFolder.AddChild(new ModuleItem(cleanName.Replace("_", " "), type));
        }

        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        if (item is ModuleItem moduleItem)
        {
            onItemSelected?.Invoke(moduleItem.Type);
        }
    }

    private class ModuleItem : AdvancedDropdownItem
    {
        public Type Type { get; }
        public ModuleItem(string name, Type type) : base(name) => Type = type;
    }
}

