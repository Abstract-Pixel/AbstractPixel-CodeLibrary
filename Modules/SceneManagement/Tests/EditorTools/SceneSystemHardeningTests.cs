using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    public class OptionalPackageTests
    {
        private const string Library = "Packages/com.abstractpixel.library/";

        private static string Read(string _path)
        {
            return File.ReadAllText(FileUtil.GetPhysicalPath(Library + _path));
        }

        [Test]
        public void TheLibraryDoesNotForceAddressablesOnAProject()
        {
            StringAssert.DoesNotContain("com.unity.addressables", Read("package.json"));
        }

        [TestCase("Modules/SceneManagement/Runtime/AbstractPixel.SceneManagement.asmdef", "com.unity.addressables", "ABSTRACTPIXEL_ADDRESSABLES")]
        [TestCase("Modules/SceneTransitionSystem/Runtime/AbstractPixel.SceneTransitions.asmdef", "com.ami.broaudio", "ABSTRACTPIXEL_BROAUDIO")]
        public void AnOptionalPackageHasAVersionDefine(string _asmdef, string _package, string _define)
        {
            string text = Read(_asmdef);
            StringAssert.Contains(_package, text);
            StringAssert.Contains(_define, text);
        }

        [Test]
        public void WithoutAddressablesTheMissingSceneErrorNamesThePackage()
        {
            string message = DefaultSceneLoader.MissingFromBuildMessage("Cellar", false);

            StringAssert.Contains("Cellar", message);
            StringAssert.Contains("com.unity.addressables", message);
        }
    }

    public class SavedSceneGroupTests
    {
        [Test]
        public void ARestoredGroupKeepsEachScenesGuidAndPath()
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            SceneReference main = new SceneReference("Cellar", "0123456789abcdef0123456789abcdef", "Assets/Levels/Cellar.unity");
            SceneReference shared = new SceneReference("Hud", "fedcba9876543210fedcba9876543210", "Assets/Shared/Hud.unity");
            group.Initialize(new System.Collections.Generic.List<SceneReference>(), new System.Collections.Generic.List<SceneReference> { shared }, main, true);
            string json = JsonUtility.ToJson(new SceneGroupData(group));
            SceneGroupData loaded = JsonUtility.FromJson<SceneGroupData>(json);

            SceneGroup restored = loaded.ToSceneGroup(loaded);

            Assert.AreEqual(main.SceneGuid, restored.MainScene.SceneGuid);
            Assert.AreEqual(main.ScenePath, restored.MainScene.ScenePath);
            Assert.AreEqual(shared.SceneGuid, restored.ContextualBootScenesList[0].SceneGuid);
            Object.DestroyImmediate(restored);
            Object.DestroyImmediate(group);
        }

        [Test]
        public void SavedDataWithNoMainSceneRestoresNothingAndSaysSoOnce()
        {
            SceneGroupData broken = new SceneGroupData();
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("^\\[Scene system\\].*no main scene"));
            int before = Resources.FindObjectsOfTypeAll<SceneGroup>().Length;

            SceneGroup restored = broken.ToSceneGroup(broken);

            Assert.IsNull(restored);
            Assert.AreEqual(before, Resources.FindObjectsOfTypeAll<SceneGroup>().Length, "no ScriptableObject is left behind");
        }
    }

    public class SceneSystemPublicApiTests
    {
        [Test]
        public void ACustomLoaderCanBeSetFromGameCode()
        {
            MethodInfo method = typeof(SceneActions).GetMethod("SetSceneLoader", BindingFlags.Public | BindingFlags.Static);

            Assert.IsNotNull(method);
        }

        [Test]
        public void ReadingSceneActionsDoesNotCreateACoordinator()
        {
            System.Func<bool> isPlaying = SceneCoordinator.IsPlaying;
            SceneCoordinator.IsPlaying = () => true;
            int before = Resources.FindObjectsOfTypeAll<SceneCoordinator>().Length;
            try
            {
                object group = SceneActions.ActiveSceneGroup;
                object preloaded = SceneActions.PreloadedSceneGroup;
                bool loading = SceneActions.IsLoadingSceneGroup;

                Assert.AreEqual(before, Resources.FindObjectsOfTypeAll<SceneCoordinator>().Length);
            }
            finally
            {
                SceneCoordinator.IsPlaying = isPlaying;
                foreach (SceneCoordinator made in Object.FindObjectsByType<SceneCoordinator>(FindObjectsSortMode.None))
                {
                    if (made.gameObject.name == "SceneCoordinator" && made.gameObject.scene.path == string.Empty)
                    {
                        Object.DestroyImmediate(made.gameObject);
                    }
                }
            }
        }
    }

    public class SceneGroupsDebuggerUndoTests
    {
        [Test]
        public void TheCheckBeforeBuildToggleCanBeUndone()
        {
            SceneSystemSettings settings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            settings.CheckSceneGroupsBeforeBuild = true;
            Undo.IncrementCurrentGroup();

            SceneGroupsDebugger.SetCheckBeforeBuild(settings, false);
            Undo.PerformUndo();

            Assert.IsTrue(settings.CheckSceneGroupsBeforeBuild);
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void MakeDefaultCanBeUndone()
        {
            SceneSystemSettings settings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            SceneSet first = ScriptableObject.CreateInstance<SceneSet>();
            SceneSet second = ScriptableObject.CreateInstance<SceneSet>();
            settings.DefaultSceneSet = first;
            Undo.IncrementCurrentGroup();

            SceneSystemSettingsProvider.MakeDefault(settings, second);
            Undo.PerformUndo();

            Assert.AreSame(first, settings.DefaultSceneSet);
            Object.DestroyImmediate(settings);
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }
    }

    public class SceneSystemNamingTests
    {
        [Test]
        public void NoFileFolderOrAssemblyKeepsAKnownTypo()
        {
            string root = FileUtil.GetPhysicalPath("Packages/com.abstractpixel.library");
            Regex typo = new Regex("SceneManagment|ScemeLoader|Singltons|Utilties|Utiltiies");
            System.Collections.Generic.List<string> offenders = new System.Collections.Generic.List<string>();
            foreach (string folder in new[] { "Core", "Modules/SceneManagment", "Modules/SceneManagement", "Modules/SceneTransitionSystem" })
            {
                string start = Path.Combine(root, folder);
                if (!Directory.Exists(start))
                {
                    continue;
                }
                foreach (string entry in Directory.GetFileSystemEntries(start, "*", SearchOption.AllDirectories))
                {
                    string relative = entry.Substring(root.Length).Replace('\\', '/');
                    if (typo.IsMatch(relative) && !relative.EndsWith(".meta") && !relative.Contains("~"))
                    {
                        offenders.Add(relative);
                    }
                }
            }
            CollectionAssert.IsEmpty(offenders);
        }

        [Test]
        public void NoSerializedFileStoresTheOldModuleName()
        {
            string root = FileUtil.GetPhysicalPath("Packages/com.abstractpixel.library");
            System.Collections.Generic.List<string> offenders = new System.Collections.Generic.List<string>();
            foreach (string file in Directory.GetFiles(Path.Combine(root, "Modules"), "*", SearchOption.AllDirectories))
            {
                bool isSerialized = file.EndsWith(".asset") || file.EndsWith(".unity") || file.EndsWith(".prefab");
                if (isSerialized && File.ReadAllText(file).Contains("SceneManagment"))
                {
                    offenders.Add(file.Substring(root.Length).Replace('\\', '/'));
                }
            }
            CollectionAssert.IsEmpty(offenders);
        }

        [Test]
        public void TheAbstractTransitionProfileHasNoCreateMenu()
        {
            object[] menus = typeof(AbstractPixel.SceneTransitions.TransitionProfile).GetCustomAttributes(typeof(CreateAssetMenuAttribute), false);

            Assert.IsEmpty(menus);
        }
    }
}
