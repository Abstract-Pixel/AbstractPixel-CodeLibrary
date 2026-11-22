using System;
using System.Collections;
using AbstractPixel.SaveSystem;
using NUnit.Framework;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AbstractPixel.SaveSystem.Samples.CustomData.Tests
{
    public class CustomDataSampleTests
    {
        const string SceneName = "Custom Data With ISavable";

        [UnityTest]
        public IEnumerator FoundItem_IsStillThereAfterSavingAndReloading()
        {
            yield return SampleScenes.LoadAndWait(SceneName);
            string item = "Test Gem " + Guid.NewGuid().ToString("N").Substring(0, 6);
            Object.FindAnyObjectByType<InventorySampleScreen>().Inventory.Add(item);
            SaveActions.SaveAll();

            yield return SampleScenes.LoadAndWait(SceneName);

            Inventory inventory = Object.FindAnyObjectByType<InventorySampleScreen>().Inventory;
            CollectionAssert.Contains(inventory.Items, item);
        }
    }
}
