namespace AbstractPixel.Core.Editor
{
    /// <summary>What the project says about one scene, read once so the scene status can be decided without the project.</summary>
    public struct SceneStatusFacts
    {
        /// <summary>The scene asset exists in the project.</summary>
        public bool HasSceneAsset;

        /// <summary>The field still stores a scene name, GUID or path, even if the asset is gone.</summary>
        public bool HasStoredScene;

        /// <summary>The scene is listed in Build Settings and switched on.</summary>
        public bool IsInBuild;

        /// <summary>The scene may never go into a build (a sample scene).</summary>
        public bool IsNeverInBuild;

        /// <summary>The scene has an Addressables entry.</summary>
        public bool IsAddressable;
    }
}
