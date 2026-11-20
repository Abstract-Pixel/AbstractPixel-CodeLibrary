namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Anything the save system stores. <see cref="FormatVersion"/> records the shape the data was written in,
    /// so a later version of the save system can recognise and upgrade older files.
    /// </summary>
    internal interface IVersionedSaveData
    {
        int FormatVersion { get; }
    }
}
