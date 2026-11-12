using System.Collections;
using AbstractPixel.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SaveSystemSamples.ScriptableObjectSample.Tests
{
    public class ScriptableObjectSampleTests
    {
        const string SceneName = "ScriptableObject";

        [UnityTest]
        public IEnumerator GameProgress_IsRestoredIntoTheAsset()
        {
            yield return SampleScene.Load(SceneName);
            GameProgress progress = Object.FindAnyObjectByType<GameProgressScreen>().Progress;
            int levels = Random.Range(1, 1000);
            progress.LevelsCompleted = levels;
            progress.Stars = 3;
            SaveSystem.SaveAll();

            // Like a fresh launch: the asset starts at its new-game values until the save system restores it.
            progress.ResetToNewGame();
            yield return SampleScene.Load(SceneName);

            Assert.AreEqual(levels, progress.LevelsCompleted);
            Assert.AreEqual(3, progress.Stars);
        }
    }
}
