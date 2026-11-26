using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Relocation.Tests
{
    /// <summary>
    /// Session 4D failure scenarios 9, 10, 15 and 18 (part): Wait for ground below only waits, frame by frame, for a
    /// non-trigger collider on the Ground layers; gives up after its time with a named error; holds an object that
    /// starts deep inside the ground; ends when its objects are gone; allocates nothing per look.
    /// </summary>
    public class RelocationGroundTests
    {
        private RelocationTestWorld world;
        private Relocatable crate;

        [SetUp]
        public void CreateWorldWithACrateThatWaitsForGround()
        {
            world = new RelocationTestWorld();
            world.SpawnPoint("Spawn", null, new Vector3(10f, 0f, 10f));
            crate = world.Crate("Crate", null);
            crate.WaitForGroundBelow = true;
            world.Wake(crate);
        }

        [TearDown]
        public void DisposeWorld()
        {
            world.Dispose();
        }

        [Test]
        public async Task GroundBelowIsFoundAndTheCrateIsReleased()
        {
            world.Ground(0f);

            await world.RunSceneChange();

            Assert.IsFalse(crate.IsHeld);
        }

        [Test]
        public async Task GroundThatAppearsLaterIsFound()
        {
            world.Broadcaster.GroundWaitSeconds = 3f;
            world.Broadcaster.Hold();
            Task relocate = world.Broadcaster.Relocate();
            await RelocationTestWorld.Frames(4);
            Assert.IsFalse(relocate.IsCompleted, "still waiting for ground");

            world.Ground(0f);
            await relocate;
            world.Broadcaster.Release();

            Assert.IsFalse(crate.IsHeld);
        }

        [Test]
        public async Task NoGroundTimesOutAndHoldsWithANamedError()
        {
            LogAssert.Expect(LogType.Error, new Regex("'Crate' stays held: no ground found below it.*Label 'No label', scene group 'Test group'"));

            Stopwatch watch = Stopwatch.StartNew();
            await world.RunSceneChange();

            Assert.IsTrue(crate.IsHeld);
            Assert.That(watch.Elapsed.TotalSeconds, Is.InRange(0.25, 2.0), "gave up after its time");
        }

        [Test]
        public async Task TriggerGroundDoesNotCount()
        {
            world.Ground(0f).isTrigger = true;
            Physics.SyncTransforms();
            LogAssert.Expect(LogType.Error, new Regex("'Crate' stays held: no ground found"));

            await world.RunSceneChange();

            Assert.IsTrue(crate.IsHeld);
        }

        [Test]
        public async Task GroundOutsideTheGroundLayersDoesNotCount()
        {
            BoxCollider ground = world.Ground(0f);
            ground.gameObject.layer = 2;
            Physics.SyncTransforms();
            LogAssert.Expect(LogType.Error, new Regex("'Crate' stays held: no ground found"));

            await world.RunSceneChange();

            Assert.IsTrue(crate.IsHeld);
        }

        [Test]
        public async Task ItsOwnCollidersNeverCountAsGround()
        {
            GameObject wheel = new GameObject("Wheel below the pivot");
            wheel.transform.SetParent(crate.transform, false);
            wheel.transform.localPosition = new Vector3(0f, -1f, 0f);
            wheel.AddComponent<BoxCollider>();
            LogAssert.Expect(LogType.Error, new Regex("'Crate' stays held: no ground found"));

            await world.RunSceneChange();

            Assert.IsTrue(crate.IsHeld);
        }

        [Test]
        public async Task AnObjectDeepInsideTheGroundStaysHeld()
        {
            crate.GetComponent<BoxCollider>().center = Vector3.zero;
            world.Ground(0.2f);
            LogAssert.Expect(LogType.Error, new Regex("'Crate' stays held: it starts 0\\.[0-9]+ m inside the ground"));

            await world.RunSceneChange();

            Assert.IsTrue(crate.IsHeld);
        }

        [Test]
        public async Task ACrateResting5CmIntoTheGroundIsReleased()
        {
            world.Ground(0.05f);

            await world.RunSceneChange();

            Assert.IsFalse(crate.IsHeld);
        }

        [Test]
        public async Task DestroyingEverythingEndsTheGroundWait()
        {
            world.Broadcaster.GroundWaitSeconds = 3f;
            world.Broadcaster.Hold();
            Stopwatch watch = Stopwatch.StartNew();
            Task relocate = world.Broadcaster.Relocate();
            await RelocationTestWorld.Frames(2);

            UnityEngine.Object.DestroyImmediate(crate.gameObject);
            await relocate;
            world.Broadcaster.Release();

            Assert.Less(watch.Elapsed.TotalSeconds, 1.0, "ended at the next frame, not after 3 s");
        }

        [Test]
        public async Task ARelocatableDisabledDuringTheGroundWaitIsSkipped()
        {
            Relocatable other = world.AwakeCrate("Other", null);
            world.SpawnPoint("Second spawn", null, new Vector3(-10f, 0f, 0f));
            world.Broadcaster.GroundWaitSeconds = 3f;
            world.Broadcaster.Hold();
            Task relocate = world.Broadcaster.Relocate();
            await RelocationTestWorld.Frames(2);

            crate.gameObject.SetActive(false);
            world.Broadcaster.OnRelocatableDisabled(crate);
            await relocate;
            world.Broadcaster.Release();

            Assert.IsFalse(other.IsHeld, "the rest are released");
            Assert.IsFalse(crate.GetComponent<Rigidbody>().isKinematic, "the disabled one got its own state back");
        }

        [Test]
        public void TheGroundLookAllocatesNothing()
        {
            GroundCheck check = new GroundCheck();
            crate.CurrentHold.HoldWhatIsActiveNow();
            Vector3 position = new Vector3(10f, 0f, 10f);
            check.TryFindGround(crate.CurrentHold, position, ~0, out Collider _);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int look = 0; look < 200; look++)
            {
                check.TryFindGround(crate.CurrentHold, position, ~0, out Collider _);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.AreEqual(0, allocated, "bytes allocated by 200 looks");
        }
    }
}
