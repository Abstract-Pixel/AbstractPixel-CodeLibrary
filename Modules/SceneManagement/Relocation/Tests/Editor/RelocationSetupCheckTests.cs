using System.Collections.Generic;
using AbstractPixel.Core;
using AbstractPixel.SceneManagement.Editor;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Relocation.Tests
{
    /// <summary>
    /// Session 4D stage 3: the setup mistakes the Scene view and the group check show before Play, worked out by the
    /// editor model with the same matching as Play: too few positions, a duplicate instance, a set instance with no
    /// position, a too-steep position, overlapping positions, and missing ground under one that waits for it.
    /// </summary>
    public class RelocationSetupCheckTests
    {
        private RelocationTestWorld world;
        private RelocationLabel crateLabel;

        [SetUp]
        public void CreateWorldWithGround()
        {
            world = new RelocationTestWorld();
            crateLabel = world.Label("Crate");
            world.Ground(0f);
        }

        [TearDown]
        public void DisposeWorld()
        {
            world.Dispose();
            RelocationEditorModel.MarkDirty();
        }

        [Test]
        public void TooFewPositionsNamesTheLabelAndTheCounts()
        {
            world.SpawnPoint("Crates", crateLabel, Vector3.zero);
            world.Crate("Crate A", crateLabel);
            world.Crate("Crate B", crateLabel);

            StringAssert.Contains("Label 'Crate': 2 relocatables, 1 positions", AllProblems());
        }

        [Test]
        public void MoreNoLabelRelocatablesThanPositionsIsAProblem()
        {
            world.Crate("Player", null);

            StringAssert.Contains("No label: 1 relocatables, 0 positions", AllProblems());
        }

        [Test]
        public void ADuplicateInstanceIsFlagged()
        {
            world.SpawnPoint("First", crateLabel, new Vector3(0f, 0f, 0f));
            world.SpawnPoint("Second", crateLabel, new Vector3(5f, 0f, 0f));

            StringAssert.Contains("'Second', Instance 1: the instance is used twice", AllProblems());
        }

        [Test]
        public void ASetInstanceWithNoPositionIsAProblem()
        {
            world.SpawnPoint("Crates", crateLabel, Vector3.zero);
            world.Crate("Wants 9", crateLabel).Instance = 9;

            StringAssert.Contains("'Wants 9' asks for Instance 9, but no Label 'Crate' position has it", AllProblems());
        }

        [Test]
        public void ATooSteepPositionNamesItsAngleAndTheLimit()
        {
            GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.transform.position = new Vector3(50f, 5f, 0f);
            ramp.transform.rotation = Quaternion.Euler(0f, 0f, 40f);
            ramp.transform.localScale = new Vector3(10f, 1f, 10f);
            Physics.SyncTransforms();
            Vector3 onRamp = new Vector3(50f, 5f, 0f) + ramp.transform.up * 0.55f;
            world.SpawnPoint("On the ramp", crateLabel, onRamp);

            StringAssert.Contains("'On the ramp', Instance 1: 40°, steepest allowed for Crate is 35°", AllProblems());
        }

        [Test]
        public void AnUprightPositionAddedOnASlopeClearsTheGround()
        {
            GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.transform.position = new Vector3(50f, 5f, 0f);
            ramp.transform.rotation = Quaternion.Euler(0f, 0f, 25f);
            ramp.transform.localScale = new Vector3(20f, 1f, 20f);
            Physics.SyncTransforms();
            Ray down = new Ray(new Vector3(50f, 30f, 0f), Vector3.down);
            Assert.IsTrue(EditorGround.TryHit(down, out RaycastHit hit));

            Pose pose = GroundPlacement.StandingPose(hit, Vector3.forward, PreviewShapes.Box(), false);

            Assert.LessOrEqual(GroundPlacement.SinkDepth(PreviewShapes.Box(), pose), 0.001f, "no corner starts inside the slope");
            Assert.Less(Vector3.Angle(Vector3.up, pose.rotation * Vector3.up), 0.1f, "upright");
        }

        [Test]
        public void APositionSinkingIntoASlopeIsFlagged()
        {
            GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.transform.position = new Vector3(50f, 5f, 0f);
            ramp.transform.rotation = Quaternion.Euler(0f, 0f, 34f);
            ramp.transform.localScale = new Vector3(20f, 1f, 20f);
            Physics.SyncTransforms();
            Vector3 onSurface = new Vector3(50f, 5f, 0f) + ramp.transform.up * 0.5f;
            world.SpawnPoint("Half buried", crateLabel, onSurface + Vector3.up * 0.05f);

            StringAssert.Contains("'Half buried', Instance 1: one side sinks", AllProblems());
        }

        [Test]
        public void OverlappingPositionsAreFlagged()
        {
            SpawnPoint spawnPoint = world.SpawnPoint("Crates", crateLabel, Vector3.zero);
            spawnPoint.AddPosition(new Pose(new Vector3(0.3f, 0f, 0f), Quaternion.identity), 2);

            StringAssert.Contains("overlaps another position", AllProblems());
        }

        [Test]
        public void MissingGroundUnderARelocatableThatWaitsIsAProblem()
        {
            world.SpawnPoint("Over the edge", crateLabel, new Vector3(1000f, 0f, 0f));
            world.Crate("Crate", crateLabel).WaitForGroundBelow = true;

            StringAssert.Contains("no ground below on the Ground layers, so 'Crate' would stay held", AllProblems());
        }

        [Test]
        public void ACleanSetupHasNoProblems()
        {
            world.SpawnPoint("Crates", crateLabel, Vector3.zero);
            world.Crate("Crate", crateLabel);

            Assert.AreEqual(string.Empty, AllProblems());
        }

        [Test]
        public void NextFreeInstanceIsOneMoreThanTheHighest()
        {
            SpawnPoint spawnPoint = world.SpawnPoint("Crates", crateLabel, Vector3.zero);
            spawnPoint.AddPosition(new Pose(new Vector3(4f, 0f, 0f), Quaternion.identity), 7);
            RelocationEditorModel.MarkDirty();

            Assert.AreEqual(8, RelocationEditorModel.Current.NextFreeInstance(crateLabel));
        }

        [Test]
        public void AddSpawnPointGivesTheNextFreeInstanceWithoutCountingItself()
        {
            Relocatable crate = world.Crate("Crate", crateLabel);
            SpawnPoint first = SpawnPointCreation.AddFor(crate);
            SpawnPoint second = SpawnPointCreation.AddFor(crate);

            Assert.AreEqual(1, first.GetInstance(0), "the first spawn point starts at Instance 1");
            Assert.AreEqual(2, second.GetInstance(0), "the next one continues");
        }

        [TestCase("Crate (12)", "Crate")]
        [TestCase("Crate 012", "Crate")]
        [TestCase("Lava drone_3", "Lava drone")]
        [TestCase("Spawn point (Crate)", "Crate")]
        [TestCase("Player", "Player")]
        public void NewLabelIsNamedAfterTheObject(string _objectName, string _labelName)
        {
            Relocatable relocatable = world.Crate(_objectName, null);

            Assert.AreEqual(_labelName, RelocationLabelField.NameFromObject(relocatable));
        }

        [Test]
        public void TheGroupCheckListsRelocationProblemsAsWarningsForTheOpenGroup()
        {
            world.Crate("Player", null);
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            string activePath = world.MainScene.path;
            group.Initialize(new SceneReference[0], new List<SceneReference>(), new SceneReference(world.MainScene.name, string.Empty, activePath), false);
            RelocationEditorModel.MarkDirty();

            List<SceneGroupProblem> problems = SceneSystemCheckHook.Find(group);
            Object.DestroyImmediate(group);

            Assert.AreEqual(1, problems.Count);
            Assert.IsFalse(problems[0].BlocksBuild, "relocation problems never stop a build");
            Assert.AreEqual(SceneGroupProblemFix.Select, problems[0].Fix);
        }

        private static string AllProblems()
        {
            RelocationEditorModel.MarkDirty();
            List<string> messages = new List<string>();
            foreach (RelocationEditorModel.Problem problem in RelocationEditorModel.Current.Problems)
            {
                messages.Add(problem.Message);
            }
            return string.Join(" | ", messages);
        }
    }
}
