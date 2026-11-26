using System;

namespace AbstractPixel.SceneManagement
{
    /// <summary>The asset behind a scene name in saved scene group data.</summary>
    [Serializable]
    public class SavedSceneIdentity
    {
        /// <summary>The scene name the saved lists use.</summary>
        public string Name;
        /// <summary>The scene asset's GUID.</summary>
        public string Guid;
        /// <summary>The scene asset's path.</summary>
        public string Path;
    }
}
