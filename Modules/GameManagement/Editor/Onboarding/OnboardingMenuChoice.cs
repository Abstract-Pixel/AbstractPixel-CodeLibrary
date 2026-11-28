using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>What the developer chose for one menu.</summary>
    public class OnboardingMenuChoice
    {
        public OnboardingMenuKind Kind;
        public bool Ticked;
        /// <summary>The menu already exists (a run later): it stays ticked and cannot change.</summary>
        public bool Locked;
        public bool UseOwnMenu;
        public GameObject OwnPrefab;
        public bool IncludeSettings;

        public OnboardingMenuChoice(OnboardingMenuKind _kind)
        {
            Kind = _kind;
        }

        /// <summary>Ticked now and not made before.</summary>
        public bool IsNew
        {
            get { return Ticked && !Locked; }
        }

        public bool WantsSettings
        {
            get { return IsNew && IncludeSettings && OnboardingMenuInfo.CanIncludeSettings(Kind); }
        }
    }
}
