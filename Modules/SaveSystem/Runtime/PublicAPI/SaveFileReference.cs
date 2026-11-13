using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// A save file picked in the inspector from a dropdown of the config's save file list. It stores the save file
    /// class's full name, the same name the list's rows are found by. In code, write <c>SaveFileReference.To&lt;GameSaveFiles.Ghosts&gt;()</c>.
    /// </summary>
    [Serializable]
    public struct SaveFileReference
    {
        [SerializeField] string classFullName;

        public string ClassFullName => classFullName;

        public bool IsSet => !string.IsNullOrEmpty(classFullName);

        public SaveFileReference(string saveFileClassFullName)
        {
            classFullName = saveFileClassFullName;
        }

        public static SaveFileReference To<TFile>() where TFile : ISaveFile
        {
            return new SaveFileReference(typeof(TFile).FullName);
        }
    }
}
