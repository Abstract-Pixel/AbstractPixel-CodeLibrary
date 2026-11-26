using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The scene picker's button on Unity's main toolbar (decision 2): a dropdown in the middle dock, left of Play. Its
    /// label names the open group (decision 11). In Play it is greyed out and keeps naming the open group (decision 9).
    /// It refreshes when scenes open or close, when Play starts or stops and when the project changes.
    /// </summary>
    [InitializeOnLoad]
    public static class ScenePickerToolbar
    {
        /// <summary>The element's path on the main toolbar, also the name Unity's toolbar picker lists it under.</summary>
        public const string ElementPath = "Abstract Pixel/Scene picker";
        public const string Tooltip = "Scene picker: open or select any scene or scene group";

        private static bool refreshQueued;

        static ScenePickerToolbar()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneClosed += OnSceneClosed;
            EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            ScenePickerProject.Changed -= QueueRefresh;
            ScenePickerProject.Changed += QueueRefresh;
        }

        // Play Mode Controls use index 0 in the middle dock; a lower index puts the picker just left of Play.
        [MainToolbarElement(ElementPath, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = -1)]
        public static MainToolbarElement CreateButton()
        {
            MainToolbarContent content = new MainToolbarContent(ScenePickerProject.ButtonLabel(), ScenePickerIcons.Get(ScenePickerIcons.SceneIcon), Tooltip);
            MainToolbarDropdown dropdown = new MainToolbarDropdown(content, ShowPopup);
            dropdown.enabled = !EditorApplication.isPlayingOrWillChangePlaymode;
            return dropdown;
        }

#if UNITY_7000_0_OR_NEWER
        // Unity 7 extras for the button go here (session 5A, stage 9). The 6.3 picker above is complete without them.
#endif

        /// <summary>What a click on the button runs; probes call it to test the real path.</summary>
        internal static void ShowPopupForProbe(Rect _buttonRect)
        {
            ShowPopup(_buttonRect);
        }

        private static void ShowPopup(Rect _buttonRect)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || ScenePickerPopup.IsOpen)
            {
                return;
            }
            if (!IsUsableRect(_buttonRect))
            {
                // A button that Unity is rebuilding has no size yet; Unity would build a broken popup from it.
                return;
            }
            try
            {
                PopupWindow.Show(_buttonRect, new ScenePickerPopup());
            }
            catch (System.Exception exception)
            {
                // Right after a pick opened a scene, Unity's PopupWindow can fail to measure the new popup (NaN position)
                // or to close its previous one. The picker never throws: one warning, and the next click works.
                Debug.LogWarning("[Scene system] Scene picker: Unity could not show the picker (" + exception.GetType().Name + ": " + exception.Message + "). Click it again.");
                if (ScenePickerPopup.Current != null)
                {
                    ScenePickerPopup.Current.Forget();
                }
                DestroyBrokenPopupWindows();
            }
        }

        private static bool IsUsableRect(Rect _rect)
        {
            bool hasNumbers = !float.IsNaN(_rect.x) && !float.IsNaN(_rect.y) && !float.IsNaN(_rect.width) && !float.IsNaN(_rect.height);
            return hasNumbers && _rect.width > 0f && _rect.height > 0f;
        }

        // A popup window Unity failed to show has no host view; Unity's next Show tries to close it and throws. Destroying
        // it lets the next click open the picker again.
        private static void DestroyBrokenPopupWindows()
        {
            foreach (PopupWindow window in Resources.FindObjectsOfTypeAll<PopupWindow>())
            {
                if (window != null && window.rootVisualElement.panel == null)
                {
                    Object.DestroyImmediate(window);
                }
            }
        }

        private static void OnSceneOpened(Scene _scene, OpenSceneMode _mode)
        {
            QueueRefresh();
        }

        private static void OnSceneClosed(Scene _scene)
        {
            QueueRefresh();
        }

        private static void OnActiveSceneChanged(Scene _previous, Scene _next)
        {
            QueueRefresh();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _change)
        {
            QueueRefresh();
        }

        // Many scene events arrive in one frame when a group opens; the button is rebuilt once, on the next Editor tick.
        private static void QueueRefresh()
        {
            if (refreshQueued)
            {
                return;
            }
            refreshQueued = true;
            EditorApplication.delayCall += Refresh;
        }

        private static void Refresh()
        {
            refreshQueued = false;
            MainToolbar.Refresh(ElementPath);
        }
    }
}
