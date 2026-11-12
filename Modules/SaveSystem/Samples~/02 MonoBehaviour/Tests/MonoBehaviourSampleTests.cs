using System.Collections;
using AbstractPixel.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SaveSystemSamples.MonoBehaviourSample.Tests
{
    public class MonoBehaviourSampleTests
    {
        const string SceneName = "MonoBehaviour";

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
            SaveSystem.SaveAll();

            yield return SampleScene.Load(SceneName);

            Assert.AreEqual(expected, Object.FindAnyObjectByType<HealthBar>().ValueReadInStart);
        }
    }
}
