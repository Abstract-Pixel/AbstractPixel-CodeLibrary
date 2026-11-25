using AbstractPixel.Core;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// "Create Scene Group" on a scene in the Project window: a new scene group next to the scene with it as the main
    /// scene, and the scene put into Build Settings unless it is Addressable. One console line names the group and scene.
    /// </summary>
    public static class CreateSceneGroupMenu
    {
        private const string MenuPath = "Assets/Create/Scene Group";
        private const string LogPrefix = "[Scene system]";

        [MenuItem(MenuPath, false, 0)]
        private static void CreateFromSelectedScene()
        {
            SceneAsset scene = Selection.activeObject as SceneAsset;
            if (scene == null)
            {
                return;
            }
            CreateFor(scene);
        }

        [MenuItem(MenuPath, true)]
        private static bool SelectionIsAScene()
        {
            return Selection.activeObject is SceneAsset;
        }

        /// <summary>Creates the scene group for this scene, or selects the group that already has it as its main scene.</summary>
        public static SceneGroup CreateFor(SceneAsset _scene)
        {
            string scenePath = AssetDatabase.GetAssetPath(_scene);
            string sceneGuid = AssetDatabase.AssetPathToGUID(scenePath);

            SceneGroup existing = SceneGroupCreation.FindGroupWithMainScene(SceneGroupAssets.FindAll(), sceneGuid);
            if (existing != null)
            {
                Debug.LogWarning(LogPrefix + " Scene '" + _scene.name + "' is already the main scene of scene group '" + existing.name + "' (" + AssetDatabase.GetAssetPath(existing) + "). A scene is the main scene of at most one scene group, so no new group was made.", existing);
                SelectAndPing(existing);
                return existing;
            }

            string groupPath = AssetDatabase.GenerateUniqueAssetPath(SceneGroupCreation.GroupPathFor(scenePath));
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            SceneReference mainScene = new SceneReference(_scene.name, sceneGuid, scenePath);
            mainScene.SceneFieldReference.SetSceneAsset(_scene);
            group.Initialize(new SceneReference[0], new SceneReference[0], mainScene);
            // A scene in a Samples folder gets a sample scene group: it stays out of builds and keeps its own scenes
            // instead of the game's default scene set.
            group.IsSample = SceneGroupCreation.IsInSamplesFolder(scenePath);
            if (!group.IsSample)
            {
                UseDefaultSceneSet(group);
            }
            AssetDatabase.CreateAsset(group, groupPath);
            SampleSceneGroups.Forget();

            // Build Settings change before the save, so the save-time fix finds nothing left to do and logs nothing.
            BuildSettingsChange change = SceneGroupBuildSettings.ApplyToProject(group, false);
            AssetDatabase.SaveAssets();

            Debug.Log(DescribeCreated(group.name, _scene.name, group.IsSample, DescribeBuild(_scene.name, change)), group);
            SelectAndPing(group);
            return group;
        }

        // A new group starts on the default scene set, which fills its manager scenes and shared scenes.
        private static void UseDefaultSceneSet(SceneGroup _group)
        {
            SceneSystemSettings settings = SceneSystemSettingsAssets.Find();
            if (settings == null || settings.DefaultSceneSet == null)
            {
                return;
            }
            _group.SceneSet = settings.DefaultSceneSet;
            SceneSetSync.ApplySet(_group);
        }

        internal static string DescribeCreated(string _groupName, string _sceneName, bool _isSample, string _buildLine)
        {
            if (_isSample)
            {
                return LogPrefix + " Created sample scene group '" + _groupName + "' with main scene '" + _sceneName + "', because the scene is in a Samples folder: its scenes never go into a build and open in the Editor only.";
            }
            return LogPrefix + " Created scene group '" + _groupName + "' with main scene '" + _sceneName + "'; " + _buildLine;
        }

        private static string DescribeBuild(string _sceneName, BuildSettingsChange _change)
        {
            if (_change.AddedPaths.Count > 0)
            {
                return "added '" + _sceneName + "' to Build Settings.";
            }
            if (_change.EnabledPaths.Count > 0)
            {
                return "switched '" + _sceneName + "' on in Build Settings.";
            }
            return "'" + _sceneName + "' was already in build or Addressable.";
        }

        private static void SelectAndPing(SceneGroup _group)
        {
            Selection.activeObject = _group;
            EditorGUIUtility.PingObject(_group);
        }
    }
}
