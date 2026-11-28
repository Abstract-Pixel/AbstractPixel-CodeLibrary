using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>"Which menus?": a tick for each menu. Menus that already exist (a run later) are ticked and locked.</summary>
    internal class OnboardingMenusPage : OnboardingPage
    {
        private static readonly string[] ToggleNames = { "toggle-mainmenu", "toggle-pause", "toggle-gameover", "toggle-gamewin" };

        internal override string Template
        {
            get { return "OnboardingMenus"; }
        }

        internal override void Wire(VisualElement _root, OnboardingContext _context)
        {
            foreach (OnboardingMenuKind kind in System.Enum.GetValues(typeof(OnboardingMenuKind)))
            {
                OnboardingMenuChoice menu = _context.Choices.Get(kind);
                Toggle toggle = _root.Q<Toggle>(ToggleNames[(int)kind]);
                toggle.SetValueWithoutNotify(menu.Ticked);
                toggle.parent.EnableInClassList("gm-selected", menu.Ticked);
                if (menu.Locked)
                {
                    toggle.parent.AddToClassList("gm-locked");
                    toggle.SetEnabled(false);
                    toggle.tooltip = "Already set up in this project.";
                    continue;
                }
                toggle.RegisterValueChangedCallback(_change =>
                {
                    menu.Ticked = _change.newValue;
                    toggle.parent.EnableInClassList("gm-selected", _change.newValue);
                });
            }
        }
    }
}
