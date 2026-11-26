#if ABSTRACTPIXEL_SAMPLES_ADDRESSABLES
using System.Collections;
using AbstractPixel.Core;
using AbstractPixel.Core.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>
    /// Sample 08: with its main scene made Addressable (the group's scene reference, by GUID), ChangeTo the Addressable
    /// group gives Done and the scene is open. The entry is removed again afterwards, so the project is left as it was.
    /// </summary>
    public class Sample08AddressablesTests
    {
        private const string Groups = "Packages/com.abstractpixel.library/Modules/SceneManagement/Samples/08 Addressables/Scene Groups/";

        private string sceneGuid;

        [UnityTest]
        public IEnumerator ChangeToTheAddressableGroup()
        {
            SceneGroup start = AssetDatabase.LoadAssetAtPath<SceneGroup>(Groups + "Sample 08 StartGroup.asset");
            SceneGroup level = AssetDatabase.LoadAssetAtPath<SceneGroup>(Groups + "Sample 08 Addressable LevelGroup.asset");
            SceneReference mainScene = level.MainScene;
            Assert.IsTrue(mainScene.HasGuid, "the group's scene reference has a GUID");
            sceneGuid = mainScene.SceneGuid;
            Assert.IsTrue(AddressableScenes.IsInstalled, "Addressables is installed");
            bool made = AddressableScenes.Provider.TryMakeAddressable(sceneGuid, out string error);
            Assert.IsTrue(made, error);

            SampleStepResult step = new SampleStepResult();
            yield return SampleTestSteps.OpenGroup(start, step);
            Assert.IsNotNull(step.Result, step.Problem);
            Assert.IsTrue(step.Result.IsDone, step.Result.ToString());

            SampleStepResult change = new SampleStepResult();
            yield return SampleTestSteps.WaitFor(SceneActions.ChangeTo(level), change);
            Assert.IsNotNull(change.Result, change.Problem);
            Assert.IsTrue(change.Result.IsDone, change.Result.ToString());
            Assert.IsTrue(SceneManager.GetSceneByName(mainScene.SceneName).isLoaded, mainScene.SceneName + " is open");
        }

        [TearDown]
        public void RemoveTheEntry()
        {
            if (string.IsNullOrEmpty(sceneGuid) || AddressableScenes.Provider == null)
            {
                return;
            }
            AddressableScenes.Provider.RemoveFromAddressables(sceneGuid);
        }
    }
}
#endif
