using System;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Gives a library module's save file class its starting row in the config's save file list.
    /// The editor adds the row by itself; the owner can still change its save location and Backup box.
    /// Example: <c>[SaveFileDefaults("Settings", SaveLocation.Global)] public sealed class SettingsSaveFile : ISaveFile { }</c>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SaveFileDefaultsAttribute : Attribute
    {
        public string Name { get; }
        public SaveLocation Location { get; }

        public SaveFileDefaultsAttribute(string name, SaveLocation location)
        {
            Name = name;
            Location = location;
        }
    }
}
