using System;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Keeps a class's automatic save ID the same after the class is renamed. Put it on a class registered with
    /// <see cref="SaveActions.Register(object)"/>. Without it, renaming the class changes its automatic save ID and
    /// old saved data stops matching.
    /// Example: <c>[SaveId("Inventory")] public class PlayerBag { [Save] int gold; }</c>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SaveIdAttribute : Attribute
    {
        /// <summary>The save ID to use instead of the class name.</summary>
        public string Id { get; }

        public SaveIdAttribute(string id)
        {
            Id = id;
        }
    }
}
