using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    public static class SaveActions
    {
        public static bool IsDataSaved => SaveManager.Instance.isDataSaved;
        public static bool IsDataLoaded => SaveManager.Instance.isDataLoaded;

        public static void SaveAll()
        {
            SaveManager.Instance.SaveALL();
        }
        public static void SaveDataOf(SaveCategory _category)
        {
            SaveManager.Instance.SaveDataOf(_category);
        }

        public static void SaveDataOfScope(SaveScope _scope)
        {
            SaveManager.Instance.SaveAllDataByScope(_scope);
        }


        public static void LoadAll()
        {
            SaveManager.Instance.LoadALL();
        }


        public static void LoadDataOf(SaveCategory _category)
        {
            SaveManager.Instance.LoadDataOf(_category);
        }

        public static void LoadDataOfScope(SaveScope _scope)
        {
            SaveManager.Instance.LoadAllDataByScope(_scope);
        }

    }
}
