using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Relocation.Tests
{
    /// <summary>
    /// Session 4D failure scenarios 11 to 14 (ADR 0006, ADR 0013): positions fill in Hierarchy order, a set instance
    /// wins, "No label" only matches "No label", only the main scene's spawn points count, positions follow scaled and
    /// mirrored parents, and too few positions hold the rest with errors naming each one.
    /// </summary>
    public class RelocationMatchingTests
    {
        private RelocationTestWorld world;
        private RelocationLabel crateLabel;

        [SetUp]
        public void CreateWorld()
        {
            world = new RelocationTestWorld();
            crateLabel = world.Label("Crate");
        }

        [TearDown]
        public void DisposeWorld()
        {
            LogAssert.ignoreFailingMessages = false;
            world.Dispose();
        }

        [Test]
        public async Task PositionsFillInHierarchyOrderThenPositionOrder()
        {
            SpawnPoint first = world.SpawnPoint("Near the bridge", crateLabel, new Vector3(0f, 0f, 0f));
            first.SetInstance(0, 1);
            first.AddPosition(new Pose(new Vector3(1f, 0f, 0f), Quaternion.identity), 2);
            SpawnPoint second = world.SpawnPoint("Near the barn", crateLabel, new Vector3(50f, 0f, 0f));
            second.SetInstance(0, 3);
            Relocatable a = world.AwakeCrate("A", crateLabel);
            Relocatable b = world.AwakeCrate("B", crateLabel);
            Relocatable c = world.AwakeCrate("C", crateLabel);

            await world.RunSceneChange();

            AssertAt(new Vector3(0f, 0f, 0f), a);
            AssertAt(new Vector3(1f, 0f, 0f), b);
            AssertAt(new Vector3(50f, 0f, 0f), c);
        }

        [Test]
        public async Task ASetInstanceWins()
        {
            SpawnPoint spawnPoint = world.SpawnPoint("Crates", crateLabel, new Vector3(0f, 0f, 0f));
            spawnPoint.AddPosition(new Pose(new Vector3(5f, 0f, 0f), Quaternion.identity), 2);
            Relocatable free = world.AwakeCrate("Free", crateLabel);
            Relocatable wantsOne = world.AwakeCrate("Wants instance 1", crateLabel);
            wantsOne.Instance = 1;

            await world.RunSceneChange();

            AssertAt(new Vector3(0f, 0f, 0f), wantsOne);
            AssertAt(new Vector3(5f, 0f, 0f), free);
        }

        [Test]
        public async Task ASetInstanceWithNoPositionStaysHeld()
        {
            world.SpawnPoint("Crates", crateLabel, Vector3.zero);
            Relocatable wantsSeven = world.AwakeCrate("Wants instance 7", crateLabel);
            wantsSeven.Instance = 7;
            LogAssert.Expect(LogType.Error, new Regex("'Wants instance 7' stays held: no free spawn point position with label 'Crate' has Instance 7.*scene group 'Test group'"));

            await world.RunSceneChange();

            Assert.IsTrue(wantsSeven.IsHeld);
        }

        [Test]
        public async Task DuplicateInstancesGoToTheFirstInHierarchyOrder()
        {
            world.SpawnPoint("First", crateLabel, new Vector3(1f, 0f, 0f));
            world.SpawnPoint("Second", crateLabel, new Vector3(2f, 0f, 0f));
            Relocatable wantsOne = world.AwakeCrate("Wants instance 1", crateLabel);
            wantsOne.Instance = 1;
            Relocatable free = world.AwakeCrate("Free", crateLabel);

            await world.RunSceneChange();

            AssertAt(new Vector3(1f, 0f, 0f), wantsOne);
            AssertAt(new Vector3(2f, 0f, 0f), free);
        }

        [Test]
        public async Task NoLabelNeverTakesALabelledPosition()
        {
            world.SpawnPoint("Crates", crateLabel, Vector3.zero);
            Relocatable unlabelled = world.AwakeCrate("Player", null);
            LogAssert.Expect(LogType.Error, new Regex("'Player' stays held: label 'No label' has no free spawn point position"));

            await world.RunSceneChange();

            Assert.IsTrue(unlabelled.IsHeld);
        }

        [Test]
        public async Task OnlyTheMainScenesSpawnPointsCount()
        {
            SpawnPoint inSharedScene = world.SpawnPoint("In the shared scene", null, Vector3.zero);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(inSharedScene.gameObject, world.CreateOtherScene());
            Relocatable player = world.AwakeCrate("Player", null);
            LogAssert.Expect(LogType.Error, new Regex("'Player' stays held: label 'No label' has no free spawn point position left in the main scene"));

            await world.RunSceneChange();

            Assert.IsTrue(player.IsHeld);
        }

        [Test]
        public async Task PositionsUnderAScaledOrMirroredParentLandRight()
        {
            GameObject parent = new GameObject("Scaled and turned parent");
            parent.transform.localScale = new Vector3(-2f, 2f, 2f);
            parent.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            SpawnPoint spawnPoint = world.SpawnPoint("Crates", crateLabel, new Vector3(3f, 0f, 0f));
            spawnPoint.transform.SetParent(parent.transform, true);
            Pose wanted = new Pose(new Vector3(10f, 1f, -4f), Quaternion.Euler(0f, 30f, 0f));
            spawnPoint.AddPosition(wanted, 2);
            Relocatable first = world.AwakeCrate("First", crateLabel);
            Relocatable second = world.AwakeCrate("Second", crateLabel);

            await world.RunSceneChange();

            AssertAt(wanted.position, second);
            float upAngle = Vector3.Angle(Vector3.up, second.transform.up);
            Assert.Less(upAngle, 0.1f, "never mirrored or tipped over");
            Assert.AreEqual(Vector3.one, second.transform.localScale, "never scaled");
            Assert.IsFalse(first.IsHeld);
        }

        [Test]
        public async Task HundredRelocatablesAndNinetyPositionsHoldTen()
        {
            SpawnPoint spawnPoint = world.SpawnPoint("Crates near the bridge", crateLabel, Vector3.zero);
            for (int position = 1; position < 90; position++)
            {
                spawnPoint.AddPosition(new Pose(new Vector3(position * 2f, 0f, 0f), Quaternion.identity), position + 1);
            }
            List<Relocatable> crates = new List<Relocatable>();
            for (int index = 0; index < 100; index++)
            {
                crates.Add(world.AwakeCrate("Crate " + index.ToString("000"), crateLabel));
            }
            for (int index = 90; index < 100; index++)
            {
                LogAssert.Expect(LogType.Error, new Regex("'Crate " + index.ToString("000") + "' stays held: label 'Crate' has no free spawn point position left.*Label 'Crate', scene group 'Test group'"));
            }

            await world.RunSceneChange();

            int held = crates.FindAll(_crate => _crate.IsHeld).Count;
            Assert.AreEqual(10, held);
            Assert.IsTrue(crates[99].IsHeld, "the last ones in Hierarchy order are the ones left");
            AssertAt(new Vector3(89 * 2f, 0f, 0f), crates[89]);
        }

        private static void AssertAt(Vector3 _expected, Relocatable _relocatable)
        {
            float distance = Vector3.Distance(_expected, _relocatable.transform.position);
            Assert.Less(distance, 0.001f, _relocatable.name + " is at " + _relocatable.transform.position);
            Assert.IsFalse(_relocatable.IsHeld, _relocatable.name + " is released");
        }
    }
}
