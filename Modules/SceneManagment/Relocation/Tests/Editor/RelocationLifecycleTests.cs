using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Relocation.Tests
{
    /// <summary>
    /// Session 4D failure scenarios 16 to 18 and 20: Released runs once per change after physics is restored, a
    /// throwing listener does not stop the others, late relocatables are relocated once per change, 1000 relocatables
    /// fit in one change, nothing runs every frame, and statics reset.
    /// </summary>
    public class RelocationLifecycleTests
    {
        private RelocationTestWorld world;

        [SetUp]
        public void CreateWorld()
        {
            world = new RelocationTestWorld();
            world.SpawnPoint("Spawn", null, new Vector3(10f, 0f, 0f));
        }

        [TearDown]
        public void DisposeWorld()
        {
            world.Dispose();
        }

        [Test]
        public async Task ReleasedRunsOncePerChangeAfterPhysicsIsRestored()
        {
            Relocatable car = world.AwakeCrate("Car", null);
            Rigidbody body = car.GetComponent<Rigidbody>();
            List<string> seen = new List<string>();
            car.Released += () => seen.Add("released, kinematic " + body.isKinematic + ", held " + car.IsHeld);
            car.Relocated += () => seen.Add("relocated, held " + car.IsHeld);

            await world.RunSceneChange();
            await world.RunSceneChange();

            string expected = "relocated, held True, released, kinematic False, held False, relocated, held True, released, kinematic False, held False";
            Assert.AreEqual(expected, string.Join(", ", seen));
        }

        [Test]
        public async Task AThrowingListenerDoesNotStopTheOthers()
        {
            Relocatable car = world.AwakeCrate("Car", null);
            int otherListenerRuns = 0;
            car.Released += () => throw new InvalidOperationException("test listener failure");
            car.Released += () => otherListenerRuns++;
            LogAssert.Expect(LogType.Exception, new Regex("test listener failure"));

            await world.RunSceneChange();

            Assert.AreEqual(1, otherListenerRuns);
            Assert.IsFalse(car.IsHeld);
        }

        [Test]
        public async Task ARelocatableThatWakesDuringTheChangeIsHeldAndRelocated()
        {
            world.Broadcaster.Hold();
            Relocatable late = world.Crate("Woke during the change", null);
            world.Wake(late);
            bool heldAtOnce = late.IsHeld;

            await world.Broadcaster.Relocate();
            world.Broadcaster.Release();

            Assert.IsTrue(heldAtOnce);
            Assert.IsFalse(late.IsHeld);
            Assert.Less(Vector3.Distance(new Vector3(10f, 0f, 0f), late.transform.position), 0.001f);
        }

        [Test]
        public async Task ALateRelocatableIsRelocatedOncePerChange()
        {
            await world.RunSceneChange();
            Relocatable pooled = world.Crate("Pooled enemy", null);

            world.Wake(pooled);
            await RelocationTestWorld.Frames(6);
            Vector3 afterFirstWake = pooled.transform.position;
            pooled.transform.position = new Vector3(-30f, 0f, 0f);
            world.Broadcaster.OnRelocatableDisabled(pooled);
            world.Wake(pooled);
            await RelocationTestWorld.Frames(6);

            Assert.Less(Vector3.Distance(new Vector3(10f, 0f, 0f), afterFirstWake), 0.001f, "relocated at once");
            Assert.Less(Vector3.Distance(new Vector3(-30f, 0f, 0f), pooled.transform.position), 0.001f, "a second wake in the same visit does nothing");
            Assert.IsFalse(pooled.IsHeld);
        }

        [Test]
        public async Task BeforeStartUpARelocatableIsHeldUntilRelocate()
        {
            Relocatable car = world.AwakeCrate("Car in the first scene", null);
            Assert.IsTrue(car.IsHeld, "held at once, waiting for start-up");

            await world.Broadcaster.Relocate();
            world.Broadcaster.Release();

            Assert.IsFalse(car.IsHeld);
        }

        [Test]
        public async Task ThousandRelocatablesInOneChange()
        {
            RelocationLabel label = world.Label("Crate");
            SpawnPoint spawnPoint = world.SpawnPoint("Crates", label, Vector3.zero);
            for (int position = 1; position < 1000; position++)
            {
                Vector3 place = new Vector3(position % 40 * 2f, 0f, position / 40 * 2f);
                spawnPoint.AddPosition(new Pose(place, Quaternion.identity), position + 1);
            }
            List<Relocatable> crates = new List<Relocatable>();
            for (int index = 0; index < 1000; index++)
            {
                crates.Add(world.AwakeCrate("Crate " + index, label));
            }

            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            await world.RunSceneChange();

            Assert.AreEqual(0, crates.FindAll(_crate => _crate.IsHeld).Count);
            Assert.Less(watch.Elapsed.TotalSeconds, 5.0, "1000 relocatables took " + watch.Elapsed.TotalSeconds + " s");
        }

        [Test]
        public void NoRelocationComponentRunsEveryFrame()
        {
            string[] everyFrame = { "Update", "LateUpdate", "FixedUpdate", "OnGUI" };
            Type[] components = { typeof(Relocatable), typeof(SpawnPoint) };
            foreach (Type component in components)
            {
                foreach (string methodName in everyFrame)
                {
                    MethodInfo method = component.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    Assert.IsNull(method, component.Name + " has " + methodName);
                }
            }
        }

        [Test]
        public void ResetStaticsClearsTheBroadcaster()
        {
            RelocationBroadcaster.ResetStatics();

            Assert.IsNull(RelocationBroadcaster.Current);
        }
    }
}
