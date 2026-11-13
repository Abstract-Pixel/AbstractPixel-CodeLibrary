namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Marks a save file class, such as <c>SaveFiles.Ghosts</c>. You write the class wherever you mean that save file,
    /// so a typo doesn't compile. Game save files come from SaveFiles.cs, which the editor writes from the config's
    /// save file list. A library module ships its own class with <see cref="SaveFileDefaultsAttribute"/>.
    /// </summary>
    public interface ISaveFile
    {
    }
}
