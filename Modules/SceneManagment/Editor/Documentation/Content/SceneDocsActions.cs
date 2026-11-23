using System.Collections.Generic;
using AbstractPixel.SceneManagement.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Runs the docs' action chips. Every action is read-only: it opens, selects, pings, frames, enters or leaves Play,
    /// opens a Project Settings section or opens the scene picker. None changes settings, Build Settings or assets.
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class SceneDocsActions
    {
        /// <summary>The label a chip shows: the last argument.</summary>
        public static string LabelOf(DocsDirective _directive)
        {
            return _directive.Argument(_directive.Arguments.Count - 1);
        }

        public static string IconOf(string _kind)
        {
            switch (_kind)
            {
                case "open-sample": return DocsIcons.GlyphPlay;
                case "ping": return DocsIcons.GlyphPing;
                case "open-settings": return DocsIcons.GlyphSettings;
                case "open-preferences": return DocsIcons.GlyphSettings;
                case "play": return DocsIcons.GlyphPlay;
                case "stop": return DocsIcons.GlyphStop;
                default: return DocsIcons.GlyphSelect;
            }
        }

        public static string TooltipOf(DocsDirective _directive)
        {
            string kind = _directive.Argument(0);
            switch (kind)
            {
                case "open-sample": return "Opens sample " + _directive.Argument(1) + "'s first scene with its scene group.";
                case "select": return "Selects " + _directive.Argument(1) + " in the Project window.";
                case "ping": return "Shows " + _directive.Argument(1) + " in the Project window.";
                case "select-in-scene": return "Selects " + _directive.Argument(2) + " in the Hierarchy and frames it in the Scene view.";
                case "open-settings": return "Opens Project Settings > Abstract Pixel > Scene System at " + _directive.Argument(1) + ".";
                case "open-preferences": return "Opens Preferences > Abstract Pixel > Scenes.";
                case "open-picker": return "Opens the scene picker from the main toolbar.";
                case "play": return "Enters Play mode.";
                case "stop": return "Leaves Play mode.";
                case "highlight": return "Shows the " + _directive.Argument(2) + " field in the " + _directive.Argument(1) + " window.";
                default: return "";
            }
        }

        /// <summary>Runs an action. Returns a short sentence saying what it did, for probes and the Console.</summary>
        public static string Run(DocsDirective _directive)
        {
            string kind = _directive.Argument(0);
            switch (kind)
            {
                case "open-sample": return OpenSample(_directive.Argument(1));
                case "select": return SelectAsset(_directive.Argument(1), false);
                case "ping": return SelectAsset(_directive.Argument(1), true);
                case "select-in-scene": return SelectInScene(_directive.Argument(1), _directive.Argument(2));
                case "open-settings": return OpenSettings(_directive.Argument(1));
                case "open-preferences": return OpenPreferences();
                case "open-picker": return OpenPicker();
                case "play": return SetPlay(true);
                case "stop": return SetPlay(false);
                case "highlight": return Highlight(_directive);
                default: return "unknown action " + kind;
            }
        }

        /// <summary>
        /// Opens the scene the sample's README starts with ("Open `Sample 02 Hub`" in its first Try it step), with its scene
        /// group when it is a group's main scene, after Unity's usual save prompt. Falls back to the first scene group.
        /// </summary>
        public static string OpenSample(string _number)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return "cannot open a sample in Play mode";
            }
            bool mayContinue = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            if (!mayContinue)
            {
                return "cancelled at the save prompt";
            }
            string firstScene = FirstSceneOfReadme(_number);
            SceneGroup group = GroupWithMainScene(_number, firstScene) ?? FirstSampleGroup(_number);
            bool opensOwnScene = firstScene != null && (group == null || group.name != firstScene + "Group");
            string scenePath = opensOwnScene ? SampleScenePath(_number, firstScene) : null;
            if (scenePath != null)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                return "opened " + firstScene;
            }
            if (group == null)
            {
                return "sample " + _number + " has no scene group";
            }
            SceneGroupOpener.Open(group);
            return "opened " + group.name;
        }

        // The first `Sample NN ...` name in the README's Try it section.
        private static string FirstSceneOfReadme(string _number)
        {
            string folder = SceneDocsTopics.SampleFolder(_number);
            string text = folder == null ? null : SceneDocsPaths.Read(folder + "/README.md");
            if (text == null)
            {
                return null;
            }
            int tryIt = text.IndexOf("## Try it", System.StringComparison.Ordinal);
            if (tryIt < 0)
            {
                return null;
            }
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(text.Substring(tryIt), "`(Sample " + _number + " [^`]+)`");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static SceneGroup GroupWithMainScene(string _number, string _sceneName)
        {
            if (_sceneName == null)
            {
                return null;
            }
            string folder = SceneDocsTopics.SampleFolder(_number);
            return AssetDatabase.LoadAssetAtPath<SceneGroup>(folder + "/Scene Groups/" + _sceneName + "Group.asset");
        }

        private static string SampleScenePath(string _number, string _sceneName)
        {
            string path = SceneDocsTopics.SampleFolder(_number) + "/Scenes/" + _sceneName + ".unity";
            return AssetDatabase.LoadMainAssetAtPath(path) != null ? path : null;
        }

        public static SceneGroup FirstSampleGroup(string _number)
        {
            string folder = SceneDocsTopics.SampleFolder(_number);
            if (folder == null)
            {
                return null;
            }
            string[] guids = AssetDatabase.FindAssets("t:SceneGroup", new[] { folder + "/Scene Groups" });
            List<string> paths = new List<string>();
            foreach (string guid in guids)
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            paths.Sort(System.StringComparer.Ordinal);
            if (paths.Count == 0)
            {
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<SceneGroup>(paths[0]);
        }

        private static string SelectAsset(string _assetPath, bool _pingOnly)
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(_assetPath);
            if (asset == null)
            {
                return "missing asset " + _assetPath;
            }
            if (!_pingOnly)
            {
                Selection.activeObject = asset;
            }
            EditorGUIUtility.PingObject(asset);
            return (_pingOnly ? "pinged " : "selected ") + _assetPath;
        }

        // Opens the scene additively when it is not open yet (no save prompt needed), then selects and frames the object.
        private static string SelectInScene(string _scenePath, string _hierarchyPath)
        {
            Scene scene = SceneManager.GetSceneByPath(_scenePath);
            bool isOpen = scene.IsValid() && scene.isLoaded;
            if (!isOpen && !EditorApplication.isPlaying)
            {
                scene = EditorSceneManager.OpenScene(_scenePath, OpenSceneMode.Additive);
            }
            GameObject found = FindInScene(scene, _hierarchyPath);
            if (found == null)
            {
                return "nothing at " + _hierarchyPath + " in " + _scenePath;
            }
            Selection.activeGameObject = found;
            EditorGUIUtility.PingObject(found);
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }
            return "selected " + _hierarchyPath;
        }

        /// <summary>The object at "Root/Child/Grandchild" in a scene, or null.</summary>
        public static GameObject FindInScene(Scene _scene, string _hierarchyPath)
        {
            if (!_scene.IsValid() || !_scene.isLoaded)
            {
                return null;
            }
            string[] parts = _hierarchyPath.Split('/');
            foreach (GameObject root in _scene.GetRootGameObjects())
            {
                if (root.name != parts[0])
                {
                    continue;
                }
                if (parts.Length == 1)
                {
                    return root;
                }
                Transform child = root.transform.Find(_hierarchyPath.Substring(parts[0].Length + 1));
                if (child != null)
                {
                    return child.gameObject;
                }
            }
            return null;
        }

        private static string OpenSettings(string _section)
        {
            SceneSystemSettingsProvider.OpenAt(_section);
            return "opened the Scene System page at " + _section;
        }

        private static string OpenPreferences()
        {
            SettingsService.OpenUserPreferences(SceneOpeningPreferencesProvider.PagePath);
            return "opened " + SceneOpeningPreferencesProvider.PagePath;
        }

        // The picker drops down from the middle of the main toolbar, where its button sits.
        private static string OpenPicker()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return "the scene picker is off in Play mode";
            }
            Rect main = EditorGUIUtility.GetMainWindowPosition();
            Rect button = new Rect(main.x + main.width * 0.5f - 120f, main.y + 30f, 120f, 22f);
            ScenePickerToolbar.ShowPopupForProbe(button);
            return "opened the scene picker";
        }

        private static string SetPlay(bool _play)
        {
            if (_play && !EditorApplication.isPlaying)
            {
                EditorApplication.EnterPlaymode();
                return "entering Play";
            }
            if (!_play && EditorApplication.isPlaying)
            {
                EditorApplication.ExitPlaymode();
                return "leaving Play";
            }
            return "Play already " + (_play ? "on" : "off");
        }

        // highlight "<window title>" "<field label>" "<chip label>" ["<asset to select first>"]
        private static string Highlight(DocsDirective _directive)
        {
            string windowTitle = _directive.Argument(1);
            string field = _directive.Argument(2);
            bool hasAsset = _directive.Arguments.Count > 4;
            string assetPath = hasAsset ? _directive.Argument(3) : null;
            if (hasAsset)
            {
                SelectAsset(assetPath, false);
            }
            if (windowTitle == "Project Settings")
            {
                SceneSystemSettingsProvider.Open();
            }
            EditorApplication.delayCall += () => HighlightWhenOpen(windowTitle, field, assetPath);
            return "highlighting " + field + " in " + windowTitle;
        }

        private static void HighlightWhenOpen(string _windowTitle, string _field, string _fallbackAsset)
        {
            bool found = Highlighter.Highlight(_windowTitle, _field, HighlightSearchMode.Auto);
            LastHighlightFound = found;
            if (found || string.IsNullOrEmpty(_fallbackAsset))
            {
                return;
            }
            SelectAsset(_fallbackAsset, false);
        }

        /// <summary>Whether the last highlight found its field (read by probes).</summary>
        public static bool LastHighlightFound { get; private set; }
    }
}
