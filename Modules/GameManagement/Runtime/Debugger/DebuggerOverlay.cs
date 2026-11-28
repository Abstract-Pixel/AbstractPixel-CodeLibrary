using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Debugging
{
    /// <summary>The in-game debugger: one hidden object, kept across scenes, that starts closed.</summary>
    /// <remarks>
    /// - Shown and hidden by the settings' show or hide action, and in the Editor by a shortcut (F8).
    /// - It only reads the machine of <see cref="GameStates"/> and never makes one.
    /// </remarks>
    [AddComponentMenu("")]
    internal class DebuggerOverlay : MonoBehaviour
    {
        /// <summary>Set by the editor code, which loads the assets by path. A player finds the preloaded ones.</summary>
        public static Func<DebuggerOverlayAssets> EditorAssets;

        /// <summary>The Editor shortcut's current key, set by the editor code.</summary>
        public static Func<string> EditorKey;

        /// <summary>True in the frame the shortcut's key goes down in the Game view, where Unity runs no shortcut during Play.</summary>
        public static Func<bool> EditorKeyPressed;

        private static DebuggerOverlay instance;

        private GameManagementSettings settings;
        private DebuggerToggleInput toggle;
        private DebuggerOverlayView view;
        private GameStateMachine shownMachine;
        private int shownVersion;
        private string keyHint;

        public static DebuggerOverlay Instance
        {
            get { return instance; }
        }

        public bool IsShown { get; private set; }

        public void SetShown(bool _shown)
        {
            IsShown = _shown;
            view.Root.style.display = _shown ? DisplayStyle.Flex : DisplayStyle.None;
            if (_shown)
            {
                keyHint = KeyHint();
                Redraw();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForNewPlaySession()
        {
            instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateOnce()
        {
            if (instance != null)
            {
                return;
            }

            DebuggerOverlayAssets assets = FindAssets();
            if (assets == null)
            {
                Debug.LogWarning("[Game management] The in-game debugger's UI assets were not found, so it is off. Reimport the Abstract Pixel Library package.");
                return;
            }

            GameObject host = new GameObject("[Game State Debugger Overlay]");
            host.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad(host);
            host.SetActive(false);
            UIDocument document = host.AddComponent<UIDocument>();
            document.panelSettings = assets.Panel;
            host.SetActive(true);

            instance = host.AddComponent<DebuggerOverlay>();
            instance.Build(document, assets.Style);
        }

        private static DebuggerOverlayAssets FindAssets()
        {
            if (EditorAssets != null)
            {
                return EditorAssets();
            }
            DebuggerOverlayAssets[] loaded = Resources.FindObjectsOfTypeAll<DebuggerOverlayAssets>();
            return loaded.Length == 0 ? null : loaded[0];
        }

        private void Build(UIDocument _document, StyleSheet _style)
        {
            view = new DebuggerOverlayView();
            view.Root.styleSheets.Add(_style);
            _document.rootVisualElement.pickingMode = PickingMode.Ignore;
            _document.rootVisualElement.Add(view.Root);
            settings = GameManagementSettingsSource.Find();
            InputActionReference reference = settings != null ? settings.DebuggerToggleAction : null;
            toggle = new DebuggerToggleInput(reference != null ? reference.action : null);
            SetShown(false);
        }

        // Update and unscaled input: both keep running at time scale 0.
        private void Update()
        {
            bool editorKey = EditorKeyPressed != null && EditorKeyPressed();
            if (editorKey || (toggle != null && toggle.WasPerformed()))
            {
                SetShown(!IsShown);
            }
            if (IsShown && HasChanged())
            {
                Redraw();
            }
        }

        private void OnDestroy()
        {
            toggle?.Dispose();
        }

        private static GameStateMachine CurrentMachine()
        {
            return GameStates.HasMachine ? GameStates.Machine : null;
        }

        private bool HasChanged()
        {
            GameStateMachine machine = CurrentMachine();
            return machine != shownMachine || (machine != null && machine.Journal.Version != shownVersion);
        }

        private void Redraw()
        {
            shownMachine = CurrentMachine();
            shownVersion = shownMachine != null ? shownMachine.Journal.Version : 0;
            GameStateCatalog catalog = settings != null ? settings.Catalog : null;
            view.Show(DebuggerSnapshot.From(shownMachine, catalog), keyHint);
        }

        private string KeyHint()
        {
            string key = EditorKey != null ? EditorKey() : null;
            if (string.IsNullOrEmpty(key) && toggle != null && toggle.Listening != null)
            {
                key = toggle.Listening.GetBindingDisplayString();
            }
            return string.IsNullOrEmpty(key) ? string.Empty : key + " hide";
        }
    }
}
