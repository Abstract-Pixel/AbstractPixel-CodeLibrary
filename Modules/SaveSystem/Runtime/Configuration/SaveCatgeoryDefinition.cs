using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    [Serializable]
    public sealed class SaveCatgeoryDefinition
    {
        [HideInInspector]public string DisplayName;
        public SaveCategory Category;
        public SaveScope DirectoryScope;
        public string CustomFileName;
        public bool EncryptFile;
        public bool IsSceneSpecific;
    }
}
