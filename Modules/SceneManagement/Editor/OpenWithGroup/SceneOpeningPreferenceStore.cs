namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Reads and writes the two scene opening preferences for one project. Both are on by default. They replace
    /// "Open the group with its main scene" (session 5A2): the first time a preference is read and its key is missing,
    /// it takes the old preference's value, so the user's choice carries over to both, and the value is written so it
    /// stays even when the old key changes later.
    /// </summary>
    public sealed class SceneOpeningPreferenceStore
    {
        public const string OldKeyPrefix = "AbstractPixel.SceneSystem.OpenGroupWithMainScene.";
        public const string MainSceneWithGroupKeyPrefix = "AbstractPixel.SceneSystem.OpenMainSceneWithGroup.";
        public const string NoGroupSceneWithDefaultSetKeyPrefix = "AbstractPixel.SceneSystem.OpenNoGroupSceneWithDefaultSet.";

        private const bool DefaultValue = true;

        private readonly IBoolPreferenceStore store;
        private readonly string projectId;

        /// <param name="_store">Where the values live (EditorPrefs in the Editor).</param>
        /// <param name="_projectId">The project's id (PlayerSettings.productGUID), because EditorPrefs are shared by every project.</param>
        public SceneOpeningPreferenceStore(IBoolPreferenceStore _store, string _projectId)
        {
            store = _store;
            projectId = _projectId;
        }

        public bool MainSceneWithGroup
        {
            get { return Read(MainSceneWithGroupKeyPrefix + projectId); }
            set { store.SetBool(MainSceneWithGroupKeyPrefix + projectId, value); }
        }

        public bool NoGroupSceneWithDefaultSet
        {
            get { return Read(NoGroupSceneWithDefaultSetKeyPrefix + projectId); }
            set { store.SetBool(NoGroupSceneWithDefaultSetKeyPrefix + projectId, value); }
        }

        public SceneOpeningChoices Choices
        {
            get { return new SceneOpeningChoices(MainSceneWithGroup, NoGroupSceneWithDefaultSet); }
        }

        private bool Read(string _key)
        {
            if (store.HasKey(_key))
            {
                return store.GetBool(_key, DefaultValue);
            }
            bool carriedOver = ReadOldPreference();
            store.SetBool(_key, carriedOver);
            return carriedOver;
        }

        // "Open the group with its main scene", the single preference before session 5A2.
        private bool ReadOldPreference()
        {
            string oldKey = OldKeyPrefix + projectId;
            if (!store.HasKey(oldKey))
            {
                return DefaultValue;
            }
            return store.GetBool(oldKey, DefaultValue);
        }
    }
}
