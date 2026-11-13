using System.Collections;
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
        public IEnumerator CreateOpenSaveAndDelete_AGameProfile_BySlotNumber()
        {
            yield return SampleScene.Load(SceneName);
            GameProfile previousGameProfile = SaveActions.GameProfiles.Current;
            SaveSlotScreen screen = Object.FindAnyObjectByType<SaveSlotScreen>();

            GameProfile created = screen.CreateGameProfile();
            int slotNumber = created.SlotNumber;
            Assert.AreSame(created, SaveActions.GameProfiles.Get(slotNumber));
            StringAssert.EndsWith(" " + slotNumber, created.DisplayName, "A new game profile is named from the config and its slot number.");

            created.Open();
            yield return SampleScene.Load(SceneName);
            screen = Object.FindAnyObjectByType<SaveSlotScreen>();
            Assert.AreEqual(0, Object.FindAnyObjectByType<Score>().Points, "A new game profile starts with new-game values.");
            screen.AddPoints();
            screen.Save();

            yield return SampleScene.Load(SceneName);
            Assert.AreEqual(100, Object.FindAnyObjectByType<Score>().Points);
            Assert.IsNotNull(created.LastSaved, "Saving updates the game profile's last saved time.");

            created.Delete();
            Assert.IsNull(SaveActions.GameProfiles.Get(slotNumber));

            if (previousGameProfile != null)
            {
                previousGameProfile.Open();
            }
        }
    }
}
