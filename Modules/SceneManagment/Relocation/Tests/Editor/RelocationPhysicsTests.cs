using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Relocation.Tests
{
    /// <summary>
    /// Session 4D failure scenarios 1 to 8 and 19 (session-4d-failure-scenarios.md): how Hold, Relocate and Release
    /// treat physics objects. Hold makes bodies kinematic with interpolation off, Relocate writes body and transform and
    /// checks a frame later, Release gives every component back its own state.
    /// </summary>
    public class RelocationPhysicsTests
    {
        private static readonly Vector3 SpawnPosition = new Vector3(100f, 0f, 50f);
        private static readonly Quaternion SpawnRotation = Quaternion.Euler(0f, 90f, 0f);

        private RelocationTestWorld world;
        private SpawnPoint spawnPoint;

        [SetUp]
        public void CreateWorldWithOneSpawnPoint()
        {
            world = new RelocationTestWorld();
            spawnPoint = world.SpawnPoint("Spawn", null, SpawnPosition);
            spawnPoint.transform.rotation = SpawnRotation;
        }

        [TearDown]
        public void DisposeWorld()
        {
            Time.timeScale = 1f;
            Physics.simulationMode = SimulationMode.FixedUpdate;
            world.Dispose();
        }

        [Test]
        public async Task JointedBodiesKeepTheirOffsets()
        {
            Relocatable car = world.AwakeCrate("Car", null);
            Rigidbody carBody = car.GetComponent<Rigidbody>();
            GameObject link = new GameObject("Chain link");
            link.transform.SetParent(car.transform, false);
            link.transform.localPosition = new Vector3(0f, 2f, 0f);
            Rigidbody linkBody = link.AddComponent<Rigidbody>();
            link.AddComponent<HingeJoint>().connectedBody = carBody;
            GameObject trailer = new GameObject("Trailer");
            trailer.transform.position = new Vector3(0f, 0f, -4f);
            Rigidbody trailerBody = trailer.AddComponent<Rigidbody>();
            trailer.AddComponent<ConfigurableJoint>().connectedBody = carBody;

            await world.RunSceneChange();

            Vector3 expectedTrailer = SpawnPosition + SpawnRotation * new Vector3(0f, 0f, -4f);
            Assert.That(Vector3.Distance(expectedTrailer, trailer.transform.position), Is.LessThan(0.001f), "the trailer moved with the car");
            Assert.That(Vector3.Distance(expectedTrailer, trailerBody.position), Is.LessThan(0.001f), "the trailer's body too");
            Assert.That(Vector3.Distance(SpawnPosition + new Vector3(0f, 2f, 0f), linkBody.position), Is.LessThan(0.001f), "the chain link kept its offset");
            Assert.IsFalse(trailerBody.isKinematic, "the trailer is released too");
        }

        [Test]
        public async Task AJointToAnotherRelocatableWarnsWithBothNames()
        {
            Relocatable car = world.AwakeCrate("Car", null);
            world.SpawnPoint("Trailer spawn", null, new Vector3(-50f, 0f, 0f));
            Relocatable trailer = world.AwakeCrate("Trailer", null);
            trailer.gameObject.AddComponent<FixedJoint>().connectedBody = car.GetComponent<Rigidbody>();

            LogAssert.Expect(LogType.Warning, new Regex("'Car' is joined to 'Trailer'"));
            await world.RunSceneChange();
        }

        [Test]
        public async Task ValuesSetWhileHeldSurviveRelease()
        {
            Relocatable car = world.AwakeCrate("Car", null);
            Rigidbody body = car.GetComponent<Rigidbody>();
            world.Broadcaster.Hold();
            body.mass = 1260f;
            body.centerOfMass = new Vector3(0f, -0.4f, 0.2f);
            body.constraints = RigidbodyConstraints.FreezeRotationZ;

            await world.Broadcaster.Relocate();
            world.Broadcaster.Release();

            Assert.AreEqual(1260f, body.mass);
            Assert.That(Vector3.Distance(new Vector3(0f, -0.4f, 0.2f), body.centerOfMass), Is.LessThan(0.0001f));
            Assert.AreEqual(RigidbodyConstraints.FreezeRotationZ, body.constraints);
            Assert.IsFalse(body.isKinematic);
        }

        [Test]
        public async Task IRelocationHoldComponentsGetHoldAndRelease()
        {
            Relocatable car = world.AwakeCrate("Car", null);
            GameObject engine = new GameObject("Engine");
            engine.transform.SetParent(car.transform, false);
            CountingHold counter = engine.AddComponent<CountingHold>();

            world.Broadcaster.Hold();
            await world.Broadcaster.Relocate();
            int holdsBeforeRelease = counter.Holds;
            world.Broadcaster.Release();

            Assert.AreEqual(1, holdsBeforeRelease, "OnHold once, while held");
            Assert.AreEqual(1, counter.Releases, "OnRelease once");
        }

        [Test]
        public async Task InterpolationIsOffWhileHeldAndRestoredOnRelease()
        {
            Relocatable car = world.AwakeCrate("Car", null);
            Rigidbody body = car.GetComponent<Rigidbody>();

            world.Broadcaster.Hold();
            RigidbodyInterpolation whileHeld = body.interpolation;
            bool kinematicWhileHeld = body.isKinematic;
            await world.Broadcaster.Relocate();
            world.Broadcaster.Release();

            Assert.AreEqual(RigidbodyInterpolation.None, whileHeld);
            Assert.IsTrue(kinematicWhileHeld);
            Assert.AreEqual(RigidbodyInterpolation.Interpolate, body.interpolation);
        }

        [Test]
        public async Task RelocateWritesTheBodyAndTheTransform()
        {
            Relocatable car = world.AwakeCrate("Car", null);
            Rigidbody body = car.GetComponent<Rigidbody>();

            world.Broadcaster.Hold();
            await world.Broadcaster.Relocate();

            Assert.That(Vector3.Distance(SpawnPosition, car.transform.position), Is.LessThan(0.001f), "transform");
            Assert.That(Vector3.Distance(SpawnPosition, body.position), Is.LessThan(0.001f), "Rigidbody.position");
            Assert.That(Quaternion.Angle(SpawnRotation, body.rotation), Is.LessThan(0.1f), "Rigidbody.rotation");
            Assert.IsTrue(car.IsHeld, "still held until Release");
            world.Broadcaster.Release();
            Assert.IsFalse(car.IsHeld);
        }

        [Test]
        public async Task TrailsAndWorldSpaceParticlesStartFreshAtTheSpawnPoint()
        {
            Relocatable car = world.AwakeCrate("Car", null);
            GameObject exhaust = new GameObject("Exhaust");
            exhaust.transform.SetParent(car.transform, false);
            ParticleSystem smoke = exhaust.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = smoke.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            smoke.Emit(20);
            GameObject tyre = new GameObject("Tyre mark");
            tyre.transform.SetParent(car.transform, false);
            TrailRenderer trail = tyre.AddComponent<TrailRenderer>();
            trail.AddPosition(new Vector3(-5f, 0f, 0f));
            trail.AddPosition(new Vector3(-4f, 0f, 0f));

            await world.RunSceneChange();

            Assert.AreEqual(0, smoke.particleCount, "no smoke left at the old place");
            Assert.AreEqual(0, trail.positionCount, "no streak across the jump");
        }

        [Test]
        public async Task ReleaseRestoresEachComponentsOwnState()
        {
            Relocatable car = world.Crate("Car", null);
            Rigidbody body = car.GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Extrapolate;
            GameObject walker = new GameObject("Walker");
            walker.transform.SetParent(car.transform, false);
            CharacterController offController = walker.AddComponent<CharacterController>();
            offController.enabled = false;
            GameObject runner = new GameObject("Runner");
            runner.transform.SetParent(car.transform, false);
            CharacterController onController = runner.AddComponent<CharacterController>();
            world.Wake(car);

            world.Broadcaster.Hold();
            bool onControllerHeldOff = !onController.enabled;
            await world.Broadcaster.Relocate();
            world.Broadcaster.Release();

            Assert.IsTrue(onControllerHeldOff, "a CharacterController is off while held");
            Assert.IsTrue(body.isKinematic, "a body that was kinematic stays kinematic");
            Assert.AreEqual(RigidbodyInterpolation.Extrapolate, body.interpolation);
            Assert.IsFalse(offController.enabled, "a CharacterController that was off stays off");
            Assert.IsTrue(onController.enabled, "a CharacterController that was on is on again");
        }

        [Test]
        public async Task NestedRelocatablesHoldEachBodyOnce()
        {
            RelocationLabel riderLabel = world.Label("Rider");
            SpawnPoint riderSpawn = world.SpawnPoint("Rider spawn", riderLabel, new Vector3(-20f, 0f, 0f));
            Relocatable car = world.Crate("Car", null);
            Relocatable rider = world.Crate("Rider", riderLabel);
            rider.transform.SetParent(car.transform, false);
            world.Wake(car);
            world.Wake(rider);

            await world.RunSceneChange();

            Assert.IsFalse(rider.GetComponent<Rigidbody>().isKinematic, "the inner body is dynamic again, not stuck kinematic");
            Assert.IsFalse(car.GetComponent<Rigidbody>().isKinematic);
            Assert.That(Vector3.Distance(riderSpawn.transform.position, rider.transform.position), Is.LessThan(0.001f), "the inner relocatable ends at its own position");
            Assert.That(Vector3.Distance(SpawnPosition, car.transform.position), Is.LessThan(0.001f));
        }

        [Test]
        public async Task ABodyAddedDuringTheHoldIsHeldBeforeRelocate()
        {
            Relocatable car = world.AwakeCrate("Car", null);
            world.Broadcaster.Hold();
            GameObject wheel = new GameObject("Wheel added late");
            wheel.transform.SetParent(car.transform, false);
            Rigidbody wheelBody = wheel.AddComponent<Rigidbody>();

            await world.Broadcaster.Relocate();
            bool heldBeforeRelease = wheelBody.isKinematic;
            world.Broadcaster.Release();

            Assert.IsTrue(heldBeforeRelease, "held before it could fall");
            Assert.IsFalse(wheelBody.isKinematic, "and given back its own state");
        }

        [Test]
        public async Task AOneOffMoveIsCorrectedByTheSecondWrite()
        {
            Relocatable car = world.AwakeCrate("Car", null);
            world.Broadcaster.Hold();

            Task relocate = world.Broadcaster.Relocate();
            car.transform.position = new Vector3(5f, 5f, 5f);
            await relocate;
            world.Broadcaster.Release();

            Assert.That(Vector3.Distance(SpawnPosition, car.transform.position), Is.LessThan(0.001f));
            Assert.IsFalse(car.IsHeld);
        }

        [Test]
        public async Task ABodyMovedEveryFrameStaysHeldWithAnError()
        {
            Relocatable car = world.AwakeCrate("Car", null);
            world.Broadcaster.Hold();
            LogAssert.Expect(LogType.Error, new Regex("'Car' stays held: something moved it away.*scene group 'Test group'"));

            Task relocate = world.Broadcaster.Relocate();
            while (!relocate.IsCompleted)
            {
                car.transform.position = new Vector3(5f, 5f, 5f);
                await Task.Yield();
            }
            await relocate;
            world.Broadcaster.Release();

            Assert.IsTrue(car.IsHeld, "a held object with a clear error beats one released in the wrong place");
        }

        [Test]
        public async Task RelocateFinishesWithGameTimeStoppedAndScriptSimulation()
        {
            Time.timeScale = 0f;
            Physics.simulationMode = SimulationMode.Script;
            world.Ground(0f);
            Relocatable car = world.AwakeCrate("Car", null);
            car.WaitForGroundBelow = true;

            await world.RunSceneChange();

            Assert.IsFalse(car.IsHeld);
        }

        [Test]
        public async Task ACharacterControllerAndABodyOnOneObjectAreBothHeld()
        {
            Relocatable player = world.Crate("Player", null);
            CharacterController controller = player.gameObject.AddComponent<CharacterController>();
            world.Wake(player);

            world.Broadcaster.Hold();
            bool bothHeld = !controller.enabled && player.GetComponent<Rigidbody>().isKinematic;
            await world.Broadcaster.Relocate();
            world.Broadcaster.Release();

            Assert.IsTrue(bothHeld);
            Assert.IsTrue(controller.enabled);
            Assert.IsFalse(player.GetComponent<Rigidbody>().isKinematic);
        }

        [Test]
        public async Task ANavMeshAgentOffTheNavMeshStaysHeld()
        {
            Relocatable drone = world.Crate("Drone", null);
            GameObject agentObject = new GameObject("Agent");
            agentObject.transform.SetParent(drone.transform, false);
            agentObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
            world.Wake(drone);
            LogAssert.ignoreFailingMessages = true;

            await world.RunSceneChange();

            LogAssert.ignoreFailingMessages = false;
            Assert.IsTrue(drone.IsHeld, "Warp failed, so it stays held");
        }
    }
}
