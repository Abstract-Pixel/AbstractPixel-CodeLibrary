using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Lets designers save from UnityEvents, such as a button's On Click, without writing code.</summary>
    [DisallowMultipleComponent]
    public class MonoSaveActions : MonoBehaviour
    {
        [SerializeField, Tooltip("The save file Save Chosen File saves. Pick it from the config's save file list.")]
        SaveFileReference chosenSaveFile;

        public void SaveAll() => SaveActions.SaveAll();

        /// <summary>Saves the save file picked in the inspector. A UnityEvent can call this, because it takes no arguments.</summary>
        public void SaveChosenFile()
        {
            if (!chosenSaveFile.IsSet)
            {
                Debug.LogError($"[SaveSystem] '{name}' was asked to save its chosen save file, but none is picked. Pick one in its MonoSaveActions inspector.", this);
                return;
            }
            SaveActions.Save(chosenSaveFile);
        }

        /// <summary>Saves one save file. In code, pass <c>SaveFileReference.To&lt;SaveFiles.Ghosts&gt;()</c>.</summary>
        public void Save(SaveFileReference saveFile) => SaveActions.Save(saveFile);
    }
}
