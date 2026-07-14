using AbstractPixel.Core;
using AbstractPixel.SaveSystem;

namespace AbstractPixel.Settings
{
    public class SettingsManager : PersistentSingleton<SettingsManager>, ISavable<SettingsDTO>
    {
        // Your code here
        public SettingsDTO CaptureData()
        {
            throw new System.NotImplementedException();
        }

        public void RestoreData(SettingsDTO _loadedData)
        {
            throw new System.NotImplementedException();
        }
    }
}