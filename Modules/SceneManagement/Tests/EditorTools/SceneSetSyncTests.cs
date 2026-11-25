using System.Collections.Generic;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>Scene sets fill scene groups: copying, overrides, override detection and Revert to set (ADR 0008).</summary>
    public class SceneSetSyncTests
    {
        private readonly List<Object> created = new List<Object>();

        [TearDown]
        public void DestroyCreated()
        {
            foreach (Object item in created)
            {
                Object.DestroyImmediate(item);
            }
            created.Clear();
        }

        private static SceneReference Scene(string _name)
        {
            return new SceneReference(_name, "guid-" + _name, "Assets/" + _name + ".unity");
        }

        private SceneSet Set(string[] _managers, string[] _shared, bool _reset)
        {
            SceneSet set = ScriptableObject.CreateInstance<SceneSet>();
            foreach (string name in _managers)
            {
                set.ManagerScenes.Add(Scene(name));
            }
            foreach (string name in _shared)
            {
                set.SharedScenes.Add(Scene(name));
            }
            set.ResetSharedScenes = _reset;
            created.Add(set);
            return set;
        }

        private SceneGroup Group(string[] _managers, string[] _shared, bool _reset, SceneSet _set)
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            List<SceneReference> managers = new List<SceneReference>();
            foreach (string name in _managers)
            {
                managers.Add(Scene(name));
            }
            List<SceneReference> shared = new List<SceneReference>();
            foreach (string name in _shared)
            {
                shared.Add(Scene(name));
            }
            group.Initialize(managers, shared, Scene("Level 1"), _reset);
            group.SceneSet = _set;
            created.Add(group);
            return group;
        }

        private static List<string> Names(List<SceneReference> _scenes)
        {
            List<string> names = new List<string>();
            foreach (SceneReference scene in _scenes)
            {
                names.Add(scene.SceneName);
            }
            return names;
        }

        [Test]
        public void ApplySet_NoOverrides_CopiesAllThreeParts()
        {
            SceneSet set = Set(new[] { "Managers" }, new[] { "Car" }, true);
            SceneGroup group = Group(new string[0], new string[0], false, set);

            bool changed = SceneSetSync.ApplySet(group);

            Assert.IsTrue(changed);
            CollectionAssert.AreEqual(new[] { "Managers" }, Names(group.ManagerialBootScenesList));
            CollectionAssert.AreEqual(new[] { "Car" }, Names(group.ContextualBootScenesList));
            Assert.IsTrue(group.ForceReloadContextualScenes);
        }

        [Test]
        public void ApplySet_EachOverrideCombination_KeepsOnlyTheOverrides([Values] bool _managersOverride, [Values] bool _sharedOverride, [Values] bool _resetOverride)
        {
            SceneSet set = Set(new[] { "SetManagers" }, new[] { "SetCar" }, true);
            SceneGroup group = Group(new[] { "OwnManagers" }, new[] { "OwnCar" }, false, set);
            group.SetOverride(SceneSetPart.ManagerScenes, _managersOverride);
            group.SetOverride(SceneSetPart.SharedScenes, _sharedOverride);
            group.SetOverride(SceneSetPart.ResetSharedScenes, _resetOverride);

            SceneSetSync.ApplySet(group);

            string expectedManager = _managersOverride ? "OwnManagers" : "SetManagers";
            string expectedShared = _sharedOverride ? "OwnCar" : "SetCar";
            bool expectedReset = _resetOverride ? false : true;
            CollectionAssert.AreEqual(new[] { expectedManager }, Names(group.ManagerialBootScenesList));
            CollectionAssert.AreEqual(new[] { expectedShared }, Names(group.ContextualBootScenesList));
            Assert.AreEqual(expectedReset, group.ForceReloadContextualScenes);
            Assert.AreEqual(_managersOverride, group.IsOverride(SceneSetPart.ManagerScenes));
            Assert.AreEqual(_sharedOverride, group.IsOverride(SceneSetPart.SharedScenes));
            Assert.AreEqual(_resetOverride, group.IsOverride(SceneSetPart.ResetSharedScenes));
        }

        [Test]
        public void ApplySet_NoSet_ChangesNothing()
        {
            SceneGroup group = Group(new[] { "OwnManagers" }, new[] { "OwnCar" }, true, null);

            bool changed = SceneSetSync.ApplySet(group);

            Assert.IsFalse(changed);
            CollectionAssert.AreEqual(new[] { "OwnManagers" }, Names(group.ManagerialBootScenesList));
            Assert.IsTrue(group.ForceReloadContextualScenes);
        }

        [Test]
        public void ApplySet_Twice_ReportsNoChangeTheSecondTime()
        {
            SceneSet set = Set(new[] { "Managers" }, new[] { "Car" }, true);
            SceneGroup group = Group(new string[0], new string[0], false, set);
            SceneSetSync.ApplySet(group);

            bool changedAgain = SceneSetSync.ApplySet(group);

            Assert.IsFalse(changedAgain);
        }

        [Test]
        public void ApplySet_CopiesTheLists_SoEditingTheGroupLeavesTheSetAlone()
        {
            SceneSet set = Set(new[] { "Managers" }, new[] { "Car" }, true);
            SceneGroup group = Group(new string[0], new string[0], false, set);
            SceneSetSync.ApplySet(group);

            group.ManagerialBootScenesList.Add(Scene("Extra"));

            Assert.AreEqual(1, set.ManagerScenes.Count);
        }

        [Test]
        public void RevertToSet_TakesTheSetValueAndClearsTheOverride()
        {
            SceneSet set = Set(new[] { "SetManagers" }, new[] { "SetCar" }, true);
            SceneGroup group = Group(new[] { "OwnManagers" }, new[] { "OwnCar" }, false, set);
            group.SetOverride(SceneSetPart.SharedScenes, true);
            group.SetOverride(SceneSetPart.ResetSharedScenes, true);

            SceneSetSync.RevertToSet(group, SceneSetPart.SharedScenes);

            CollectionAssert.AreEqual(new[] { "SetCar" }, Names(group.ContextualBootScenesList));
            Assert.IsFalse(group.IsOverride(SceneSetPart.SharedScenes));
            Assert.IsTrue(group.IsOverride(SceneSetPart.ResetSharedScenes));
            Assert.IsFalse(group.ForceReloadContextualScenes);
        }

        [Test]
        public void UpdateOverrideAfterEdit_ValueDiffers_BecomesAnOverride()
        {
            SceneSet set = Set(new[] { "Managers" }, new[] { "Car" }, true);
            SceneGroup group = Group(new[] { "Managers" }, new[] { "Car" }, true, set);
            group.ContextualBootScenesList.Add(Scene("Trailer"));

            bool flagChanged = SceneSetSync.UpdateOverrideAfterEdit(group, SceneSetPart.SharedScenes);

            Assert.IsTrue(flagChanged);
            Assert.IsTrue(group.IsOverride(SceneSetPart.SharedScenes));
            Assert.IsFalse(group.IsOverride(SceneSetPart.ManagerScenes));
        }

        [Test]
        public void UpdateOverrideAfterEdit_EditedBackToTheSetValue_IsNoLongerAnOverride()
        {
            SceneSet set = Set(new[] { "Managers" }, new[] { "Car" }, true);
            SceneGroup group = Group(new[] { "Managers" }, new[] { "Car" }, false, set);
            SceneSetSync.UpdateOverrideAfterEdit(group, SceneSetPart.ResetSharedScenes);
            Assert.IsTrue(group.IsOverride(SceneSetPart.ResetSharedScenes));

            group.ForceReloadContextualScenes = true;
            SceneSetSync.UpdateOverrideAfterEdit(group, SceneSetPart.ResetSharedScenes);

            Assert.IsFalse(group.IsOverride(SceneSetPart.ResetSharedScenes));
        }

        [Test]
        public void UpdateOverrideAfterEdit_NoSet_NeverAnOverride()
        {
            SceneGroup group = Group(new[] { "Managers" }, new[] { "Car" }, false, null);

            SceneSetSync.UpdateOverrideAfterEdit(group, SceneSetPart.ManagerScenes);

            Assert.IsFalse(group.IsOverride(SceneSetPart.ManagerScenes));
        }

        [Test]
        public void SameScenes_OrderMatters_AndGuidWinsOverName()
        {
            List<SceneReference> first = new List<SceneReference> { Scene("A"), Scene("B") };
            List<SceneReference> reversed = new List<SceneReference> { Scene("B"), Scene("A") };
            List<SceneReference> renamed = new List<SceneReference> { new SceneReference("A renamed", "guid-A", "Assets/A renamed.unity"), Scene("B") };

            Assert.IsFalse(SceneSetSync.SameScenes(first, reversed));
            Assert.IsTrue(SceneSetSync.SameScenes(first, renamed));
        }
    }
}
