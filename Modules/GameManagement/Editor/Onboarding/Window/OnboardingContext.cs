namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>What every page of the setup window shares: the choices, what the run made, and whether Next may be pressed.</summary>
    internal class OnboardingContext
    {
        internal OnboardingChoices Choices;
        internal OnboardingResult Result;
        internal bool PlayExists;
        /// <summary>The folder the settings are in, or will be in.</summary>
        internal string Folder;
        /// <summary>A page sets this when something it shows makes Next unsafe, for example a folder outside Assets.</summary>
        internal bool CanContinue = true;
        /// <summary>A page calls this after it changes CanContinue.</summary>
        internal System.Action Changed;
    }
}
