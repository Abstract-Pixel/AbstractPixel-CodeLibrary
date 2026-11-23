namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>The project settings a status click depends on, read once per popup open. Plain data, so rules are testable.</summary>
    public sealed class ScenePickerProjectFacts
    {
        /// <summary>The Scene System page sends new scenes to Addressables and the Addressables package is installed.</summary>
        public bool NewScenesGoToAddressables;

        /// <summary>Why Addressables cannot take a scene (no settings, no default group); empty when it can.</summary>
        public string AddressablesSetupProblem = string.Empty;

        /// <summary>The active build profile when it has its own scene list; empty when the shared scene list is in use.</summary>
        public string SceneListProfileName = string.Empty;

        public bool AddressablesNeedsSetup
        {
            get { return NewScenesGoToAddressables && !string.IsNullOrEmpty(AddressablesSetupProblem); }
        }
    }
}
