using System;
using System.Collections;
using AbstractPixel.SaveSystem;
using NUnit.Framework;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace SaveSystemSamples.SaveAField.Tests
{
    public class SaveAFieldSampleTests
    {
        const string SceneName = "Save A Field";

        [UnityTest]
        public IEnumerator HealthBar_ReadsTheSavedHealthInStart_AfterAReload()
        {
            yield return SampleScene.Load(SceneName);
            Health health = Object.FindAnyObjectByType<Health>();
            health.Heal(Health.Max);
            int hits = Random.Range(1, 10);
            for (int i = 0; i < hits; i++)
            {
                health.TakeDamage(10);
            }
            int expected = Health.Max - hits * 10;
            SaveActions.SaveAll();

            yield return SampleScene.Load(SceneName);

            Assert.AreEqual(expected, Object.FindAnyObjectByType<HealthBar>().ValueReadInStart);
        }

        [UnityTest]
        public IEnumerator Backpack_KeepsAFoundItem_AfterSavingAndReloading()
        {
            yield return SampleScene.Load(SceneName);
            string item = "Test Gem " + Guid.NewGuid().ToString("N").Substring(0, 6);
            Object.FindAnyObjectByType<SaveAFieldScreen>().Backpack.Add(item);
            SaveActions.SaveAll();

            yield return SampleScene.Load(SceneName);

            Backpack backpack = Object.FindAnyObjectByType<SaveAFieldScreen>().Backpack;
            CollectionAssert.Contains(backpack.Items, item);

            // Leave the sample's save data as it was.
            backpack.RemoveLast();
            SaveActions.SaveAll();
        }
    }
}
