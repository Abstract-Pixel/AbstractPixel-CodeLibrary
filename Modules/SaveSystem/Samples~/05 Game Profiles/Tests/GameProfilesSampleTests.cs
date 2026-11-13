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
            string previousGameProfile = SaveActions.GameProfiles.CurrentGameProfileId;
            string displayName = "Test Slot " + Guid.NewGuid().ToString("N").Substring(0, 6);

            string gameProfileId = SaveActions.GameProfiles.CreateNew(displayName);
            Assert.IsTrue(SaveActions.GameProfiles.GetAll().Any(manifest => manifest.DisplayName == displayName));

            SaveActions.GameProfiles.Open(gameProfileId);
            yield return SampleScene.Load(SceneName);
            Assert.AreEqual(0, Object.FindAnyObjectByType<Score>().Points, "A new game profile starts with new-game values.");
            Object.FindAnyObjectByType<Score>().Add(100);
            SaveActions.SaveAll();

            yield return SampleScene.Load(SceneName);
            Assert.AreEqual(100, Object.FindAnyObjectByType<Score>().Points);
            Assert.IsTrue(SaveActions.GameProfiles.TryGetManifest(gameProfileId, out GameProfileManifest manifest));
            Assert.IsFalse(string.IsNullOrEmpty(manifest.LastSavedUtc), "Saving updates the manifest's last saved time.");

            SaveActions.GameProfiles.Delete(gameProfileId);
            Assert.IsFalse(SaveActions.GameProfiles.GetAll().Any(slot => slot.GameProfileId == gameProfileId));

            SaveActions.GameProfiles.Open(previousGameProfile);
        }
    }
}
