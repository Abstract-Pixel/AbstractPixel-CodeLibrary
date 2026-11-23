using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Renames a scene set's asset file from the Scene System page (session 5A2). The name must be a valid file name
    /// and free: no other scene set in the project and no other asset in the same folder may have it. The GUID stays,
    /// so every scene group keeps its scene set.
    /// </summary>
    public static class SceneSetRename
    {
        /// <summary>The names this scene set can't take: the other scene sets' names and the other assets in its folder.</summary>
        public static List<string> TakenNames(SceneSet _set)
        {
            List<string> taken = new List<string>();
            string ownPath = AssetDatabase.GetAssetPath(_set);
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(SceneSet)))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path != ownPath)
                {
                    AddOnce(taken, Path.GetFileNameWithoutExtension(path));
                }
            }
            string folder = Path.GetDirectoryName(ownPath);
            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                foreach (string file in Directory.GetFiles(folder, "*.asset"))
                {
                    string path = file.Replace('\\', '/');
                    if (path != ownPath)
                    {
                        AddOnce(taken, Path.GetFileNameWithoutExtension(path));
                    }
                }
            }
            return taken;
        }

        /// <summary>Why the scene set can't be called this, in one sentence; empty when it can (its own name included).</summary>
        public static string Problem(SceneSet _set, string _newName)
        {
            if (_set == null)
            {
                return "The scene set no longer exists.";
            }
            if (_newName == _set.name)
            {
                return string.Empty;
            }
            return SceneSetNames.Problem(_newName, TakenNames(_set));
        }

        /// <summary>Renames the asset file. Returns false with the reason when the name can't be used or Unity refuses.</summary>
        public static bool Rename(SceneSet _set, string _newName, out string _error)
        {
            _error = Problem(_set, _newName);
            if (!string.IsNullOrEmpty(_error))
            {
                return false;
            }
            if (_newName == _set.name)
            {
                return true;
            }
            string path = AssetDatabase.GetAssetPath(_set);
            string unityError = AssetDatabase.RenameAsset(path, _newName);
            if (!string.IsNullOrEmpty(unityError))
            {
                _error = unityError;
                return false;
            }
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("[Scene system] Renamed scene set to '" + _newName + "' (" + AssetDatabase.GetAssetPath(_set) + ").", _set);
            return true;
        }

        private static void AddOnce(List<string> _names, string _name)
        {
            if (!_names.Contains(_name))
            {
                _names.Add(_name);
            }
        }
    }
}
