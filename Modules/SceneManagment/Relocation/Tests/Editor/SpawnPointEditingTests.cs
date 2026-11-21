using NUnit.Framework;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using AbstractPixel.SceneManagement.Editor;

namespace AbstractPixel.SceneManagement.Relocation.Tests
{
    /// <summary>
    /// Bugs the user found by hand (4D): holding Shift+D duplicated once per key repeat, and Ctrl+Z after that left the
    /// position selection pointing past the end, so the next draw threw IndexOutOfRange.
    /// </summary>
    public class SpawnPointEditingTests
    {
        private RelocationTestWorld world;
        private SpawnPoint spawnPoint;

        [SetUp]
        public void CreateSpawnPointWithThreePositions()
        {
            world = new RelocationTestWorld();
            spawnPoint = world.SpawnPoint("Crates", null, Vector3.zero);
            spawnPoint.AddPosition(new Pose(new Vector3(2f, 0f, 0f), Quaternion.identity), 2);
            spawnPoint.AddPosition(new Pose(new Vector3(4f, 0f, 0f), Quaternion.identity), 3);
            Selection.activeGameObject = spawnPoint.gameObject;
            SpawnPointPositionSelection.Select(spawnPoint, 2, false);
        }

        [TearDown]
        public void DisposeWorld()
        {
            SpawnPointPositionSelection.Clear();
            Selection.activeGameObject = null;
            world.Dispose();
        }

        [Test]
        public void HoldingTheDuplicateKeyDuplicatesOnce()
        {
            SpawnPointShortcuts.Duplicate(new ShortcutArguments { stage = ShortcutStage.Begin });
            // Key repeats while the key is held arrive as nothing new for a clutch shortcut; End comes when it is let go.
            SpawnPointShortcuts.Duplicate(new ShortcutArguments { stage = ShortcutStage.End });

            Assert.AreEqual(4, spawnPoint.PositionCount);
        }

        [Test]
        public void UndoAfterManyDuplicatesLeavesAValidSelection()
        {
            Undo.IncrementCurrentGroup();
            for (int press = 0; press < 6; press++)
            {
                SpawnPointEditing.DuplicateSelected();
                Undo.IncrementCurrentGroup();
            }
            Assert.AreEqual(9, spawnPoint.PositionCount);

            for (int undo = 0; undo < 6; undo++)
            {
                Undo.PerformUndo();
            }

            Assert.AreEqual(3, spawnPoint.PositionCount, "every duplicate was undone");
            foreach (int position in SpawnPointPositionSelection.Positions)
            {
                Assert.Less(position, spawnPoint.PositionCount, "the selection only names positions that exist");
            }
            Assert.DoesNotThrow(() => SpawnPointEditing.MoveSelected(Vector3.up, Quaternion.identity));
            Assert.DoesNotThrow(SpawnPointEditing.FrameSelected);
            Assert.DoesNotThrow(SpawnPointEditing.RemoveSelected);
        }
    }
}
