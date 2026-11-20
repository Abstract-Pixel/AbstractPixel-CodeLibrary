using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Rename tracking (decision Q7): when a scene is renamed or moved, every .asset, .prefab and .unity file under
    /// Assets/ that holds its GUID is re-saved, so each scene field's stored name and path follow the scene. Scenes open
    /// with unsaved changes are skipped and named in a warning; they update when saved.
    /// </summary>
    public static class SceneRenameTracking
    {
        /// <summary>Bigger files are skipped: lighting data and meshes, never a scene field. Logged when it happens.</summary>
        public const long LargestFileBytes = 16L * 1024L * 1024L;
        private const string LogPrefix = "[Scene system]";
        private static readonly byte[] YamlHeader = Encoding.ASCII.GetBytes("%YAML");

        /// <summary>For each GUID, the files whose bytes contain it. Files &lt;paramref name="_readBytes"/&gt; returns null for are skipped.</summary>
        public static Dictionary<string, List<string>> PathsContainingGuids(IEnumerable<string> _paths, IReadOnlyList<string> _guids, Func<string, byte[]> _readBytes)
        {
            Dictionary<string, List<string>> found = new Dictionary<string, List<string>>();
            List<byte[]> patterns = new List<byte[]>();
            foreach (string guid in _guids)
            {
                found[guid] = new List<string>();
                patterns.Add(Encoding.ASCII.GetBytes(guid));
            }
            foreach (string path in _paths)
            {
                byte[] bytes = _readBytes(path);
                if (bytes == null)
                {
                    continue;
                }
                ReadOnlySpan<byte> content = bytes;
                for (int index = 0; index < patterns.Count; index++)
                {
                    if (content.IndexOf(patterns[index]) >= 0)
                    {
                        found[_guids[index]].Add(path);
                    }
                }
            }
            return found;
        }

        /// <summary>Every .asset, .prefab and .unity file under Assets/, with forward slashes.</summary>
        public static List<string> ProjectFiles()
        {
            List<string> paths = new List<string>();
            foreach (string pattern in new[] { "*.asset", "*.prefab", "*.unity" })
            {
                foreach (string file in Directory.GetFiles("Assets", pattern, SearchOption.AllDirectories))
                {
                    paths.Add(file.Replace('\\', '/'));
                }
            }
            return paths;
        }

        /// <summary>The file's bytes when it is a text-serialized file of at most &lt;see cref="LargestFileBytes"/&gt;, otherwise null.</summary>
        public static byte[] ReadIfSmallText(string _path)
        {
            try
            {
                FileInfo info = new FileInfo(_path);
                if (!info.Exists || info.Length > LargestFileBytes)
                {
                    return null;
                }
                byte[] bytes = File.ReadAllBytes(_path);
                ReadOnlySpan<byte> content = bytes;
                if (!content.StartsWith(YamlHeader))
                {
                    return null;
                }
                return bytes;
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }

        /// <summary>Called by the asset watcher with the scenes that were renamed or moved.</summary>
        public static void TrackMovedScenes(IReadOnlyList<string> _movedScenePaths)
        {
            if (_movedScenePaths == null || _movedScenePaths.Count == 0)
            {
                return;
            }
            List<string> guids = new List<string>();
            List<string> sceneNames = new List<string>();
            foreach (string scenePath in _movedScenePaths)
            {
                guids.Add(AssetDatabase.AssetPathToGUID(scenePath));
                sceneNames.Add("'" + Path.GetFileNameWithoutExtension(scenePath) + "'");
            }

            List<string> files = ProjectFiles();
            int skippedLarge = CountLargeFiles(files);
            Dictionary<string, List<string>> byGuid = PathsContainingGuids(files, guids, ReadIfSmallText);
            HashSet<string> toRefresh = new HashSet<string>();
            HashSet<string> movedScenes = new HashSet<string>(_movedScenePaths);
            foreach (List<string> paths in byGuid.Values)
            {
                foreach (string path in paths)
                {
                    if (!movedScenes.Contains(path))
                    {
                        toRefresh.Add(path);
                    }
                }
            }
            string what = "Scene " + string.Join(", ", sceneNames) + " was renamed or moved";
            RefreshFiles(new List<string>(toRefresh), what, skippedLarge);
        }

        /// <summary>
        /// Re-saves the scene groups whose stored scene names or paths are stale, so a build (which reads the stored
        /// names) opens renamed scenes. Returns how many were refreshed.
        /// </summary>
        public static int RefreshStaleSceneGroups()
        {
            List<string> stalePaths = new List<string>();
            foreach (SceneGroup group in SceneGroupAssets.FindAll())
            {
                if (HasStaleScene(group))
                {
                    stalePaths.Add(AssetDatabase.GetAssetPath(group));
                }
            }
            if (stalePaths.Count == 0)
            {
                return 0;
            }
            RefreshFiles(stalePaths, "Some scenes were renamed or moved since their scene groups were saved", 0);
            return stalePaths.Count;
        }

        private static bool HasStaleScene(SceneGroup _group)
        {
            List<AbstractPixel.Core.SceneReference> scenes = new List<AbstractPixel.Core.SceneReference>();
            scenes.Add(_group.MainScene);
            scenes.AddRange(_group.ManagerialBootScenesList);
            scenes.AddRange(_group.ContextualBootScenesList);
            foreach (AbstractPixel.Core.SceneReference scene in scenes)
            {
                if (scene != null && scene.SceneFieldReference.HasStaleIdentity)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Re-saves the files so their scene fields copy each scene's current name and path. One console line.</summary>
        public static void RefreshFiles(List<string> _paths, string _what, int _skippedLarge)
        {
            List<string> openWithChanges = OpenScenesWithUnsavedChanges();
            List<string> toSave = new List<string>();
            List<string> skipped = new List<string>();
            foreach (string path in _paths)
            {
                if (openWithChanges.Contains(path))
                {
                    skipped.Add(path);
                    continue;
                }
                toSave.Add(path);
            }
            toSave.Sort(StringComparer.OrdinalIgnoreCase);

            if (toSave.Count > 0)
            {
                AssetDatabase.ForceReserializeAssets(toSave, ForceReserializeAssetsOptions.ReserializeAssets);
            }
            string line = LogPrefix + " " + _what + ": updated its name and path in " + toSave.Count + " file(s)";
            line += toSave.Count > 0 ? ": " + string.Join(", ", toSave) + "." : ".";
            if (_skippedLarge > 0)
            {
                line += " Skipped " + _skippedLarge + " file(s) over " + (LargestFileBytes / (1024 * 1024)) + " MB (lighting data and meshes).";
            }
            Debug.Log(line);
            if (skipped.Count > 0)
            {
                Debug.LogWarning(LogPrefix + " Not updated because they are open with unsaved changes: " + string.Join(", ", skipped) + ". Save them and their scene fields update. Play mode already uses the new name.");
            }
        }

        private static int CountLargeFiles(List<string> _files)
        {
            int count = 0;
            foreach (string path in _files)
            {
                FileInfo info = new FileInfo(path);
                if (info.Exists && info.Length > LargestFileBytes)
                {
                    count++;
                }
            }
            return count;
        }

        private static List<string> OpenScenesWithUnsavedChanges()
        {
            List<string> paths = new List<string>();
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (scene.isDirty && !string.IsNullOrEmpty(scene.path))
                {
                    paths.Add(scene.path);
                }
            }
            return paths;
        }
    }
}
