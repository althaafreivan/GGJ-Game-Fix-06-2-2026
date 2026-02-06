using UnityEditor;
using UnityEngine;

namespace EvanUIKits.EditorTools
{
    public class EvanUIContextMenus
    {
        private const string Root = "GameObject/Evan UI Kits/";
        private const string PrefabPath = "Assets/Evan UI Kits/Prefabs/2D/";

        #region Create Methods

        [MenuItem(Root + "Panel/Root", false, 0)]
        private static void CreateRoot(MenuCommand mc) => Spawn(mc, "Root");

        [MenuItem(Root + "/Text", false, 25)]
        private static void CreateText(MenuCommand mc) => Spawn(mc, "Text");

        [MenuItem(Root + "Pop Up/Dialogue", false, 10)]
        private static void CreateDialogue(MenuCommand mc) => Spawn(mc, "Dialog");

        [MenuItem(Root + "Pop Up/Alert", false, 11)]
        private static void CreateAlert(MenuCommand mc) => Spawn(mc, "Alert Dialog");

        [MenuItem(Root + "Pop Up/Confirmation", false, 12)]
        private static void CreateConfirmation(MenuCommand mc) => Spawn(mc, "Confirmation Dialog");

        [MenuItem(Root + "Button/Advanced Button", false, 25)]
        private static void CreateButton(MenuCommand mc) => Spawn(mc, "Button");

        [MenuItem(Root + "Button/Back Button", false, 26)]
        private static void CreateBackButton(MenuCommand mc) => Spawn(mc, "Back Button");

        [MenuItem(Root + "Panel/Big Modal", false, 0)]
        private static void CreateBigModal(MenuCommand mc) => Spawn(mc, "Big Modal");

        [MenuItem(Root + "Panel/Small Modal", false, 0)]
        private static void CreateSmallModal(MenuCommand mc) => Spawn(mc, "Small Modal");

        [MenuItem(Root + "Slider", false, 55)]
        private static void CreateSlider(MenuCommand mc) => Spawn(mc, "Slider");

        #endregion

        private static void Spawn(MenuCommand menuCommand, string fileName)
        {
            string fullPath = $"{PrefabPath}{fileName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);

            if (prefab == null)
            {
                Debug.LogError($"[EvanUIKits] Prefab not found at: {fullPath}");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            GameObjectUtility.SetParentAndAlign(instance, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);

            Selection.activeObject = instance;
        }
    }
}