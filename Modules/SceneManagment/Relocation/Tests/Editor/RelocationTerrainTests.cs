using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AbstractPixel.SceneManagement.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Relocation.Tests
{
    /// <summary>
    /// Relocation and the spawn point tool on a real Terrain, not only on box colliders: a 100 m terrain whose west half
    /// is flat (0°) and whose east half rises 60 m over 50 m (about 50°). Wait for ground below finds the
    /// TerrainCollider, an object deep inside the terrain stays held, and the editor reads the terrain's slope for the
    /// Shift+click ray and the too-steep check.
    /// </summary>
    public class RelocationTerrainTests
    {
        private const float SteepRise = 60f;
        private RelocationTestWorld world;
        private TerrainData terrainData;
        private RelocationLabel crateLabel;

        [SetUp]
        public void CreateWorldWithATerrain()
        {
            world = new RelocationTestWorld();
            crateLabel = world.Label("Crate");
            terrainData = FlatThenSteepTerrain();
            GameObject terrain = Terrain.CreateTerrainGameObject(terrainData);
            terrain.transform.position = new Vector3(-50f, 0f, -50f);
            Physics.SyncTransforms();
        }

        [TearDown]
        public void DisposeWorld()
        {
            world.Dispose();
            Object.DestroyImmediate(terrainData);
            RelocationEditorModel.MarkDirty();
        }

        [Test]
        public async Task WaitForGroundBelowFindsTheTerrain()
        {
            world.SpawnPoint("On flat terrain", crateLabel, new Vector3(-20f, 0.05f, 0f));
            Relocatable crate = world.Crate("Crate", crateLabel);
            crate.WaitForGroundBelow = true;
            world.Wake(crate);

            await world.RunSceneChange();

            Assert.IsFalse(crate.IsHeld);
        }

        [Test]
        public async Task ACrateDeepInsideTheTerrainStaysHeld()
        {
            world.SpawnPoint("Sunk into the terrain", crateLabel, new Vector3(-20f, -0.6f, 0f));
            Relocatable crate = world.Crate("Crate", crateLabel);
            crate.WaitForGroundBelow = true;
            world.Wake(crate);
            LogAssert.Expect(LogType.Error, new Regex("'Crate' stays held: it starts 0\\.[0-9]+ m under the terrain 'Terrain'"));

            await world.RunSceneChange();

            Assert.IsTrue(crate.IsHeld);
        }

        [Test]
        public void TheShiftClickRayHitsTheTerrainWithItsSlope()
        {
            Ray fromAboveFlat = new Ray(new Vector3(-20f, 100f, 0f), Vector3.down);
            Ray fromAboveSteep = new Ray(new Vector3(25f, 200f, 0f), Vector3.down);

            bool hitFlat = EditorGround.TryHit(fromAboveFlat, out RaycastHit flat);
            bool hitSteep = EditorGround.TryHit(fromAboveSteep, out RaycastHit steep);

            Assert.IsTrue(hitFlat && hitSteep, "the ray finds the TerrainCollider");
            Assert.Less(EditorGround.SlopeOf(flat.normal), 1f, "flat half");
            Assert.That(EditorGround.SlopeOf(steep.normal), Is.InRange(45f, 55f), "steep half");
            Assert.That(steep.point.y, Is.InRange(27f, 33f), "the hit is on the terrain surface");
        }

        [Test]
        public void ASpawnPointOnTheSteepTerrainIsTooSteepAndOneOnTheFlatIsNot()
        {
            world.SpawnPoint("On flat terrain", crateLabel, new Vector3(-20f, 0.05f, 0f));
            float steepHeight = SteepRise * 25f / 50f;
            world.SpawnPoint("On steep terrain", crateLabel, new Vector3(25f, steepHeight + 0.05f, 0f));

            string problems = AllProblems();

            StringAssert.DoesNotContain("'On flat terrain'", problems);
            StringAssert.IsMatch("'On steep terrain', Instance 1: (4[5-9]|5[0-5])°, steepest allowed for Crate is 35°", problems);
        }

        [Test]
        public void ASpawnPointUnderTheTerrainIsAProblemInTheEditor()
        {
            world.SpawnPoint("Under the terrain", crateLabel, new Vector3(-20f, -2f, 0f));

            StringAssert.Contains("'Under the terrain', Instance 1: 2.00 m under the terrain", AllProblems());
        }

        // 129 x 129 heights over 100 m: x below the middle is 0, above it rises linearly to SteepRise at the east edge.
        private static TerrainData FlatThenSteepTerrain()
        {
            TerrainData data = new TerrainData();
            data.heightmapResolution = 129;
            data.size = new Vector3(100f, SteepRise, 100f);
            int resolution = data.heightmapResolution;
            float[,] heights = new float[resolution, resolution];
            int middle = (resolution - 1) / 2;
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float rise = 0f;
                    if (x > middle)
                    {
                        rise = (x - middle) / (float)middle;
                    }
                    heights[z, x] = rise;
                }
            }
            data.SetHeights(0, 0, heights);
            return data;
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
