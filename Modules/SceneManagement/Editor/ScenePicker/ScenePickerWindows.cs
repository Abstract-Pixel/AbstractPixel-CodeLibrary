using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Opens the windows a status click leads to (session 5A2). Build Profiles opens through Unity's public
    /// BuildPlayerWindow.ShowBuildPlayerWindow; Unity has no public call that picks its scene list, so the scene list
    /// in use is picked through guarded reflection on the window. When that fails (a Unity version that renamed it),
    /// the window simply stays where Unity opened it, and nothing is logged.
    /// </summary>
    public static class ScenePickerWindows
    {
        private const string BuildProfileWindowType = "UnityEditor.Build.Profile.BuildProfileWindow, UnityEditor.BuildProfileModule";
        private const string SharedSceneListHandler = "OnClassicSceneListSelected";
        private const string SelectionField = "m_BuildProfileSelection";
        private const string SelectItemMethod = "SelectItem";
        private const string CustomProfileHandler = "OnCustomProfileSelected";
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        /// <summary>Build Profiles at the scene list in use: the active build profile's own list when it overrides the shared one.</summary>
        public static void OpenBuildProfilesSceneList()
        {
            BuildPlayerWindow.ShowBuildPlayerWindow();
            // The window builds its lists in CreateGUI, which runs after this call returns.
            AbstractPixel.Core.Editor.EditorIdleQueue.Run(PickSceneListInUse);
        }

        public static void OpenAddressablesGroups()
        {
            AddressablesSetup.OpenGroupsWindow();
        }

        /// <summary>True when the reflection this class uses still finds Unity's members (checked by a probe on each Unity version).</summary>
        public static bool CanPickSceneList()
        {
            Type windowType = Type.GetType(BuildProfileWindowType);
            if (windowType == null)
            {
                return false;
            }
            return windowType.GetMethod(SharedSceneListHandler, Private) != null && windowType.GetField(SelectionField, Private) != null;
        }

        private static void PickSceneListInUse()
        {
            try
            {
                EditorWindow window = OpenBuildProfileWindow();
                if (window == null)
                {
                    return;
                }
                BuildProfile profile = BuildProfile.GetActiveBuildProfile();
                bool profileHasItsOwnList = profile != null && profile.overrideGlobalScenes;
                if (profileHasItsOwnList)
                {
                    PickProfile(window, profile);
                    return;
                }
                MethodInfo showSharedList = window.GetType().GetMethod(SharedSceneListHandler, Private);
                if (showSharedList != null)
                {
                    showSharedList.Invoke(window, null);
                }
            }
            catch (Exception)
            {
                // Unity changed its window: it stays where it opened, which still has the scene list one click away.
            }
        }

        private static EditorWindow OpenBuildProfileWindow()
        {
            Type windowType = Type.GetType(BuildProfileWindowType);
            if (windowType == null)
            {
                return null;
            }
            UnityEngine.Object[] windows = Resources.FindObjectsOfTypeAll(windowType);
            if (windows.Length == 0)
            {
                return null;
            }
            return windows[0] as EditorWindow;
        }

        // The active profile's page holds its own scene list.
        private static void PickProfile(EditorWindow _window, BuildProfile _profile)
        {
            FieldInfo selectionField = _window.GetType().GetField(SelectionField, Private);
            object selection = selectionField == null ? null : selectionField.GetValue(_window);
            if (selection != null)
            {
                MethodInfo selectItem = selection.GetType().GetMethod(SelectItemMethod, Private);
                if (selectItem != null)
                {
                    selectItem.Invoke(selection, new object[] { _profile });
                }
            }
            MethodInfo showProfile = _window.GetType().GetMethod(CustomProfileHandler, Private);
            if (showProfile != null)
            {
                showProfile.Invoke(_window, new object[] { new object[] { _profile } });
            }
            // The profile's page builds its sections a moment later; its Scene List section starts folded.
            _window.rootVisualElement.schedule.Execute(() => UnfoldSceneList(_window)).ExecuteLater(SceneListUnfoldDelayMilliseconds);
        }

        private const long SceneListUnfoldDelayMilliseconds = 150;
        private const string SceneListSectionTitle = "Scene List";

        private static void UnfoldSceneList(EditorWindow _window)
        {
            if (_window == null)
            {
                return;
            }
            foreach (Foldout foldout in _window.rootVisualElement.Query<Foldout>().ToList())
            {
                if (foldout.text == SceneListSectionTitle)
                {
                    foldout.value = true;
                    return;
                }
            }
        }
    }
}
