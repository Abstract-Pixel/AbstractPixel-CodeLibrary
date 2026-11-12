using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    [DisallowMultipleComponent] 
    public class MonoSaveActions : MonoBehaviour
    {
        public void SaveAll() => SaveActions.SaveAll();
        public void LoadAll() => SaveActions.LoadAll();
        public void SaveDataOf(SaveCategory _category) => SaveActions.SaveDataOf(_category);
        public void LoadDataOf(SaveCategory _category) => SaveActions.LoadDataOf(_category);

    }
}