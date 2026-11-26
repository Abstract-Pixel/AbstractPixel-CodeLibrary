using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>
    /// Loads the docs style sheets: Docs.uss (layout and Unity's own colour roles) and the skin sheet with our own colour
    /// tokens, DocsDark.uss or DocsLight.uss, picked by EditorGUIUtility.isProSkin.
    /// </summary>
    public static class DocsStyles
    {
        public const string Folder = DocsEngineFiles.Root + "Styles/";

        /// <summary>Adds the docs sheets to an element (a window root or an inspector help row).</summary>
        public static void Apply(VisualElement _root)
        {
            AddSheet(_root, "Docs.uss");
            AddSheet(_root, "DocsNewLayout.uss");
            string skinSheet = EditorGUIUtility.isProSkin ? "DocsDark.uss" : "DocsLight.uss";
            AddSheet(_root, skinSheet);
        }

        private static void AddSheet(VisualElement _root, string _fileName)
        {
            StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Folder + _fileName);
            if (sheet == null)
            {
                return;
            }
            if (_root.styleSheets.Contains(sheet))
            {
                return;
            }
            _root.styleSheets.Add(sheet);
        }
    }
}
