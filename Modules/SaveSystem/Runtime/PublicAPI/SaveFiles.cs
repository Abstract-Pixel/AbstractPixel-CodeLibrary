namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// The built-in save files every game has. Library modules and game code both use them:
    /// <c>SaveActions.Save&lt;SaveFiles.Settings&gt;();</c> or <c>public class Chest : MonoBehaviour, ISavable&lt;ChestData, SaveFiles.Scene&gt;</c>.
    /// The game's own save files are in <c>GameSaveFiles</c>, which the editor writes from the save file list.
    /// </summary>
    public static class SaveFiles
    {
        /// <summary>Data about one playthrough, such as health and inventory. Saved objects with no save file class use it.</summary>
        public sealed class GameProfile : ISaveFile { }

        /// <summary>Data about one scene within one game profile, such as which chests in the Forest are open.</summary>
        public sealed class Scene : ISaveFile { }

        /// <summary>The player's settings. Lives in Global, so every game profile shares it.</summary>
        public sealed class Settings : ISaveFile { }

        /// <summary>The player's input bindings. Lives in Global.</summary>
        public sealed class Input : ISaveFile { }

        /// <summary>The player's achievements. Lives in Global.</summary>
        public sealed class Achievements : ISaveFile { }
    }
}
