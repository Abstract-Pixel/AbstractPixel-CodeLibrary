using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using AbstractPixel.SaveSystem.Editor;

namespace AbstractPixel.SaveSystem.Tests
{
    // Checks rules from the build plan that span the whole save system.
    public class SaveSystemRulesTests
    {
        const string RuntimeFolder = "Packages/com.abstractpixel.library/Modules/SaveSystem/Runtime";

        [Test]
        public void RuntimeCode_OnlyDiskStorageServiceTouchesFilesAndFolders()
        {
            Regex fileSystemCall = new Regex(@"\b(File|Directory)\.[A-Z]");

            string[] offenders = Directory.GetFiles(Path.GetFullPath(RuntimeFolder), "*.cs", SearchOption.AllDirectories)
                .Where(path => Path.GetFileName(path) != "DiskStorageService.cs")
                .Where(path => fileSystemCall.IsMatch(File.ReadAllText(path)))
                .Select(Path.GetFileName)
                .ToArray();

            CollectionAssert.IsEmpty(offenders, "Only DiskStorageService may use System.IO.File or Directory.");
        }

        [Test]
        public void RuntimeCode_UsesNoCoroutinesFrameWaitsOrSingletons()
        {
            // Step 2 replaced the WaitForEndOfFrame load and the SaveManager singleton with the whichever-second rule.
            Regex forbidden = new Regex(@"WaitForEndOfFrame|StartCoroutine|IEnumerator|PersistentSingleton");

            string[] offenders = Directory.GetFiles(Path.GetFullPath(RuntimeFolder), "*.cs", SearchOption.AllDirectories)
                .Where(path => forbidden.IsMatch(File.ReadAllText(path)))
                .Select(Path.GetFileName)
                .ToArray();

            CollectionAssert.IsEmpty(offenders);
        }

        [Test]
        public void RuntimeFiles_StayUnderAbout400Lines()
        {
            string[] tooLong = Directory.GetFiles(Path.GetFullPath(RuntimeFolder), "*.cs", SearchOption.AllDirectories)
                .Where(path => File.ReadAllLines(path).Length > 400)
                .Select(Path.GetFileName)
                .ToArray();

            CollectionAssert.IsEmpty(tooLong, "Split files longer than about 400 lines with composition.");
        }

        [Test]
        public void EditorTools_DeleteTheSameFoldersTheGameSavesTo()
        {
            // Bug 4: "Delete All Save Files" never set up its paths, so it deleted nothing.
            CollectionAssert.AreEqual(
                new[] { DiskStorageService.GetRootFolder(false), DiskStorageService.GetRootFolder(true) },
                SaveSystemToolsMenu.GetSaveFolders());
        }

        [Test]
        public void SaveFileDefinition_FileNameIsTheStorageNameOrTheName()
        {
            SaveFileDefinition renamed = new SaveFileDefinition { Name = "Phantoms", StorageName = "Ghosts" };
            SaveFileDefinition unnamed = new SaveFileDefinition { Name = "Settings" };

            Assert.AreEqual("Ghosts", renamed.FileName);
            Assert.AreEqual("Settings", unnamed.FileName);
        }

        [Test]
        public void SaveSystem_HasNoCategoriesLeft()
        {
            // Save files are classes now; SaveCategory and [Savable] were removed in B2.
            Assert.IsNull(typeof(SaveActions).Assembly.GetType("AbstractPixel.SaveSystem.SaveCategory"));
            Assert.IsNull(typeof(SaveActions).Assembly.GetType("AbstractPixel.SaveSystem.SavableAttribute"));
        }
    }
}
