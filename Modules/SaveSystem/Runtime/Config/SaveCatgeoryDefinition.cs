using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace AbstractPixel.SaveSystem
{
    [Serializable]
    public sealed class SaveCatgeoryDefinition
    {
        [HideInInspector] public string DisplayName;
        public SaveCategory Category;

        [Tooltip("The save location this save file lives in: Global, GameProfile or Scene.")]
        [FormerlySerializedAs("DirectoryScope")]
        public SaveLocation Location = SaveLocation.GameProfile;

        public string CustomFileName;
        public bool EncryptFile;

        /// <summary>The save file's name: the custom name if one is set, otherwise the category's name.</summary>
        public string FileName => string.IsNullOrEmpty(CustomFileName) ? Category.ToString() : CustomFileName;
    }
}
