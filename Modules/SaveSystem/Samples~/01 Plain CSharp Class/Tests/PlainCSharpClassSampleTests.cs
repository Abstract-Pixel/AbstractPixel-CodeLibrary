using System;
using System.Collections;
using AbstractPixel.SaveSystem;
using NUnit.Framework;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace SaveSystemSamples.PlainCSharpClass.Tests
{
    public class PlainCSharpClassSampleTests
    {
        const string SceneName = "Plain CSharp Class";

        [UnityTest]
        public IEnumerator FoundItem_IsStillThereAfterSavingAndReloading()
        {
            yield return SampleScene.Load(SceneName);
            string item = "Test Gem " + Guid.NewGuid().ToString("N").Substring(0, 6);
            Object.FindAnyObjectByType<InventorySampleScreen>().Inventory.Add(item);
            SaveActions.SaveAll();

            yield return SampleScene.Load(SceneName);

            Inventory inventory = Object.FindAnyObjectByType<InventorySampleScreen>().Inventory;
            CollectionAssert.Contains(inventory.Items, item);

            // Leave the sample's save data as it was.
            inventory.Remove(item);
            SaveActions.SaveAll();
        }
    }
}
