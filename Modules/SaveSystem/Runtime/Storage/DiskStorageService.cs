using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Stores saved text as files on disk. A key such as <c>Global/Settings</c> becomes
    /// <c>&lt;root folder&gt;/Global/Settings.json</c>. Writes go to a temporary file first, so a crash
    /// mid-write never destroys the previous save.
    /// </summary>
    internal sealed class DiskStorageService : IDataStorageService
    {
        const string ReleaseFolderName = "SaveFiles";
        const string DebugFolderName = "DebugSaveFiles";
        const string TemporaryFileSuffix = ".tmp";

        // Windows can lock a file for a moment right after it was read (virus scanners, the search indexer),
        // so swapping a new file in is retried after these waits before the write gives up.
        static readonly int[] SwapRetryDelaysInMilliseconds = { 10, 50, 100, 250, 500 };

        // UTF-8 without a byte order mark, so other tools read the JSON cleanly.
        static readonly Encoding FileEncoding = new UTF8Encoding(false);

        readonly string rootFolder;
        readonly string fileExtension;

        /// <summary>Uses the config's folder and file type, the same ones the game uses.</summary>
        public DiskStorageService(SaveSystemConfigSO config)
            : this(GetRootFolder(config.UseDebugPath), config.PrimaryFileExtension)
        {
        }

        // Tests pass their own temporary folder. Game code never chooses a folder.
        internal DiskStorageService(string rootFolder, FileExtension extension)
        {
            this.rootFolder = Path.GetFullPath(rootFolder);
            fileExtension = "." + extension.ToString().ToLowerInvariant();
        }

        /// <summary>The folder save files live in. The editor tools use this so they find the same folder as the game.</summary>
        public static string GetRootFolder(bool useDebugFolder)
        {
            if (useDebugFolder)
            {
                // Next to Assets/, not inside it, so Unity doesn't import save files as assets.
                return Path.GetFullPath(Path.Combine(Application.dataPath, "..", DebugFolderName));
            }
            return Path.Combine(Application.persistentDataPath, ReleaseFolderName);
        }

        public bool Write(string key, string text)
        {
            if (!StorageKey.IsValid(key))
            {
                return false;
            }

            string path = ToPath(key);
            string temporaryPath = path + TemporaryFileSuffix;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                // A leftover temporary file means an earlier write was interrupted. It is never valid data.
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }

                WriteAndFlush(temporaryPath, text);
                SwapIn(temporaryPath, path);
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                Debug.LogError($"[SaveSystem] Could not write the save key '{key}' to '{path}': {exception.Message} Check that the folder is writable and the disk isn't full. The previous save is untouched.");
                return false;
            }
        }

        public bool TryRead(string key, out string text)
        {
            text = null;
            if (!StorageKey.IsValid(key))
            {
                return false;
            }

            string path = ToPath(key);
            try
            {
                if (!File.Exists(path))
                {
                    return false;
                }
                text = File.ReadAllText(path, FileEncoding);
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                Debug.LogError($"[SaveSystem] Could not read the save key '{key}' from '{path}': {exception.Message} Check that no other program has the file open.");
                return false;
            }
        }

        public bool Exists(string key)
        {
            return StorageKey.IsValid(key) && File.Exists(ToPath(key));
        }

        public bool Delete(string key)
        {
            if (!StorageKey.IsValid(key))
            {
                return false;
            }

            string path = ToPath(key);
            try
            {
                File.Delete(path);
                File.Delete(path + TemporaryFileSuffix);
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                Debug.LogError($"[SaveSystem] Could not delete the save key '{key}' at '{path}': {exception.Message} Check that no other program has the file open.");
                return false;
            }
        }

        public bool DeleteAll(string keyPrefix)
        {
            bool allDeleted = true;
            foreach (string key in ListKeys(keyPrefix))
            {
                allDeleted &= Delete(key);
            }
            RemoveEmptyFolders(rootFolder);
            return allDeleted;
        }

        public IReadOnlyList<string> ListKeys(string keyPrefix)
        {
            if (!Directory.Exists(rootFolder))
            {
                return Array.Empty<string>();
            }

            // Only files ending in the save extension count, so temporary files are never listed.
            return Directory.EnumerateFiles(rootFolder, "*" + fileExtension, SearchOption.AllDirectories)
                .Where(path => path.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
                .Select(ToKey)
                .Where(key => key.StartsWith(keyPrefix ?? string.Empty, StringComparison.Ordinal))
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>The full path of the file a key is stored in, for "Show in Explorer".</summary>
        public string GetFilePath(string key)
        {
            return ToPath(key);
        }

        string ToPath(string key)
        {
            return Path.Combine(rootFolder, key.Replace('/', Path.DirectorySeparatorChar) + fileExtension);
        }

        string ToKey(string path)
        {
            string relativePath = path.Substring(rootFolder.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string withoutExtension = relativePath.Substring(0, relativePath.Length - fileExtension.Length);
            return withoutExtension.Replace(Path.DirectorySeparatorChar, '/');
        }

        // Moves the finished temporary file into place. File.Replace keeps the old file intact if the swap fails.
        static void SwapIn(string temporaryPath, string path)
        {
            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Replace(temporaryPath, path, null);
                    }
                    else
                    {
                        File.Move(temporaryPath, path);
                    }
                    return;
                }
                catch (IOException) when (attempt < SwapRetryDelaysInMilliseconds.Length)
                {
                    Thread.Sleep(SwapRetryDelaysInMilliseconds[attempt]);
                }
            }
        }

        static void WriteAndFlush(string path, string text)
        {
            using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                byte[] bytes = FileEncoding.GetBytes(text ?? string.Empty);
                stream.Write(bytes, 0, bytes.Length);
                // Flush past the OS cache, so the bytes are on disk before the swap.
                stream.Flush(true);
            }
        }

        static void RemoveEmptyFolders(string folder)
        {
            if (!Directory.Exists(folder))
            {
                return;
            }
            foreach (string childFolder in Directory.GetDirectories(folder))
            {
                RemoveEmptyFolders(childFolder);
                if (!Directory.EnumerateFileSystemEntries(childFolder).Any())
                {
                    Directory.Delete(childFolder);
                }
            }
        }
    }
}
