using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>Ladder pills ("1 daily code") and kind badges ("component"), with text so colour is never alone.</summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class DocsPill
    {
        private static readonly string[] Words = { "no code", "daily code", "choices", "full control" };
        private static readonly string[] Tooltips =
        {
            "Level 0: done in the Inspector, no code.",
            "Level 1: the code you write every day.",
            "Level 2: choices most projects make once.",
            "Level 3: full control, for the rare case."
        };

        public static Label Ladder(int _level)
        {
            Label pill = new Label(_level + " " + Words[_level]);
            pill.AddToClassList("docs-pill");
            pill.AddToClassList("docs-pill-" + _level);
            pill.tooltip = Tooltips[_level];
            return pill;
        }

        public static Label Kind(string _kind)
        {
            Label badge = new Label(_kind);
            badge.AddToClassList("docs-kind");
            return badge;
        }
    }
}
