namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>The two scene opening preferences, read once, as plain values.</summary>
    public readonly struct SceneOpeningChoices
    {
        public static readonly SceneOpeningChoices BothOn = new SceneOpeningChoices(true, true);

        /// <summary>"Open a main scene with its scene group".</summary>
        public readonly bool MainSceneWithGroup;

        /// <summary>"Open a scene in no group with the default scene set".</summary>
        public readonly bool NoGroupSceneWithDefaultSet;

        public SceneOpeningChoices(bool _mainSceneWithGroup, bool _noGroupSceneWithDefaultSet)
        {
            MainSceneWithGroup = _mainSceneWithGroup;
            NoGroupSceneWithDefaultSet = _noGroupSceneWithDefaultSet;
        }

        /// <summary>True when the given preference is on; the launch scene's "None" counts as on, since it opens nothing anyway.</summary>
        public bool Allows(SceneOpeningPreference _preference)
        {
            switch (_preference)
            {
                case SceneOpeningPreference.MainSceneWithGroup:
                    return MainSceneWithGroup;
                case SceneOpeningPreference.NoGroupSceneWithDefaultSet:
                    return NoGroupSceneWithDefaultSet;
                default:
                    return true;
            }
        }
    }
}
