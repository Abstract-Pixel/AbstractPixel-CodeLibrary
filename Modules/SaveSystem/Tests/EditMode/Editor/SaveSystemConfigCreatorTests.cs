using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using UnityEditor;

namespace AbstractPixel.SaveSystem.Tests
{
    // B8: Tools > External Project Tools > Save System > Create Config makes the config for you.
    public class SaveSystemConfigCreatorTests
    {
        const string TestFolder = "Assets/SaveSystemCreateConfigTest";
        const string TestConfigPath = TestFolder + "/Resources/SaveSystemConfig.asset";

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TestFolder);
        }

        [Test]
        public void CreateOrFind_MakesTheConfigAndItsFolders_WithTheBuiltInRowsFirst()
        {
            SaveSystemConfigSO config = SaveSystemConfigCreator.CreateOrFind(TestConfigPath);

            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SaveSystemConfigSO>(TestConfigPath));
            Assert.AreEqual(BuiltInSaveFile.Global, config.SaveFileList[0].BuiltIn);
            Assert.AreEqual(BuiltInSaveFile.GameProfile, config.SaveFileList[1].BuiltIn);
            Assert.AreEqual(BuiltInSaveFile.Scene, config.SaveFileList[2].BuiltIn);
        }

        [Test]
        public void CreateOrFind_WhenTheConfigExists_ReturnsItAndMakesNoOther()
        {
            SaveSystemConfigSO first = SaveSystemConfigCreator.CreateOrFind(TestConfigPath);

            SaveSystemConfigSO second = SaveSystemConfigCreator.CreateOrFind(TestConfigPath);

            Assert.AreSame(first, second);
            Assert.AreEqual(1, AssetDatabase.FindAssets("t:SaveSystemConfigSO", new[] { TestFolder }).Length);
        }
    }
}
