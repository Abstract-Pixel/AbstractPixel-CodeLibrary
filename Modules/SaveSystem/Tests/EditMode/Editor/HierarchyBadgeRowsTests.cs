#if UNITY_6000_7_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Tests
{
    // T4 drawing: the badge on rows of the real UI Toolkit Hierarchy window. Edits go through Undo, the way a
    // person's edits do, because that is what raises the change events the badge listens to.
    public class HierarchyBadgeRowsTests
    {
        const int FramesToSettle = 10;

        readonly List<GameObject> gameObjects = new List<GameObject>();
        HierarchyWindow window;

        [SetUp]
        public void SetUp()
        {
            if (Application.isBatchMode)
            {
                Assert.Ignore("Needs the Hierarchy window on screen.");
            }
            window = EditorWindow.GetWindow<HierarchyWindow>();
            HierarchyBadgeCache.Clear();
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
        }

        // Edit mode has no running save system, so a script that was never saved to disk shows "Set up", not "Saved".
        [UnityTest]
        public IEnumerator ASetUpGameObject_ShowsASetUpBadgeOnItsRow()
        {
            GameObject chest = NewSavedGameObject("Chest");
            EditorGUIUtility.PingObject(chest);
            yield return Settle();

            Label badge = BadgeOn(chest) as Label;

            Assert.IsNotNull(badge, "The Chest row has no badge.");
            Assert.AreEqual("Set up", badge.text);
            Assert.AreEqual("Saves: Chest Script", badge.tooltip);
            Assert.Greater(badge.worldBound.width, 0f, "The badge takes no space on the row.");
        }

        // The row isn't bound again when a component is added, so only the redraw after the change can update it.
        [UnityTest]
        public IEnumerator ARowOnScreen_GetsItsBadgeWhenASavedScriptIsAdded()
        {
            GameObject chest = NewGameObject("Chest");
            EditorGUIUtility.PingObject(chest);
            yield return Settle();
            Assert.IsNull(BadgeOn(chest), "Nothing is saved on the Chest yet.");

            Undo.AddComponent<SavableBridge>(chest);
            HierarchyBadgeTests.AddSavedScript<HierarchyBadgeTests.ChestScript>(chest);
            yield return Settle();

            Assert.IsNotNull(BadgeOn(chest), "The Chest row kept no badge after a saved script was added.");
        }

        [UnityTest]
        public IEnumerator AParentRowOnScreen_GetsADotWhenASavedChildMovesUnderIt_AndLosesItWhenTheChildIsDeleted()
        {
            GameObject parent = NewGameObject("Parent");
            GameObject lid = NewSavedGameObject("Lid");
            EditorGUIUtility.PingObject(parent);
            yield return Settle();
            Assert.IsNull(BadgeOn(parent), "The parent has no saved children yet.");

            Undo.SetTransformParent(lid.transform, parent.transform, "Move Lid");
            EditorGUIUtility.PingObject(lid);
            yield return Settle();
            VisualElement dot = BadgeOn(parent);
            Assert.IsNotNull(dot, "The parent row kept its old badge after a saved child moved under it.");
            Assert.AreEqual("Children save: Lid", dot.tooltip);

            Undo.DestroyObjectImmediate(lid);
            yield return Settle();
            Assert.IsNull(BadgeOn(parent), "The parent row kept its dot after its saved child was deleted.");
        }

        GameObject NewSavedGameObject(string name)
        {
            GameObject gameObject = new GameObject(name);
            HierarchyBadgeTests.AddSavedScript<HierarchyBadgeTests.ChestScript>(gameObject);
            return Track(gameObject);
        }

        GameObject NewGameObject(string name)
        {
            return Track(new GameObject(name));
        }

        GameObject Track(GameObject gameObject)
        {
            Undo.RegisterCreatedObjectUndo(gameObject, $"Create {gameObject.name}");
            gameObjects.Add(gameObject);
            return gameObject;
        }

        IEnumerator Settle()
        {
            window.Repaint();
            for (int frame = 0; frame < FramesToSettle; frame++)
            {
                yield return null;
            }
        }

        VisualElement BadgeOn(GameObject gameObject)
        {
            foreach (HierarchyViewItem item in window.rootVisualElement.Query<HierarchyViewItem>().ToList())
            {
                if (item.Handler is HierarchyGameObjectHandler handler && handler.GetGameObject(in item.Node) == gameObject)
                {
                    return item.Q(className: HierarchyBadgeRows.BadgeClass);
                }
            }
            Assert.Fail($"{gameObject.name} has no row in the Hierarchy window.");
            return null;
        }
    }
}
#endif
