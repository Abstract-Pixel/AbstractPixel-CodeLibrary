using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>Unity's own Editor icons for the scene picker, in the variant that matches the light or dark skin.</summary>
    public static class ScenePickerIcons
    {
        public const string SceneIcon = "SceneAsset Icon";
        public const string SceneGroupIcon = "ScriptableObject Icon";
        public const string SelectIcon = "Project";

        /// <summary>The icon the Project window shows for the row's asset, or Unity's scene or asset icon.</summary>
        public static Texture ForAsset(ScenePickerRow _row)
        {
            Texture cached = null;
            if (_row != null && !string.IsNullOrEmpty(_row.AssetPath))
            {
                cached = AssetDatabase.GetCachedIcon(_row.AssetPath);
            }
            if (cached != null)
            {
                return cached;
            }
            bool isGroup = _row != null && _row.Kind == ScenePickerRowKind.SceneGroup;
            return Get(isGroup ? SceneGroupIcon : SceneIcon);
        }

        public static Texture2D Get(string _name)
        {
            if (EditorGUIUtility.isProSkin)
            {
                Texture2D dark = Load("d_" + _name);
                if (dark != null)
                {
                    return dark;
                }
            }
            return Load(_name);
        }

        // IconContent knows Unity's icon names; it logs nothing and returns an empty image for an unknown one.
        private static Texture2D Load(string _name)
        {
            GUIContent content = EditorGUIUtility.TrIconContent(_name);
            if (content == null)
            {
                return null;
            }
            return content.image as Texture2D;
        }
    }
}
