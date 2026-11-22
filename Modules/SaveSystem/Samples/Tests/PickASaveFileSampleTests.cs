using System.Collections;
using AbstractPixel.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.Samples.PickASaveFile.Tests
{
    public class PickASaveFileSampleTests
    {
        const string Forest = "Pick A Save File Forest";
        const string Cave = "Pick A Save File Cave";

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
            yield return SampleScenes.LoadAndWait(Forest);
            GameProfile firstGameProfile = SaveActions.GameProfiles.Current;
            Object.FindAnyObjectByType<VolumeSetting>().VolumeDown();
            int volume = Object.FindAnyObjectByType<VolumeSetting>().Volume;
            Object.FindAnyObjectByType<PlayerCoins>().Add(5);
            int coins = Object.FindAnyObjectByType<PlayerCoins>().Coins;
            FirstChest().Open();

            // A scene change keeps the chest's state in memory, even before saving.
            yield return SampleScenes.LoadAndWait(Cave);
            yield return SampleScenes.LoadAndWait(Forest);
            Assert.IsTrue(FirstChest().IsOpen, "The chest should still be open after visiting the cave.");
            Assert.AreEqual(1, Object.FindAnyObjectByType<PickASaveFileScreen>().SceneFileLoadCount, "The typed load listener hears this scene's Scene file once.");
            SaveActions.SaveAll();

            GameProfile secondGameProfile = SaveActions.GameProfiles.CreateNew("Pick A Save File Test");
            secondGameProfile.Open();
            yield return SampleScenes.LoadAndWait(Forest);

            Assert.AreEqual(volume, Object.FindAnyObjectByType<VolumeSetting>().Volume, "Global data is shared by every game profile.");
            Assert.AreEqual(0, Object.FindAnyObjectByType<PlayerCoins>().Coins, "A new game profile starts with new-game coins.");
            Assert.IsFalse(FirstChest().IsOpen, "A new game profile starts with closed chests.");

            firstGameProfile.Open();
            yield return SampleScenes.LoadAndWait(Forest);

            Assert.AreEqual(coins, Object.FindAnyObjectByType<PlayerCoins>().Coins);
            Assert.IsTrue(FirstChest().IsOpen);

            secondGameProfile.Delete();
        }

        [UnityTest]
        public IEnumerator SaveOnlyTheSettings_WritesTheVolume_AndLeavesTheCoinsUnsaved()
        {
            yield return SampleScenes.LoadAndWait(Forest);
            PickASaveFileScreen screen = Object.FindAnyObjectByType<PickASaveFileScreen>();
            VolumeSetting volume = Object.FindAnyObjectByType<VolumeSetting>();
            PlayerCoins coins = Object.FindAnyObjectByType<PlayerCoins>();
            screen.SaveEverything();
            int savedCoins = coins.Coins;

            volume.VolumeDown();
            int changedVolume = volume.Volume;
            coins.Add(5);
            screen.SaveOnlyTheSettings();
            screen.UndoUnsavedCoins();

            Assert.AreEqual(savedCoins, coins.Coins, "The coins were never saved, so reloading the GameProfile file puts them back.");
            SaveActions.Reload<SaveFiles.Settings>();
            Assert.AreEqual(changedVolume, volume.Volume, "The settings file was saved on its own, so reloading it keeps the new volume.");
        }
    }
}
