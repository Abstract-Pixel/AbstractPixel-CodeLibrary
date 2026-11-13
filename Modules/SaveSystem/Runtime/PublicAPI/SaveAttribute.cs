using System;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Marks a field to save and restore automatically, keyed by its name. Works on a MonoBehaviour, a
    /// ScriptableObject, or a plain class registered with <see cref="SaveActions.Register(object)"/>.
    /// Renaming the field? Keep its data with <c>Formerly</c>: <c>[Save(Formerly = "hp")] int health;</c>
    /// Data isn't plain fields? Use <see cref="ISavable{TData}"/> instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SaveAttribute : Attribute
    {
        /// <summary>The field's old name. Read when the new name is missing from a save file. Optional.</summary>
        public string Formerly { get; set; }
    }
}
