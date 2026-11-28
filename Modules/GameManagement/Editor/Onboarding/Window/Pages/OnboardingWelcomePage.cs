using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Page 1: the four words the tool uses, and the "Don't show again" switch for this project.</summary>
    internal class OnboardingWelcomePage : OnboardingPage
    {
        internal override string Template
        {
            get { return "OnboardingWelcome"; }
        }

        internal override void Wire(VisualElement _root, OnboardingContext _context)
        {
            Toggle dontShow = new Toggle("Don't show again");
            dontShow.name = "dontShowAgain";
            dontShow.value = OnboardingAutoOpen.IsTurnedOff;
            dontShow.tooltip = "For this project. Project Settings > Abstract Pixel > Game Management can turn it back on.";
            dontShow.style.marginTop = 10;
            dontShow.RegisterValueChangedCallback(_change => OnboardingAutoOpen.IsTurnedOff = _change.newValue);
            _root.Q<VisualElement>(className: "gm-content-inner").Add(dontShow);
        }
    }
}
