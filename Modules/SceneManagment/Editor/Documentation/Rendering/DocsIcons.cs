using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Every built-in Editor icon the docs window uses, by name. A test loads each one in both Editors, because icon
    /// names can change between Unity versions. The library ships no icon files.
    /// </summary>
    public static class DocsIcons
    {
        // Flat glyphs for buttons and chips (drawn in the text colour, so they follow the skin). Built-in icons are kept
        // for the help "?", Note and Warning only.
        public const string GlyphBack = "‹";
        public const string GlyphForward = "›";
        public const string GlyphSidebar = "☰";
        public const string GlyphCopy = "";
        public const string GlyphOpen = "↗";
        public const string GlyphSelect = "→";
        public const string GlyphPing = "→";
        public const string GlyphSettings = "≡";
        public const string GlyphPlay = "▶";
        public const string GlyphStop = "■";
        public const string GlyphTip = "✓";
        public const string GlyphWhy = "i";

        public const string Help = "_Help";
        public const string Back = "back";
        public const string Forward = "forward";
        public const string Sidebar = "UnityEditor.SceneHierarchyWindow";
        public const string Copy = "Clipboard";
        public const string OpenScript = "cs Script Icon";
        public const string Select = "scenepicking_pickable";
        public const string Ping = "ViewToolZoom";
        public const string Settings = "SettingsIcon";
        public const string Play = "PlayButton";
        public const string Stop = "StopButton";
        public const string Note = "console.infoicon.sml";
        public const string Warning = "console.warnicon.sml";
        public const string Tip = "Valid";
        public const string Why = "UnityEditor.InspectorWindow";
        public const string Tick = "TestPassed";
        public const string External = "Linked";

        /// <summary>Every name above, for the icon test.</summary>
        public static readonly IReadOnlyList<string> All = new[]
        {
            Help, Back, Forward, Sidebar, Copy, OpenScript, Select, Ping, Settings, Play, Stop, Note, Warning, Tip, Why, Tick, External
        };

        /// <summary>
        /// The icon's texture for the current skin (the "d_" variant in the dark skin when there is one), or null when
        /// this Editor has no icon of that name. FindTexture logs nothing for a missing name, unlike IconContent.
        /// </summary>
        public static Texture2D Load(string _name)
        {
            bool wantsDarkVariant = EditorGUIUtility.isProSkin && !_name.StartsWith("d_") && !_name.StartsWith("_");
            if (wantsDarkVariant)
            {
                Texture2D dark = EditorGUIUtility.FindTexture("d_" + _name);
                if (dark != null)
                {
                    return dark;
                }
            }
            return EditorGUIUtility.FindTexture(_name);
        }
    }
}
