using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>Ladder pills ("Basic Code") and kind badges ("component"), with text so colour is never alone.</summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class DocsPill
    {
        // Title Case, no numbers: the colour and the word give the level.
        private static readonly string[] Words = { "No Code", "Basic Code", "Choices", "Full Control" };
        private static readonly string[] Tooltips =
        {
            "Done in the Inspector, no code.",
            "The few calls most games make.",
            "Choices most projects make once.",
            "Full control, for the rare case."
        };

        public static Label Ladder(int _level)
        {
            Label pill = new Label(Words[_level]);
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
