using UnityEditor;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// The colours that go inside rich text (links, terms, syntax), where USS cannot reach. Everything else is in
    /// DocsDark.uss and DocsLight.uss. Picked by EditorGUIUtility.isProSkin; contrast is at least 4.5:1 on the
    /// code and window backgrounds of each skin.
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class DocsTheme
    {
        public static string Link { get { return EditorGUIUtility.isProSkin ? "#7BAAF7" : "#0B4FB8"; } }
        public static string Term { get { return EditorGUIUtility.isProSkin ? "#D2D2D2" : "#1E1E1E"; } }
        public static string InlineCode { get { return EditorGUIUtility.isProSkin ? "#D9B98F" : "#7A4A12"; } }
        /// <summary>The faint box behind inline code, like Unity's Script Reference: grey with low alpha, so it suits both skins.</summary>
        public static string InlineCodeBackground { get { return EditorGUIUtility.isProSkin ? "#FFFFFF1A" : "#0000001A"; } }
        public static string Keyword { get { return EditorGUIUtility.isProSkin ? "#7FA7D9" : "#2A55A0"; } }
        public static string TypeName { get { return EditorGUIUtility.isProSkin ? "#8CC4B4" : "#2C6E62"; } }
        public static string String { get { return EditorGUIUtility.isProSkin ? "#CFA488" : "#8A3B1E"; } }
        public static string Comment { get { return EditorGUIUtility.isProSkin ? "#8A9A80" : "#4F6B45"; } }
        /// <summary>Hover colours: the same hues, brighter, so a link or code span under the pointer stands out.</summary>
        public static string LinkHover { get { return EditorGUIUtility.isProSkin ? "#B5D0FF" : "#062F70"; } }
        public static string InlineCodeHover { get { return EditorGUIUtility.isProSkin ? "#F5DDB8" : "#4E2E08"; } }
        public static string TermHover { get { return EditorGUIUtility.isProSkin ? "#FFFFFF" : "#000000"; } }
        public static string InlineCodeBackgroundHover { get { return EditorGUIUtility.isProSkin ? "#FFFFFF33" : "#00000033"; } }
        /// <summary>Method names, light yellow on the dark skin like an IDE's.</summary>
        public static string Method { get { return EditorGUIUtility.isProSkin ? "#DCDCAA" : "#74531F"; } }
        public static string Number { get { return EditorGUIUtility.isProSkin ? "#B8C8A8" : "#3D6B4F"; } }
    }
}
