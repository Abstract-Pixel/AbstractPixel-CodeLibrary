using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>Finding the scene system settings asset by its cached GUID, after a move, and repairing Preloaded Assets.</summary>
    public class SceneSystemSettingsAssetsTests
    {
        private const string TempRoot = "Assets/_SceneSystemSettingsTests";
        private readonly List<Object> created = new List<Object>();
        private string cachedGuidBefore;

        [SetUp]
        public void RememberCache()
        {
            cachedGuidBefore = SceneSystemSettingsAssets.CachedGuid;
        }

        [TearDown]
        public void CleanUp()
        {
            foreach (Object item in created)
            {
                if (item != null && !AssetDatabase.Contains(item))
                {
                    Object.DestroyImmediate(item);
                }
            }
            created.Clear();
            if (AssetDatabase.IsValidFolder(TempRoot))
            {
                AssetDatabase.DeleteAsset(TempRoot);
            }
            SceneSystemSettingsAssets.CachedGuid = cachedGuidBefore;
        }

        private T Make<T>() where T : ScriptableObject
        {
            T item = ScriptableObject.CreateInstance<T>();
            created.Add(item);
            return item;
        }

        [Test]
        public void WithSettingsPreloaded_Missing_AddsItOnceAndDropsNulls()
        {
            SceneSystemSettings settings = Make<SceneSystemSettings>();
            SceneSet other = Make<SceneSet>();

            Object[] repaired = SceneSystemSettingsAssets.WithSettingsPreloaded(new Object[] { other, null }, settings);

            CollectionAssert.AreEqual(new Object[] { other, settings }, repaired);
        }

        [Test]
        public void WithSettingsPreloaded_AlreadyThere_KeepsTheOrder()
        {
            SceneSystemSettings settings = Make<SceneSystemSettings>();
            SceneSet other = Make<SceneSet>();

            Object[] repaired = SceneSystemSettingsAssets.WithSettingsPreloaded(new Object[] { settings, other }, settings);

            CollectionAssert.AreEqual(new Object[] { settings, other }, repaired);
        }

        [Test]
        public void WithSettingsPreloaded_NoSettings_OnlyDropsNulls()
        {
            SceneSet other = Make<SceneSet>();

            Object[] repaired = SceneSystemSettingsAssets.WithSettingsPreloaded(new Object[] { null, other }, null);

            CollectionAssert.AreEqual(new Object[] { other }, repaired);
        }

        [Test]
        public void PickSettingsGuid_CachedGuidStillPointsAtAnAsset_KeepsIt()
        {
            string picked = SceneSystemSettingsAssets.PickSettingsGuid("cached", _guid => "Assets/Moved/Settings.asset", () => new[] { "other" });

            Assert.AreEqual("cached", picked);
        }

        [Test]
        public void PickSettingsGuid_CachedGuidGone_SearchesByType()
        {
            string picked = SceneSystemSettingsAssets.PickSettingsGuid("cached", _guid => string.Empty, () => new[] { "found" });

            Assert.AreEqual("found", picked);
        }

        [Test]
        public void PickSettingsGuid_NothingAnywhere_IsEmpty()
        {
            string picked = SceneSystemSettingsAssets.PickSettingsGuid(string.Empty, _guid => string.Empty, () => new string[0]);

            Assert.AreEqual(string.Empty, picked);
        }

        [Test]
        public void Find_AfterTheAssetIsMoved_StillFindsIt()
        {
            AssetDatabase.CreateFolder("Assets", "_SceneSystemSettingsTests");
            AssetDatabase.CreateFolder(TempRoot, "First");
            AssetDatabase.CreateFolder(TempRoot, "Second");
            SceneSystemSettings settings = Make<SceneSystemSettings>();
            AssetDatabase.CreateAsset(settings, TempRoot + "/First/Settings.asset");
            SceneSystemSettingsAssets.Remember(settings);

            string moveError = AssetDatabase.MoveAsset(TempRoot + "/First/Settings.asset", TempRoot + "/Second/Settings.asset");
            SceneSystemSettings found = SceneSystemSettingsAssets.Find();

            Assert.AreEqual(string.Empty, moveError);
            Assert.AreSame(settings, found);
            Assert.AreEqual(TempRoot + "/Second/Settings.asset", AssetDatabase.GetAssetPath(found));
        }
    }
}
