using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Lets designers save from UnityEvents, such as a button's On Click, without writing code.</summary>
    [DisallowMultipleComponent]
    public class MonoSaveActions : MonoBehaviour
    {
        public void SaveAll() => SaveActions.SaveAll();

        /// <summary>Saves one save file, for example "Settings".</summary>
        public void Save(string _saveFileName) => SaveActions.Save(_saveFileName);
    }
}
