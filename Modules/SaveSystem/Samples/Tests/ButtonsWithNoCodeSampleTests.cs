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

            // Turning the lamp back on works after a reload too.
            Press("Turn The Lamp On Button");
            yield return null;
            Assert.IsTrue(FindIncludingInactive("Lamp").activeSelf);

            testGameProfile.Delete();
            if (previousGameProfile != null)
            {
                previousGameProfile.Open();
            }
        }

        static void Press(string buttonName)
        {
            GameObject buttonObject = GameObject.Find(buttonName);
            if (buttonObject == null)
            {
                return;
            }
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
