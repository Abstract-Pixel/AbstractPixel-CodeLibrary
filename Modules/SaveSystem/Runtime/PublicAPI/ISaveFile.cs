namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Marks a save file class, such as <c>SaveFiles.Settings</c> or <c>GameSaveFiles.Ghosts</c>. You write the class
    /// wherever you mean that save file, so a typo doesn't compile. The built-in save files ship with the library in
    /// <see cref="SaveFiles"/>. Game save files come from GameSaveFiles.cs, which the editor writes from the config's
    /// save file list. A library module ships its own class with <see cref="SaveFileDefaultsAttribute"/>.
    /// </summary>
    public interface ISaveFile
    {
    }
}
