using System.IO;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// A4 changes how a scene reference is serialized. A scene group saved in the old format (two backing fields, the
    /// scene name and the scene asset) must still list its scenes, and answer with the scene asset's GUID.
    /// </summary>
    public class SceneGroupOldFormatTests
    {
        private const string TestFolder = "Assets/A4OldFormatTests";
        private const string SceneGroupScriptGuid = "f376c00b5d210e54d9f6f50518eaa4dd";

        [SetUp]
        public void CreateFolder()
        {
            AssetDatabase.DeleteAsset(TestFolder);
            AssetDatabase.CreateFolder("Assets", "A4OldFormatTests");
        }

        [TearDown]
        public void DeleteFolder()
        {
            AssetDatabase.DeleteAsset(TestFolder);
        }

        private static string SaveSceneAndGetGuid(string _name)
        {
            string path = TestFolder + "/" + _name + ".unity";
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, path);
            return AssetDatabase.AssetPathToGUID(path);
        }

        private static string OldFieldYaml(string _sceneName, string _sceneGuid)
        {
            return "    <SceneAssetReference>k__BackingField:\n"
                 + "      m_AssetGUID: \n"
                 + "      m_SubObjectName: \n"
                 + "      m_SubObjectType: \n"
                 + "      m_SubObjectGUID: \n"
                 + "      m_EditorAssetChanged: 0\n"
                 + "    <SceneFieldReference>k__BackingField:\n"
                 + "      sceneName: " + _sceneName + "\n"
                 + "      sceneAsset: {fileID: 102900000, guid: " + _sceneGuid + ", type: 3}\n";
        }

        private static string OldGroupYaml(string _managerGuid, string _mainGuid)
        {
            return "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!114 &11400000\nMonoBehaviour:\n"
                 + "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n"
                 + "  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 0}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
                 + "  m_Script: {fileID: 11500000, guid: " + SceneGroupScriptGuid + ", type: 3}\n"
                 + "  m_Name: OldFormatGroup\n  m_EditorClassIdentifier: \n"
                 + "  ManagerialBootScenesList:\n  - " + OldFieldYaml("OldManager", _managerGuid).TrimStart()
                 + "  ContextualBootScenesList: []\n"
                 + "  MainScene:\n" + OldFieldYaml("OldMain", _mainGuid)
                 + "  ForceReloadContextualScenes: 1\n";
        }

        [Test]
        public void AGroupSavedInTheOldFormatStillListsItsScenes()
        {
            string managerGuid = SaveSceneAndGetGuid("OldManager");
            string mainGuid = SaveSceneAndGetGuid("OldMain");
            string groupPath = TestFolder + "/OldFormatGroup.asset";
            File.WriteAllText(groupPath, OldGroupYaml(managerGuid, mainGuid));
            AssetDatabase.ImportAsset(groupPath);

            SceneGroup group = AssetDatabase.LoadAssetAtPath<SceneGroup>(groupPath);

            Assert.IsNotNull(group, "the old format group must load");
            Assert.AreEqual("OldMain", group.MainScene.SceneName);
            Assert.AreEqual(1, group.ManagerialBootScenesList.Count);
            Assert.AreEqual("OldManager", group.ManagerialBootScenesList[0].SceneName);
            Assert.IsTrue(group.ForceReloadContextualScenes, "force reload must survive");
            Assert.AreEqual(mainGuid, group.MainScene.SceneGuid, "the GUID comes from the scene asset the old format kept");
        }
    }
}
