using UnityEngine.UIElements;
#if UNITY_7000_0_OR_NEWER
using UnityEditor.Toolbars;
#endif

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// "Show the scene picker in the toolbar" (decision 13), for the setup window and the Scene System page. Unity
    /// hides a new toolbar element until the user ticks it. Unity 7 can open its own toolbar picker filtered to the
    /// scene picker; Unity 6.3 cannot, so it shows one line saying where to tick it.
    /// </summary>
    public static class ScenePickerToolbarHint
    {
        public const string ButtonText = "Show the scene picker in the toolbar";
        public const string PickerSearch = "Scene picker";
        public const string Unity6Hint = "Right-click an empty part of Unity's toolbar, or open its More (⋮) menu, then tick Scene picker.";
        public const string Unity6CenteredHint = "Tip: the scene picker opens any scene or group from Unity's toolbar. " + Unity6Hint;

        /// <summary>True where Unity can open its toolbar picker for us (Unity 7).</summary>
        public static bool CanOpenToolbarPicker
        {
            get
            {
#if UNITY_7000_0_OR_NEWER
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>Opens Unity's toolbar picker filtered to the scene picker. Does nothing before Unity 7.</summary>
        public static void OpenToolbarPicker()
        {
#if UNITY_7000_0_OR_NEWER
            MainToolbar.OpenMainToolbarPicker(MainToolbar.PickerMode.ToolbarElements, PickerSearch);
#endif
        }

        /// <summary>For a centred page such as the setup window's last one: the button on Unity 7, a centred tip on 6.3.</summary>
        public static VisualElement Centered()
        {
            if (CanOpenToolbarPicker)
            {
                UnityEngine.UIElements.Button button = SceneSystemUI.Button(ButtonText, "Opens Unity's toolbar picker at the scene picker, so one tick shows it left of Play.", OpenToolbarPicker);
                button.style.alignSelf = Align.Center;
                return button;
            }
            Label hint = SceneSystemUI.Hint(Unity6CenteredHint);
            hint.style.whiteSpace = WhiteSpace.Normal;
            hint.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            hint.style.alignSelf = Align.Center;
            hint.style.maxWidth = 340;
            return hint;
        }

        /// <summary>"Scene picker   [Show the scene picker in the toolbar]" on Unity 7, or the one-line hint on 6.3.</summary>
        public static VisualElement Row()
        {
            VisualElement row = SceneSystemUI.Row();
            row.style.marginTop = 2;
            row.style.marginBottom = 2;
            Label label = new Label("Scene picker");
            label.tooltip = "Opens or selects any scene or scene group from Unity's main toolbar, left of Play.";
            label.style.width = 120;
            label.style.flexShrink = 0;
            row.Add(label);
            if (CanOpenToolbarPicker)
            {
                row.Add(SceneSystemUI.Button(ButtonText, "Opens Unity's toolbar picker at the scene picker, so one tick shows it left of Play.", OpenToolbarPicker));
                return row;
            }
            Label hint = SceneSystemUI.Hint(Unity6Hint);
            hint.style.flexShrink = 1;
            hint.style.whiteSpace = WhiteSpace.Normal;
            hint.tooltip = "Unity hides a new toolbar element until you tick it. Unity 7 adds a button here that does it for you.";
            row.Add(hint);
            return row;
        }
    }
}
