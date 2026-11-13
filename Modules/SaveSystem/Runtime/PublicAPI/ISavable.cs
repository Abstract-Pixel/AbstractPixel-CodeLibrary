namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// A saved object that hands its data to the save system and takes it back. With no save file class,
    /// its data lives in the GameProfile save file. Use <see cref="ISavable{TData, TFile}"/> to pick another file.
    /// </summary>
    public interface ISavable<TData>
    {
        public TData CaptureData();
        public void RestoreData(TData _loadedData);
    }

    /// <summary>
    /// What and where in one line: a saved object whose data is <typeparamref name="TData"/>, kept in the save file
    /// <typeparamref name="TFile"/>. Example: <c>public class SettingsManager : MonoBehaviour, ISavable&lt;SettingsDTO, SettingsSaveFile&gt;</c>
    /// </summary>
    public interface ISavable<TData, TFile> : ISavable<TData> where TFile : ISaveFile
    {
    }
}
