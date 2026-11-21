using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Relocation.Tests
{
    /// <summary>
    /// The test runner's scene as the main scene, with a relocation broadcaster of its own, plus builders for labels,
    /// spawn points, relocatables and ground. Relocatables are "woken" by the test, since OnEnable does not run outside
    /// Play mode. Dispose it in the test's tear down: it destroys every root object the test added.
    /// </summary>
    internal sealed class RelocationTestWorld
    {
        public const string SceneGroupName = "Test group";

        private readonly List<Object> createdObjects = new List<Object>();
        private readonly List<Scene> previewScenes = new List<Scene>();
        private readonly HashSet<GameObject> rootsBeforeTheTest = new HashSet<GameObject>();

        public RelocationBroadcaster Broadcaster { get; private set; }
        public Scene MainScene { get; private set; }

        public RelocationTestWorld()
        {
            MainScene = SceneManager.GetActiveScene();
            rootsBeforeTheTest.UnionWith(MainScene.GetRootGameObjects());
            Broadcaster = new RelocationBroadcaster();
            Broadcaster.GroundWaitSeconds = 0.3f;
            Broadcaster.DescribeSceneGroup = () => SceneGroupName;
            RelocationBroadcaster.MakeCurrent(Broadcaster);
        }

        /// <summary>Another scene, standing for a shared scene. It is not the active scene.</summary>
        public Scene CreateOtherScene()
        {
            Scene scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
            previewScenes.Add(scene);
            return scene;
        }

        public RelocationLabel Label(string _name)
        {
            RelocationLabel label = ScriptableObject.CreateInstance<RelocationLabel>();
            label.name = _name;
            createdObjects.Add(label);
            return label;
        }

        public SpawnPoint SpawnPoint(string _name, RelocationLabel _label, Vector3 _position)
        {
            GameObject spawnObject = new GameObject(_name);
            spawnObject.transform.position = _position;
            SpawnPoint spawnPoint = spawnObject.AddComponent<SpawnPoint>();
            spawnPoint.Label = _label;
            return spawnPoint;
        }

        /// <summary>A crate: a dynamic, interpolating body with a 1 m box whose bottom is at its pivot. Not woken yet.</summary>
        public Relocatable Crate(string _name, RelocationLabel _label)
        {
            GameObject crate = new GameObject(_name);
            BoxCollider box = crate.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.5f, 0f);
            Rigidbody body = crate.AddComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            Relocatable relocatable = crate.AddComponent<Relocatable>();
            relocatable.Label = _label;
            relocatable.WaitForGroundBelow = false;
            return relocatable;
        }

        /// <summary>Stands for OnEnable in Play mode.</summary>
        public void Wake(Relocatable _relocatable)
        {
            Broadcaster.OnRelocatableEnabled(_relocatable);
        }

        public Relocatable AwakeCrate(string _name, RelocationLabel _label)
        {
            Relocatable crate = Crate(_name, _label);
            Wake(crate);
            return crate;
        }

        /// <summary>A 200 m wide ground box whose top is at the given height.</summary>
        public BoxCollider Ground(float _topHeight)
        {
            GameObject ground = new GameObject("Ground");
            ground.transform.position = new Vector3(0f, _topHeight - 0.5f, 0f);
            BoxCollider box = ground.AddComponent<BoxCollider>();
            box.size = new Vector3(400f, 1f, 400f);
            Physics.SyncTransforms();
            return box;
        }

        /// <summary>One whole scene change: Hold, Relocate, Release.</summary>
        public async Task RunSceneChange()
        {
            Broadcaster.Hold();
            await Broadcaster.Relocate();
            Broadcaster.Release();
        }

        public static async Task Frames(int _count)
        {
            for (int frame = 0; frame < _count; frame++)
            {
                await Task.Yield();
            }
        }

        public void Dispose()
        {
            RelocationBroadcaster.MakeCurrent(null);
            foreach (Object createdObject in createdObjects)
            {
                Object.DestroyImmediate(createdObject);
            }
            createdObjects.Clear();
            foreach (GameObject root in MainScene.GetRootGameObjects())
            {
                if (!rootsBeforeTheTest.Contains(root))
                {
                    Object.DestroyImmediate(root);
                }
            }
            foreach (Scene scene in previewScenes)
            {
                UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
            }
            previewScenes.Clear();
        }
    }
}
