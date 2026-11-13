using System.Collections.Generic;
using AbstractPixel.Core;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Reads and writes whole save files as JSON text, for editor tools and debug buttons. The save system config
    /// says where each file lives and whether it keeps a backup. Works in play mode and in edit mode.
    /// <code>
    /// SaveStorage.ReadText&lt;SaveFiles.Settings&gt;(out string json);                       // Global file
    /// SaveStorage.ReadText&lt;SaveFiles.GameProfile&gt;(out string json);                    // open game profile
    /// SaveStorage.ReadText&lt;SaveFiles.GameProfile&gt;(otherProfile, out string json);
    /// SaveStorage.ReadText&lt;SaveFiles.Scene&gt;(forestScene, out string json);             // open game profile
    /// SaveStorage.ReadText&lt;SaveFiles.Scene&gt;(forestScene, otherProfile, out string json);
    /// </code>
    /// A Scene save file always needs its scene, as a <see cref="SceneField"/>. In play mode, WriteText and Delete
    /// also update the running save system: the registered objects in that file are restored from the new text,
    /// and the next save can't write the old copy back.
    /// </summary>
    public static class SaveStorage
    {
        /// <summary>Reads a Global save file, or a GameProfile save file of the open game profile.</summary>
        public static bool ReadText<TFile>(out string json) where TFile : ISaveFile
        {
            return Read<TFile>(SaveFileTarget.OpenGameProfile, out json);
        }

        /// <summary>Reads a GameProfile save file of any game profile, open or not.</summary>
        public static bool ReadText<TFile>(GameProfile profile, out string json) where TFile : ISaveFile
        {
            return Read<TFile>(SaveFileTarget.Of(profile), out json);
        }

        /// <summary>Reads a Scene save file for one scene of the open game profile.</summary>
        public static bool ReadText<TFile>(SceneField scene, out string json) where TFile : ISaveFile
        {
            return Read<TFile>(SaveFileTarget.In(scene), out json);
        }

        /// <summary>Reads a Scene save file for one scene of any game profile, open or not.</summary>
        public static bool ReadText<TFile>(SceneField scene, GameProfile profile, out string json) where TFile : ISaveFile
        {
            return Read<TFile>(SaveFileTarget.In(scene, profile), out json);
        }

        /// <summary>Writes a Global save file, or a GameProfile save file of the open game profile. The write is atomic.</summary>
        public static bool WriteText<TFile>(string json) where TFile : ISaveFile
        {
            return Write<TFile>(SaveFileTarget.OpenGameProfile, json);
        }

        /// <summary>Writes a GameProfile save file of any game profile.</summary>
        public static bool WriteText<TFile>(GameProfile profile, string json) where TFile : ISaveFile
        {
            return Write<TFile>(SaveFileTarget.Of(profile), json);
        }

        /// <summary>Writes a Scene save file for one scene of the open game profile.</summary>
        public static bool WriteText<TFile>(SceneField scene, string json) where TFile : ISaveFile
        {
            return Write<TFile>(SaveFileTarget.In(scene), json);
        }

        /// <summary>Writes a Scene save file for one scene of any game profile.</summary>
        public static bool WriteText<TFile>(SceneField scene, GameProfile profile, string json) where TFile : ISaveFile
        {
            return Write<TFile>(SaveFileTarget.In(scene, profile), json);
        }

        /// <summary>Deletes a Global save file, or a GameProfile save file of the open game profile, with its backup.</summary>
        public static bool Delete<TFile>() where TFile : ISaveFile
        {
            return DeleteFile<TFile>(SaveFileTarget.OpenGameProfile);
        }

        /// <summary>Deletes a GameProfile save file of any game profile, with its backup.</summary>
        public static bool Delete<TFile>(GameProfile profile) where TFile : ISaveFile
        {
            return DeleteFile<TFile>(SaveFileTarget.Of(profile));
        }

        /// <summary>Deletes a Scene save file for one scene of the open game profile, with its backup.</summary>
        public static bool Delete<TFile>(SceneField scene) where TFile : ISaveFile
        {
            return DeleteFile<TFile>(SaveFileTarget.In(scene));
        }

        /// <summary>Deletes a Scene save file for one scene of any game profile, with its backup.</summary>
        public static bool Delete<TFile>(SceneField scene, GameProfile profile) where TFile : ISaveFile
        {
            return DeleteFile<TFile>(SaveFileTarget.In(scene, profile));
        }

        /// <summary>The full path of a Global save file, or of the open game profile's file, for "Show in Explorer".</summary>
        public static string GetFilePath<TFile>() where TFile : ISaveFile
        {
            return FilePath<TFile>(SaveFileTarget.OpenGameProfile);
        }

        /// <summary>The full path of a GameProfile save file of any game profile.</summary>
        public static string GetFilePath<TFile>(GameProfile profile) where TFile : ISaveFile
        {
            return FilePath<TFile>(SaveFileTarget.Of(profile));
        }

        /// <summary>The full path of a Scene save file for one scene of the open game profile.</summary>
        public static string GetFilePath<TFile>(SceneField scene) where TFile : ISaveFile
        {
            return FilePath<TFile>(SaveFileTarget.In(scene));
        }

        /// <summary>The full path of a Scene save file for one scene of any game profile.</summary>
        public static string GetFilePath<TFile>(SceneField scene, GameProfile profile) where TFile : ISaveFile
        {
            return FilePath<TFile>(SaveFileTarget.In(scene, profile));
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

        static bool Read<TFile>(SaveFileTarget target, out string json) where TFile : ISaveFile
        {
            json = null;
            SaveFileAccess files = SaveFileAccess.Find();
            return files != null && files.ReadText(typeof(TFile), target, out json);
        }

        static bool Write<TFile>(SaveFileTarget target, string json) where TFile : ISaveFile
        {
            SaveFileAccess files = SaveFileAccess.Find();
            return files != null && files.WriteText(typeof(TFile), target, json);
        }

        static bool DeleteFile<TFile>(SaveFileTarget target) where TFile : ISaveFile
        {
            SaveFileAccess files = SaveFileAccess.Find();
            return files != null && files.Delete(typeof(TFile), target);
        }

        static string FilePath<TFile>(SaveFileTarget target) where TFile : ISaveFile
        {
            SaveFileAccess files = SaveFileAccess.Find();
            return files?.GetFilePath(typeof(TFile), target, nameof(GetFilePath));
        }
    }
}
