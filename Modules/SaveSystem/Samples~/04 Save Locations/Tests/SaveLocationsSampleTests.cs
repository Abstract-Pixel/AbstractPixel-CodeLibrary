using System.Collections;
using AbstractPixel.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SaveSystemSamples.SaveLocations.Tests
{
    public class SaveLocationsSampleTests
    {
        const string Forest = "Save Locations Forest";
        const string Cave = "Save Locations Cave";

        static Chest FirstChest()
        {
            foreach (Chest chest in Object.FindObjectsByType<Chest>(FindObjectsInactive.Exclude))
            {
                if (chest.name == "Chest 1")
                {
                    return chest;
                }
            }
            return null;
        }

        [UnityTest]
        public IEnumerator SwitchingGameProfiles_KeepsGlobalData_AndChangesTheRest()
        {
            yield return SampleScene.Load(Forest);
            string firstGameProfile = SaveActions.GameProfiles.CurrentGameProfileId;
            Object.FindAnyObjectByType<VolumeSetting>().VolumeDown();
            int volume = Object.FindAnyObjectByType<VolumeSetting>().Volume;
            Object.FindAnyObjectByType<PlayerCoins>().Add(5);
            int coins = Object.FindAnyObjectByType<PlayerCoins>().Coins;
            FirstChest().Open();

            // A scene change keeps the chest's state in memory, even before saving.
            yield return SampleScene.Load(Cave);
            yield return SampleScene.Load(Forest);
            Assert.IsTrue(FirstChest().IsOpen, "The chest should still be open after visiting the cave.");
            SaveActions.SaveAll();

            string secondGameProfile = SaveActions.GameProfiles.CreateNew("Save Locations Test");
            SaveActions.GameProfiles.Open(secondGameProfile);
            yield return SampleScene.Load(Forest);

            Assert.AreEqual(volume, Object.FindAnyObjectByType<VolumeSetting>().Volume, "Global data is shared by every game profile.");
            Assert.AreEqual(0, Object.FindAnyObjectByType<PlayerCoins>().Coins, "A new game profile starts with new-game coins.");
            Assert.IsFalse(FirstChest().IsOpen, "A new game profile starts with closed chests.");

            SaveActions.GameProfiles.Open(firstGameProfile);
            yield return SampleScene.Load(Forest);

            Assert.AreEqual(coins, Object.FindAnyObjectByType<PlayerCoins>().Coins);
            Assert.IsTrue(FirstChest().IsOpen);

            SaveActions.GameProfiles.Delete(secondGameProfile);
        }
    }
}
