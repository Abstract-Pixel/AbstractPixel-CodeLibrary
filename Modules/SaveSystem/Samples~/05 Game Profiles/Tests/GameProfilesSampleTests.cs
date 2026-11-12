using System;
using System.Collections;
using System.Linq;
using AbstractPixel.SaveSystem;
using NUnit.Framework;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace SaveSystemSamples.GameProfiles.Tests
{
    public class GameProfilesSampleTests
    {
        const string SceneName = "Game Profiles";

        [UnityTest]
        public IEnumerator CreateOpenSaveAndDelete_AGameProfile()
        {
            yield return SampleScene.Load(SceneName);
            string previousGameProfile = SaveSystem.GameProfiles.CurrentGameProfileId;
            string displayName = "Test Slot " + Guid.NewGuid().ToString("N").Substring(0, 6);

            string gameProfileId = SaveSystem.GameProfiles.CreateNew(displayName);
            Assert.IsTrue(SaveSystem.GameProfiles.GetAll().Any(manifest => manifest.DisplayName == displayName));

            SaveSystem.GameProfiles.Open(gameProfileId);
            yield return SampleScene.Load(SceneName);
            Assert.AreEqual(0, Object.FindAnyObjectByType<Score>().Points, "A new game profile starts with new-game values.");
            Object.FindAnyObjectByType<Score>().Add(100);
            SaveSystem.SaveAll();

            yield return SampleScene.Load(SceneName);
            Assert.AreEqual(100, Object.FindAnyObjectByType<Score>().Points);
            Assert.IsTrue(SaveSystem.GameProfiles.TryGetManifest(gameProfileId, out GameProfileManifest manifest));
            Assert.IsFalse(string.IsNullOrEmpty(manifest.LastSavedUtc), "Saving updates the manifest's last saved time.");

            SaveSystem.GameProfiles.Delete(gameProfileId);
            Assert.IsFalse(SaveSystem.GameProfiles.GetAll().Any(slot => slot.GameProfileId == gameProfileId));

            SaveSystem.GameProfiles.Open(previousGameProfile);
        }
    }
}
