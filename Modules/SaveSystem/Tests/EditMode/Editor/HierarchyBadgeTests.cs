using System.Collections.Generic;
using System.Reflection;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Tests
{
    // T4: which Hierarchy rows get a badge, a red badge or a dot. The cache decides; HierarchyBadgeRowsTests checks the drawing.
    public class HierarchyBadgeTests
    {
        internal class ChestScript : MonoBehaviour
        {
            [Save] public int gold;
        }

        class SceneScript : MonoBehaviour, ISaveIn<SaveFiles.Scene>
        {
            [Save] public int count;
        }

        class UnbridgedScript : MonoBehaviour
        {
            [Save] public int gold;
        }

        readonly List<GameObject> gameObjects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            HierarchyBadgeCache.Clear();
            SetupCheck.Forget();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject != null)
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
            gameObjects.Clear();
            HierarchyBadgeCache.Clear();
            SetupCheck.Forget();
        }

        [Test]
        public void AGameObjectWithNoBridge_GetsNoBadge()
        {
            GameObject plain = NewGameObject("Plain", null);

            Assert.AreEqual(HierarchyBadgeKind.None, HierarchyBadgeCache.Get(plain).Kind);
        }

        [Test]
        public void ABridgeWithNoSavedScripts_GetsNoBadge()
        {
            GameObject empty = NewGameObject("Empty", null);
            empty.AddComponent<SavableBridge>();

            Assert.AreEqual(HierarchyBadgeKind.None, HierarchyBadgeCache.Get(empty).Kind);
        }

        // Edit mode has no running save system, so a script that was never saved to disk shows "Set up".
        [Test]
        public void ASavedScript_GetsASetUpBadge_NamingWhatItSaves()
        {
            GameObject chest = NewGameObject("Chest", null);
            AddSavedScript<ChestScript>(chest);

            HierarchyBadgeInfo badge = HierarchyBadgeCache.Get(chest);

            Assert.AreEqual(HierarchyBadgeKind.SetUp, badge.Kind);
            Assert.AreEqual("Saves: Chest Script", badge.Tooltip);
        }

        [Test]
        public void ASavedScript_InAnotherSaveFile_NamesThatFileInTheTooltip()
        {
            GameObject chest = NewGameObject("Chest", null);
            AddSavedScript<ChestScript>(chest);
            AddSavedScript<SceneScript>(chest);

            Assert.AreEqual("Saves: Chest Script, Scene Script (Scene)", HierarchyBadgeCache.Get(chest).Tooltip);
        }

        [Test]
        public void AScriptWithNoBridge_GetsASetupWrongBadge()
        {
            GameObject broken = NewGameObject("Broken", null);
            broken.AddComponent<UnbridgedScript>();
            SetupCheck.Forget();

            Assert.AreEqual(HierarchyBadgeKind.SetupWrong, HierarchyBadgeCache.Get(broken).Kind);
        }

        [Test]
        public void AParentOfASavedChild_GetsADot_NamingTheChildren()
        {
            GameObject parent = NewGameObject("Parent", null);
            GameObject lid = NewGameObject("Lid", parent);
            GameObject loot = NewGameObject("Loot", parent);
            AddSavedScript<ChestScript>(lid);
            AddSavedScript<ChestScript>(loot);

            HierarchyBadgeInfo badge = HierarchyBadgeCache.Get(parent);

            Assert.AreEqual(HierarchyBadgeKind.ChildrenSave, badge.Kind);
            Assert.AreEqual("Children save: Lid, Loot", badge.Tooltip);
        }

        [Test]
        public void AParentThatIsSavedItself_KeepsItsOwnBadge()
        {
            GameObject parent = NewGameObject("Parent", null);
            AddSavedScript<ChestScript>(parent);
            AddSavedScript<ChestScript>(NewGameObject("Lid", parent));

            Assert.AreEqual(HierarchyBadgeKind.SetUp, HierarchyBadgeCache.Get(parent).Kind);
        }

        [Test]
        public void TheCache_ComputesOncePerGameObject_UntilCleared()
        {
            GameObject chest = NewGameObject("Chest", null);
            AddSavedScript<ChestScript>(chest);
            int before = HierarchyBadgeCache.ComputeCount;

            HierarchyBadgeCache.Get(chest);
            HierarchyBadgeCache.Get(chest);
            Assert.AreEqual(before + 1, HierarchyBadgeCache.ComputeCount);

            HierarchyBadgeCache.Clear();
            HierarchyBadgeCache.Get(chest);
            Assert.AreEqual(before + 2, HierarchyBadgeCache.ComputeCount);
        }

        [Test]
        public void AfterTheCacheIsCleared_AChangedGameObjectShowsItsNewBadge()
        {
            GameObject chest = NewGameObject("Chest", null);
            Assert.AreEqual(HierarchyBadgeKind.None, HierarchyBadgeCache.Get(chest).Kind);

            AddSavedScript<ChestScript>(chest);
            HierarchyBadgeCache.Clear();

            Assert.AreEqual(HierarchyBadgeKind.SetUp, HierarchyBadgeCache.Get(chest).Kind);
        }

        GameObject NewGameObject(string name, GameObject parent)
        {
            GameObject gameObject = new GameObject(name);
            gameObjects.Add(gameObject);
            if (parent != null)
            {
                gameObject.transform.SetParent(parent.transform);
            }
            return gameObject;
        }

        // Puts the script on the GameObject and in its bridge's target list, the way the bridge's own sync would.
        internal static void AddSavedScript<TScript>(GameObject gameObject) where TScript : MonoBehaviour
        {
            SavableBridge bridge = gameObject.GetComponent<SavableBridge>();
            if (bridge == null)
            {
                bridge = gameObject.AddComponent<SavableBridge>();
            }
            MonoBehaviour script = gameObject.AddComponent<TScript>();
            List<SavableTarget> targets = new List<SavableTarget>(bridge.SavableTargets);
            targets.Add(new SavableTarget(script, new SavableIdentification(typeof(TScript).Name, $"guid-{targets.Count}")));
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            typeof(SavableBridge).GetField("savableTargets", flags).SetValue(bridge, targets);
        }
    }
}
