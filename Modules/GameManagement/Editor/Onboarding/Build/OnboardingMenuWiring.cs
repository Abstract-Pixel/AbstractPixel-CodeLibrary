using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>The states a copied menu's buttons point at. A null state means that part of the game does not exist, so
    /// the button that would use it is removed.</summary>
    internal class OnboardingMenuWiring
    {
        public OnboardingMenuKind? Kind;
        public bool IsSettingsMenu;
        public StateSO State;
        public StateSO Play;
        public StateSO MainMenu;
        public StateSO Settings;
        /// <summary>This menu gets a button that opens the Settings state.</summary>
        public bool HasSettingsButton;
    }
}
