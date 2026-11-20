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
            AssetDatabase.CreateAsset(group, groupPath);

            // Build Settings change before the save, so the save-time fix finds nothing left to do and logs nothing.
            BuildSettingsChange change = SceneGroupBuildSettings.ApplyToProject(group, false);
            AssetDatabase.SaveAssets();

            Debug.Log(LogPrefix + " Created scene group '" + group.name + "' with main scene '" + _scene.name + "'; " + DescribeBuild(_scene.name, change), group);
            SelectAndPing(group);
            return group;
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
