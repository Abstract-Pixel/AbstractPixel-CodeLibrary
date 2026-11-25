using System;
using System.Linq;
using AbstractPixel.Core;
using AbstractPixel.Core.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>5B-1: the page and the group inspector follow changes made outside them, and fixes keep overrides honest.</summary>
    public class SceneSystemUiRefreshTests
    {
        [Test]
        public void RemoveFromGroupOnASetFollowingListMakesAnOverrideThatSurvivesTheSet()
        {
            SceneSet set = ScriptableObject.CreateInstance<SceneSet>();
            set.ManagerScenes.Add(new SceneReference("ManagerA"));
            set.ManagerScenes.Add(new SceneReference("ManagerB"));
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            group.name = "Group under test";
            group.Initialize(new SceneReference[0], new SceneReference[0], new SceneReference("Level"));
            group.SceneSet = set;
            SceneSetSync.ApplySet(group);
            SceneGroupProblem problem = new SceneGroupProblem(SceneGroupProblemKind.ListedTwice, SceneGroupProblemFix.RemoveFromGroup, SceneGroupListKind.ManagerScenes, 1, group.ManagerialBootScenesList[1], "test");

            SceneGroupProblemFixer.Apply(group, problem);
            SceneSetSync.ApplySet(group);

            Assert.AreEqual(1, group.ManagerialBootScenesList.Count);
            Assert.IsTrue(group.IsOverride(SceneSetPart.ManagerScenes));
            UnityEngine.Object.DestroyImmediate(group);
            UnityEngine.Object.DestroyImmediate(set);
        }

        [Test]
        public void TheSceneSystemPageListensToAddressablesChanges()
        {
            IAddressableSceneProvider realProvider = AddressableScenes.Provider;
            CountingProvider provider = new CountingProvider();
            AddressableScenes.Register(provider);
            try
            {
                SceneSystemSettingsProvider.SubscribeToProjectChanges();
                Assert.AreEqual(1, provider.ListenerCount);
                SceneSystemSettingsProvider.UnsubscribeFromProjectChanges();
                Assert.AreEqual(0, provider.ListenerCount);
            }
            finally
            {
                AddressableScenes.Register(realProvider);
            }
        }

        [Test]
        public void TheGroupInspectorPopupsFollowAChangeMadeOutsideIt()
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            group.name = "Group under test";
            group.Initialize(new SceneReference[0], new SceneReference[0], new SceneReference("Level"));
            SceneGroupEditor editor = (SceneGroupEditor)UnityEditor.Editor.CreateEditor(group, typeof(SceneGroupEditor));
            VisualElement root = editor.CreateInspectorGUI();
            PopupField<string> show = root.Query<PopupField<string>>().ToList().First(_field => _field.label == "Show the loading scene");
            string never = show.value;

            // What Undo and Redo do: the asset changes under the open inspector.
            group.ShowLoadingScene = ShowLoadingScene.EveryTimeRetryIncluded;
            editor.RefreshGroupState();

            Assert.AreNotEqual(never, show.value);
            Assert.AreEqual("Every time, Retry included", show.value);
            UnityEngine.Object.DestroyImmediate(editor);
            UnityEngine.Object.DestroyImmediate(group);
        }

        // Found in the 5B sweep: after the scene was deleted the scene field still said "is in build".
        [Test]
        public void TheSceneFieldListensToProjectChanges()
        {
            int calls = 0;
            Action refresh = () => calls++;

            SceneFieldPropertyDrawer.SubscribeToProject(refresh);
            RaiseProjectChanged();
            SceneFieldPropertyDrawer.UnsubscribeFromProject(refresh);
            RaiseProjectChanged();

            Assert.AreEqual(1, calls);
        }

        // Found in the 5B sweep: after the main scene was deleted, the open inspector's Problems list did not change.
        [Test]
        public void TheGroupInspectorListensToProjectChanges()
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            group.name = "Group under test";
            group.Initialize(new SceneReference[0], new SceneReference[0], new SceneReference("Level"));
            SceneGroupEditor editor = (SceneGroupEditor)UnityEditor.Editor.CreateEditor(group, typeof(SceneGroupEditor));
            VisualElement root = editor.CreateInspectorGUI();
            PopupField<string> show = root.Query<PopupField<string>>().ToList().First(_field => _field.label == "Show the loading scene");
            group.ShowLoadingScene = ShowLoadingScene.EveryTimeRetryIncluded;

            editor.SubscribeToProjectChanges();
            RaiseProjectChanged();
            editor.UnsubscribeFromProjectChanges();

            Assert.AreEqual("Every time, Retry included", show.value);
            UnityEngine.Object.DestroyImmediate(editor);
            UnityEngine.Object.DestroyImmediate(group);
        }

        // What Unity does after any asset is added, deleted, moved or renamed.
        private static void RaiseProjectChanged()
        {
            System.Reflection.MethodInfo raise = typeof(UnityEditor.EditorApplication).GetMethod("Internal_CallProjectHasChanged", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            raise.Invoke(null, null);
        }

        // A page closed with a rebuild still waiting must not stop refreshing for the rest of the session.
        [Test]
        public void ClosingThePageDropsAWaitingRefresh()
        {
            System.Reflection.FieldInfo queued = typeof(SceneSystemSettingsProvider).GetField("refreshQueued", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            queued.SetValue(null, true);

            SceneSystemSettingsProvider.Deactivate();

            Assert.IsFalse((bool)queued.GetValue(null));
        }

        private sealed class CountingProvider : IAddressableSceneProvider
        {
            private Action changed;

            public event Action Changed
            {
                add { changed += value; }
                remove { changed -= value; }
            }

            public int ListenerCount
            {
                get { return changed == null ? 0 : changed.GetInvocationList().Length; }
            }

            public bool IsAddressable(string _sceneGuid)
            {
                return false;
            }

            public bool TryMakeAddressable(string _sceneGuid, out string _error)
            {
                _error = "test provider";
                return false;
            }

            public void RemoveFromAddressables(string _sceneGuid)
            {
            }
        }
    }
}
