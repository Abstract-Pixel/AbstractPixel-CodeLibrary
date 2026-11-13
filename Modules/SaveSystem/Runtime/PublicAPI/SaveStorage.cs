using System.Collections.Generic;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Reads and writes whole save files as JSON text, for editor tools and debug buttons. The save system config
    /// says where each file lives and whether it keeps a backup, so nothing else is passed in. Works in play mode
    /// and in edit mode.
    /// <code>
    /// SaveStorage.ReadText&lt;SaveFiles.Settings&gt;(out string json);
    /// SaveStorage.ReadText&lt;SaveFiles.Scene&gt;(profile, "Forest", out string json);
    /// SaveStorage.WriteText&lt;SaveFiles.Settings&gt;(json);
    /// </code>
    /// In play mode the running save system doesn't see a write. Call SaveActions.Reload&lt;TFile&gt;() after
    /// WriteText to restore the saved objects from the new text.
    /// </summary>
    public static class SaveStorage
    {
        /// <summary>Reads a Global save file, or a GameProfile save file of the open game profile.</summary>
        public static bool ReadText<TFile>(out string json) where TFile : ISaveFile
        {
            return Read<TFile>(null, null, out json);
        }

        /// <summary>Reads a GameProfile save file of any game profile, open or not.</summary>
        public static bool ReadText<TFile>(GameProfile profile, out string json) where TFile : ISaveFile
        {
            return Read<TFile>(profile, null, out json);
        }

        /// <summary>Reads a Scene save file for one scene of a game profile.</summary>
        public static bool ReadText<TFile>(GameProfile profile, string sceneName, out string json) where TFile : ISaveFile
        {
            return Read<TFile>(profile, sceneName, out json);
        }

        /// <summary>Writes a Global save file, or a GameProfile save file of the open game profile. The write is atomic.</summary>
        public static bool WriteText<TFile>(string json) where TFile : ISaveFile
        {
            return Write<TFile>(null, null, json);
        }

        public static bool WriteText<TFile>(GameProfile profile, string json) where TFile : ISaveFile
        {
            return Write<TFile>(profile, null, json);
        }

        public static bool WriteText<TFile>(GameProfile profile, string sceneName, string json) where TFile : ISaveFile
        {
            return Write<TFile>(profile, sceneName, json);
        }

        /// <summary>Deletes a Global save file, or a GameProfile save file of the open game profile.</summary>
        public static bool Delete<TFile>() where TFile : ISaveFile
        {
            SaveFileAccess files = SaveFileAccess.Find();
            return files != null && files.Delete(typeof(TFile), null, null);
        }

        /// <summary>The full path of a Global save file, or of the open game profile's file, for "Show in Explorer".</summary>
        public static string GetFilePath<TFile>() where TFile : ISaveFile
        {
            SaveFileAccess files = SaveFileAccess.Find();
            return files?.GetFilePath(typeof(TFile), null, null);
        }

        /// <summary>Every game profile, sorted by slot number. In edit mode they are read from disk.</summary>
        public static IReadOnlyList<GameProfile> GameProfiles
        {
            get
            {
                SaveFileAccess files = SaveFileAccess.Find();
                if (files == null)
                {
                    return new List<GameProfile>();
                }
                return files.GameProfiles.All;
            }
        }

        static bool Read<TFile>(GameProfile profile, string sceneName, out string json) where TFile : ISaveFile
        {
            json = null;
            SaveFileAccess files = SaveFileAccess.Find();
            return files != null && files.ReadText(typeof(TFile), profile, sceneName, out json);
        }

        static bool Write<TFile>(GameProfile profile, string sceneName, string json) where TFile : ISaveFile
        {
            SaveFileAccess files = SaveFileAccess.Find();
            return files != null && files.WriteText(typeof(TFile), profile, sceneName, json);
        }
    }
}
