using System;
using System.Collections.Generic;
using AbstractPixel.Core.Editor;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Sample scene groups (ADR 0014): finds them and their scenes, keeps every writer from putting their scenes into
    /// Build Settings, and lets the default loader open them in Play mode from their path. The list is read again after
    /// any project change.
    /// </summary>
    [InitializeOnLoad]
    public static class SampleSceneGroups
    {
        /// <summary>Every scene group in the project. Tests replace it.</summary>
        internal static Func<List<SceneGroup>> FindGroups = SceneGroupAssets.FindAll;

        private static Dictionary<string, SceneGroup> sampleSceneOwners;
        private static Dictionary<string, SceneGroup> gameSceneOwners;

        static SampleSceneGroups()
        {
            SceneBuildSettingsWriter.ScenePathsNeverInBuild = ScenePaths;
            DefaultSceneLoader.IsEditorOnlySampleScene = IsSampleScene;
            EditorApplication.projectChanged += Forget;
        }

        /// <summary>Drops the cached list, so the next look reads the project again.</summary>
        public static void Forget()
        {
            sampleSceneOwners = null;
            gameSceneOwners = null;
        }

        public static List<SceneGroup> All()
        {
            List<SceneGroup> samples = new List<SceneGroup>();
            foreach (SceneGroup group in FindGroups())
            {
                if (group != null && group.IsSample)
                {
                    samples.Add(group);
                }
            }
            return samples;
        }

        /// <summary>The scene paths used by sample scene groups.</summary>
        public static HashSet<string> ScenePaths()
        {
            return new HashSet<string>(Owners().Keys);
        }

        public static bool IsSampleScene(string _scenePath)
        {
            return !string.IsNullOrEmpty(_scenePath) && Owners().ContainsKey(_scenePath);
        }

        /// <summary>The sample scene group that uses a scene, or null.</summary>
        public static SceneGroup SampleUsing(string _scenePath)
        {
            Owners().TryGetValue(_scenePath, out SceneGroup owner);
            return owner;
        }

        /// <summary>The first group of the other kind (game for a sample, sample for a game) that uses the same scene, or null.</summary>
        public static SceneGroup OtherKindUsing(SceneGroup _group, string _scenePath)
        {
            if (_group == null || string.IsNullOrEmpty(_scenePath))
            {
                return null;
            }
            Owners();
            Dictionary<string, SceneGroup> otherKind = _group.IsSample ? gameSceneOwners : sampleSceneOwners;
            otherKind.TryGetValue(_scenePath, out SceneGroup other);
            return other;
        }

        // Each scene path with the first sample and the first game scene group that uses it, read once per project change.
        private static Dictionary<string, SceneGroup> Owners()
        {
            if (sampleSceneOwners != null)
            {
                return sampleSceneOwners;
            }
            sampleSceneOwners = new Dictionary<string, SceneGroup>();
            gameSceneOwners = new Dictionary<string, SceneGroup>();
            foreach (SceneGroup group in FindGroups())
            {
                if (group == null)
                {
                    continue;
                }
                Dictionary<string, SceneGroup> owners = group.IsSample ? sampleSceneOwners : gameSceneOwners;
                foreach (string path in SceneGroupBuildSettings.GroupScenePaths(group))
                {
                    if (!owners.ContainsKey(path))
                    {
                        owners.Add(path, group);
                    }
                }
            }
            return sampleSceneOwners;
        }
    }
}
