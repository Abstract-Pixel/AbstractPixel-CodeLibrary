namespace AbstractPixel.SaveSystem
{
    public interface ISavableBridge
    {
        public string UniqueId { get; }

        public object CaptureState(SaveCategory _categoryFilter);

        public void RestoreState(object data, SaveCategory _categoryFilter);

        /// <summary>
        /// Target GUID -> the data type each target restores, for this category. Read on the main thread so the save file
        /// can be converted to these types on a worker thread before RestoreState.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, System.Type> GetRestoreTypes(SaveCategory _categoryFilter);

    }
}
