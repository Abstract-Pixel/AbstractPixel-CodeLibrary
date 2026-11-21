using System;
using System.Collections.Generic;
using System.Text;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Stops any build that still contains a sample scene (ADR 0014), from Build Settings, the active Build Profile or
    /// Addressables, with an error naming each scene and its sample scene group. Stopping is chosen over removing them
    /// silently: a scripted build cannot change its scene list at this point, and the mistake should be seen. The
    /// Scene System page has "Remove sample scenes from the build".
    /// </summary>
    public class SampleSceneBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 1; }
        }

        public void OnPreprocessBuild(BuildReport _report)
        {
            SampleSceneGroups.Forget();
            string message = FailureMessage(ScenesInBuild(), SampleSceneGroups.ScenePaths(), IsAddressable);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            throw new BuildFailedException(message);
        }

        /// <summary>The message that stops the build, naming each sample scene found in it; empty when there is none.</summary>
        public static string FailureMessage(List<string> _scenesInBuild, HashSet<string> _sampleScenes, Func<string, bool> _isAddressable)
        {
            StringBuilder lines = new StringBuilder();
            int count = 0;
            foreach (string samplePath in _sampleScenes)
            {
                bool inSceneList = _scenesInBuild.Contains(samplePath);
                bool addressable = _isAddressable(samplePath);
                if (!inSceneList && !addressable)
                {
                    continue;
                }
                string where = inSceneList ? "Build Settings or the Build Profile" : "Addressables";
                SceneGroup owner = SampleSceneGroups.SampleUsing(samplePath);
                string ownerName = owner != null ? owner.name : "a sample scene group";
                lines.Append("\n- '").Append(System.IO.Path.GetFileNameWithoutExtension(samplePath)).Append("' (sample scene group '").Append(ownerName).Append("') is in ").Append(where).Append(".");
                count++;
            }
            if (count == 0)
            {
                return string.Empty;
            }
            return "[Scene system] The build stopped: " + count + " sample scene(s) would ship. Click 'Remove sample scenes from the build' in Project Settings > Abstract Pixel > Scene System > Sample scene groups." + lines;
        }

        /// <summary>Every enabled scene of Build Settings and of the active Build Profile.</summary>
        public static List<string> ScenesInBuild()
        {
            List<string> paths = new List<string>();
            AddEnabled(EditorBuildSettings.scenes, paths);
            BuildProfile profile = BuildProfile.GetActiveBuildProfile();
            if (profile != null && profile.overrideGlobalScenes)
            {
                AddEnabled(profile.scenes, paths);
            }
            return paths;
        }

        /// <summary>Takes every sample scene out of Build Settings, the active Build Profile and Addressables.</summary>
        public static void RemoveSampleScenesFromBuild()
        {
            SampleSceneGroups.Forget();
            HashSet<string> samples = SampleSceneGroups.ScenePaths();
            SceneBuildSettingsWriter.Write(EditorBuildSettings.scenes);
            BuildProfile profile = BuildProfile.GetActiveBuildProfile();
            if (profile != null && profile.overrideGlobalScenes)
            {
                profile.scenes = SceneBuildSettingsWriter.WithoutScenesNeverInBuild(profile.scenes, samples);
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssetIfDirty(profile);
            }
            IAddressableSceneProvider provider = AddressableScenes.Provider;
            if (provider != null)
            {
                foreach (string path in samples)
                {
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    if (provider.IsAddressable(guid))
                    {
                        provider.RemoveFromAddressables(guid);
                    }
                }
            }
            UnityEngine.Debug.Log("[Scene system] Sample scenes removed from the build.");
        }

        private static void AddEnabled(EditorBuildSettingsScene[] _scenes, List<string> _paths)
        {
            if (_scenes == null)
            {
                return;
            }
            foreach (EditorBuildSettingsScene scene in _scenes)
            {
                if (scene.enabled && !_paths.Contains(scene.path))
                {
                    _paths.Add(scene.path);
                }
            }
        }

        private static bool IsAddressable(string _scenePath)
        {
            IAddressableSceneProvider provider = AddressableScenes.Provider;
            if (provider == null)
            {
                return false;
            }
            return provider.IsAddressable(AssetDatabase.AssetPathToGUID(_scenePath));
        }
    }
}
