using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>One page of the setup window: a UXML template and the code that makes its controls change the choices.</summary>
    internal abstract class OnboardingPage
    {
        /// <summary>The page's UXML file name, without the extension.</summary>
        internal abstract string Template { get; }

        internal virtual string NextText
        {
            get { return "Next"; }
        }

        internal virtual bool BackAllowed
        {
            get { return true; }
        }

        /// <summary>Connects the loaded page to the choices. Runs every time the page is shown.</summary>
        internal virtual void Wire(VisualElement _root, OnboardingContext _context)
        {
        }

        /// <summary>Runs when Next is pressed. Return false to stay on the page (the page says why).</summary>
        internal virtual bool Leave(VisualElement _root, OnboardingContext _context)
        {
            return true;
        }
    }
}
