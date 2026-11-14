using System.Collections;
using AbstractPixel.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SaveSystemSamples.SpawnedCopies.Tests
{
    public class SpawnedCopiesSampleTests
    {
        const string SceneName = "Spawned Copies";

        [UnityTest]
        public IEnumerator EachCopy_KeepsItsOwnHealth_AndADefeatedOneStaysDefeated()
        {
            yield return SampleScene.Load(SceneName);
            GoblinSpawner spawner = Object.FindAnyObjectByType<GoblinSpawner>();
            spawner.BringEveryGoblinBack();
            Assert.AreEqual(3, spawner.LivingGoblins.Count, "The sample scene has three spawn points.");

            spawner.LivingGoblins["Spawn Point 1"].Hit();
            spawner.LivingGoblins["Spawn Point 2"].Hit();
            spawner.LivingGoblins["Spawn Point 2"].Hit();
            spawner.Defeat("Spawn Point 3");
            SaveActions.SaveAll();

            yield return SampleScene.Load(SceneName);
            spawner = Object.FindAnyObjectByType<GoblinSpawner>();

            Assert.AreEqual(Goblin.MaxHealth - 1, spawner.LivingGoblins["Spawn Point 1"].Health);
            Assert.AreEqual(Goblin.MaxHealth - 2, spawner.LivingGoblins["Spawn Point 2"].Health, "Copies of one prefab never share saved data.");
            Assert.IsFalse(spawner.LivingGoblins.ContainsKey("Spawn Point 3"), "A defeated goblin isn't spawned again.");

            spawner.BringEveryGoblinBack();
            Assert.AreEqual(Goblin.MaxHealth, spawner.LivingGoblins["Spawn Point 3"].Health, "Forget removed the defeated copy's data, so it comes back fresh.");

            // Leave the sample's save data as a fresh game: defeating forgets each copy's data, then all come back new.
            spawner.Defeat("Spawn Point 1");
            spawner.Defeat("Spawn Point 2");
            spawner.Defeat("Spawn Point 3");
            yield return null;
            spawner.BringEveryGoblinBack();
            yield return null;
            SaveActions.SaveAll();
        }
    }
}
