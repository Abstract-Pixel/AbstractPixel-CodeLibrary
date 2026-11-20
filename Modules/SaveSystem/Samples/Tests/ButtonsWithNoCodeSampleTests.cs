using System.Collections;
using AbstractPixel.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AbstractPixel.SaveSystem.Samples.ButtonsWithNoCode.Tests
{
    // The sample has no scripts of its own, so the test presses its buttons the way a player would.
    public class ButtonsWithNoCodeSampleTests
    {
        const string SceneName = "Buttons With No Code";

        // The hint tells the player to drag the crate in the Game view, so the crate must follow the pointer there.
        [UnityTest]
        public IEnumerator TheCrate_FollowsThePointer_WhenDraggedInTheGameView()
        {
            yield return SampleScenes.LoadAndWait(SceneName);
            DragInGameView crate = GameObject.Find("Crate").GetComponent<DragInGameView>();
            Camera viewCamera = Camera.main;
            Vector3 startPosition = crate.transform.position;
            Vector3 crateOnScreen = viewCamera.WorldToScreenPoint(startPosition);

            crate.StartDragging(viewCamera.ScreenPointToRay(crateOnScreen), viewCamera.transform.forward);
            crate.DragAlong(viewCamera.ScreenPointToRay(crateOnScreen + new Vector3(200f, 0f, 0f)));
            crate.StopDragging();

            Assert.Greater(crate.transform.position.x, startPosition.x + 0.5f, "Dragging right moves the crate right.");
            Assert.AreEqual(startPosition.z, crate.transform.position.z, 0.001f, "The crate moves across the screen, not toward the camera.");
        }

        [UnityTest]
        public IEnumerator TheCrateLampAndCoin_KeepTheirState_AfterSavingWithAButton()
        {
            // A picked-up coin stays gone for good, so the test plays in a game profile of its own and deletes it after.
            GameProfile previousGameProfile = SaveActions.GameProfiles.Current;
            GameProfile testGameProfile = SaveActions.GameProfiles.CreateNew("Buttons With No Code Test");
            testGameProfile.Open();
            yield return SampleScenes.LoadAndWait(SceneName);

            GameObject crate = GameObject.Find("Crate");
            Vector3 movedTo = crate.transform.position + new Vector3(Random.Range(1f, 3f), 0f, 0f);
            crate.transform.position = movedTo;
            Press("Turn The Lamp Off Button");
            Assert.IsNotNull(GameObject.Find("Coin"), "A new game profile starts with the coin in place.");
            Press("Pick Up The Coin Button");
            yield return null;
            GameObject savedLabel = FindIncludingInactive("Saved Label");
            savedLabel.SetActive(false);
            Press("Save Button");
            Assert.IsTrue(savedLabel.activeSelf, "Mono Save Actions' On Saved event turns the Saved label on.");

            yield return SampleScenes.LoadAndWait(SceneName);

            Assert.AreEqual(movedTo, GameObject.Find("Crate").transform.position, "Save Transform keeps where the crate was moved.");
            Assert.IsFalse(FindIncludingInactive("Lamp").activeSelf, "Save Active State keeps the lamp off.");
            Assert.IsNull(GameObject.Find("Coin"), "Save Destroyed keeps the picked-up coin gone.");

            // Turning the lamp back on works after a reload too, and saving remembers it.
            Press("Turn The Lamp On Button");
            yield return null;
            Assert.IsTrue(FindIncludingInactive("Lamp").activeSelf);
            savedLabel = FindIncludingInactive("Saved Label");
            savedLabel.SetActive(false);
            Press("Save Button");
            Assert.IsTrue(savedLabel.activeSelf, "Saving after turning the lamp back on shows the Saved label.");

            yield return SampleScenes.LoadAndWait(SceneName);

            Assert.IsTrue(FindIncludingInactive("Lamp").activeSelf, "The lamp was turned back on and saved, so it stays on.");

            testGameProfile.Delete();
            if (previousGameProfile != null)
            {
                previousGameProfile.Open();
            }
        }

        static void Press(string buttonName)
        {
            GameObject buttonObject = GameObject.Find(buttonName);
            Assert.IsNotNull(buttonObject, $"The sample scene has no active button named '{buttonName}'.");
            buttonObject.GetComponent<Button>().onClick.Invoke();
        }

        static GameObject FindIncludingInactive(string objectName)
        {
            foreach (Transform candidate in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (candidate.name == objectName)
                {
                    return candidate.gameObject;
                }
            }
            Assert.Fail($"The sample scene has no object named '{objectName}'.");
            return null;
        }
    }
}
