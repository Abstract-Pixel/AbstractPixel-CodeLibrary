namespace AbstractPixel.SaveSystem
{
    public interface ISavableBridge
    {
        public string UniqueId { get; }

        public object CaptureState(SaveCategory _categoryFilter);

        public void RestoreState(object data, SaveCategory _categoryFilter);

    }
}
