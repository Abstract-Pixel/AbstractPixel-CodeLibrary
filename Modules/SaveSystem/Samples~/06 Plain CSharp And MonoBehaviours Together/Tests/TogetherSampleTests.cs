using System.Collections;
using AbstractPixel.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SaveSystemSamples.Together.Tests
{
    public class TogetherSampleTests
    {
        const string SceneName = "Plain CSharp And MonoBehaviours Together";

        [UnityTest]
        public IEnumerator PickupAndQuestLog_AreBothRestoredAfterAReload()
        {
            yield return SampleScene.Load(SceneName);
            TogetherScreen screen = Object.FindAnyObjectByType<TogetherScreen>();
            screen.ResetSample();
            Pickup chosen = screen.Pickups[1];
            chosen.Collect(screen.QuestLog);
            SaveSystem.SaveAll();

            yield return SampleScene.Load(SceneName);

            TogetherScreen reloaded = Object.FindAnyObjectByType<TogetherScreen>();
            Assert.IsFalse(reloaded.Pickups[0].IsCollected);
            Assert.IsTrue(reloaded.Pickups[1].IsCollected);
            Assert.IsFalse(reloaded.Pickups[2].IsCollected);
            CollectionAssert.AreEqual(new[] { $"Found the {chosen.DisplayName}" }, reloaded.QuestLog.CompletedQuests);
        }
    }
}
