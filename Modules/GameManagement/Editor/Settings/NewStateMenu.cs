using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>The + menu ("Add existing state", "New state") and the making of new State assets.</summary>
    internal static class NewStateMenu
    {
        public static void Show(GameManagementSettings _settings, Rect _button, Action _changed)
        {
            GenericMenu menu = new GenericMenu();
            List<StateSO> unlisted = new List<StateSO>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(StateSO)))
            {
                StateSO state = AssetDatabase.LoadAssetAtPath<StateSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (state != null && !_settings.Catalog.Contains(state))
                {
                    unlisted.Add(state);
                }
            }
            if (unlisted.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("Add existing state/Every state is already listed"));
            }
            foreach (StateSO state in unlisted)
            {
                // The folder tells apart states that share a name. A slash would open another submenu.
                string path = AssetDatabase.GetAssetPath(state).Replace('/', '∕');
                menu.AddItem(new GUIContent("Add existing state/" + state.name + "   (" + path + ")"), false, () =>
                {
                    StateListEdits.Add(_settings.Catalog, state);
                    _changed();
                });
            }
            menu.AddItem(new GUIContent("New state..."), false, () => Create(_settings, null, _changed));
            menu.DropDown(_button);
        }

        /// <summary>Asks for a name, makes the State asset next to the settings and lists it. With a parent, it is a sub-state.</summary>
        public static void Create(GameManagementSettings _settings, StateSO _parent, Action _changed)
        {
            string settingsFolder = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(_settings)).Replace('\\', '/');
            string folder = settingsFolder + "/States";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder(settingsFolder, "States");
            }
            string defaultName = _parent != null ? "New Sub-state" : "New State";
            string path = EditorUtility.SaveFilePanelInProject("New state", defaultName, "asset", "Name the new state.", folder);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
            {
                // Writing over an asset would break every reference to it.
                path = AssetDatabase.GenerateUniqueAssetPath(path);
            }
            StateSO state = ScriptableObject.CreateInstance<StateSO>();
            if (_parent != null)
            {
                state.Parents.Add(_parent);
            }
            AssetDatabase.CreateAsset(state, path);
            StateListEdits.Add(_settings.Catalog, state);
            _changed();
        }
    }
}
