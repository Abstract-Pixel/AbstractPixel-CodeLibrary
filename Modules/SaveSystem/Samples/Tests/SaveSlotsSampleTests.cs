using System.Collections;
using AbstractPixel.SaveSystem;
using NUnit.Framework;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AbstractPixel.SaveSystem.Samples.SaveSlots.Tests
{
    public class SaveSlotsSampleTests
    {
        const string SceneName = "Save Slots And Continue";

        [UnityTest]
        public IEnumerator CreateOpenSaveAndDelete_AGameProfile_BySlotNumber()
        {
            yield return SampleScenes.LoadAndWait(SceneName);
            GameProfile previousGameProfile = SaveActions.GameProfiles.Current;
            SaveSlotScreen screen = Object.FindAnyObjectByType<SaveSlotScreen>();

            GameProfile created = screen.CreateGameProfile();
            int slotNumber = created.SlotNumber;
            Assert.AreSame(created, SaveActions.GameProfiles.Get(slotNumber));
            StringAssert.EndsWith(" " + slotNumber, created.DisplayName, "A new game profile is named from the config and its slot number.");

            created.Open();
            yield return SampleScenes.LoadAndWait(SceneName);
            screen = Object.FindAnyObjectByType<SaveSlotScreen>();
            Assert.AreEqual(0, Object.FindAnyObjectByType<Score>().Points, "A new game profile starts with new-game values.");
            screen.AddPoints();
            screen.Save();

            yield return SampleScenes.LoadAndWait(SceneName);
            Assert.AreEqual(100, Object.FindAnyObjectByType<Score>().Points);
            Assert.IsNotNull(created.LastSaved, "Saving updates the game profile's last saved time.");

            created.Delete();
            Assert.IsNull(SaveActions.GameProfiles.Get(slotNumber));

            if (previousGameProfile != null)
            {
                previousGameProfile.Open();
            }
        }

        [UnityTest]
        public IEnumerator Continue_ReopensTheLastGameProfile_AfterGoingBackToTheMainMenu()
        {
            yield return SampleScenes.LoadAndWait(SceneName);
            GameProfile previousGameProfile = SaveActions.GameProfiles.Current;
            GameProfile created = SaveActions.GameProfiles.CreateNew("Continue Test");
            created.Open();
            yield return SampleScenes.LoadAndWait(SceneName);
            SaveSlotScreen screen = Object.FindAnyObjectByType<SaveSlotScreen>();
            screen.AddPoints();

            screen.BackToMainMenu();
            Assert.IsNull(SaveActions.GameProfiles.Current, "Back to the main menu closes the game profile.");
            Assert.AreSame(created, SaveActions.GameProfiles.LastOpened, "Closing keeps it as the one Continue opens.");

            screen.Continue();
            yield return SampleScenes.LoadAndWait(SceneName);
            Assert.AreSame(created, SaveActions.GameProfiles.Current);
            Assert.AreEqual(100, Object.FindAnyObjectByType<Score>().Points, "Back to the main menu saved the points first.");

            created.Delete();
            if (previousGameProfile != null)
            {
                previousGameProfile.Open();
            }
        }
    }
}
