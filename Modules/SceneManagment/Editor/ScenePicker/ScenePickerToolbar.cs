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

        private static void ShowPopup(Rect _buttonRect)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || ScenePickerPopup.IsOpen)
            {
                return;
            }
            PopupWindow.Show(_buttonRect, new ScenePickerPopup());
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
