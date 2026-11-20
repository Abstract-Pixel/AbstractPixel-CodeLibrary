using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>The setup's folder for the scene system settings asset: inside Assets/, valid names, not an Editor folder.</summary>
    public class SceneSystemSetupTests
    {
        [TestCase("Assets/SceneSystem")]
        [TestCase("Assets")]
        [TestCase("Assets/[1]Project/ProjectServices/SceneSystem")]
        [TestCase("Assets/_Temp4A")]
        public void FolderProblem_ValidFolder_IsEmpty(string _folder)
        {
            Assert.AreEqual(string.Empty, SceneSystemSetup.FolderProblem(_folder));
        }

        [TestCase("", "Pick")]
        [TestCase("   ", "Pick")]
        [TestCase("Packages/com.example/Scenes", "inside Assets")]
        [TestCase("C:/Projects/Scenes", "inside Assets")]
        [TestCase("assets/SceneSystem", "inside Assets")]
        [TestCase("AssetsExtra/SceneSystem", "inside Assets")]
        [TestCase("Assets/../SceneSystem", "..")]
        [TestCase("Assets/Editor/SceneSystem", "Editor")]
        [TestCase("Assets/Tools/Editor", "Editor")]
        [TestCase("Assets/Scene:System", "character")]
        [TestCase("Assets//SceneSystem", "empty")]
        [TestCase("Assets/SceneSystem.", "end")]
        public void FolderProblem_InvalidFolder_SaysWhy(string _folder, string _expectedWord)
        {
            string problem = SceneSystemSetup.FolderProblem(_folder);

            StringAssert.Contains(_expectedWord, problem);
        }

        [TestCase("Assets\\SceneSystem\\", "Assets/SceneSystem")]
        [TestCase(" Assets/SceneSystem/ ", "Assets/SceneSystem")]
        [TestCase("Assets", "Assets")]
        public void NormalizeFolder_ForwardSlashesNoTrailingSlash(string _folder, string _expected)
        {
            Assert.AreEqual(_expected, SceneSystemSetup.NormalizeFolder(_folder));
        }
    }
}
