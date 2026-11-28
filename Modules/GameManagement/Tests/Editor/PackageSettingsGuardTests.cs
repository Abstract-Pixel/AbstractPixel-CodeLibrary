using AbstractPixel.GameManagement.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.GameManagement.Tests
{
    /// <summary>A catalog that lives in a package (a sample's) is never written to: no add, remove, move or replace.</summary>
    public class PackageSettingsGuardTests
    {
        private const string SampleCatalogPath = "Packages/com.abstractpixel.library/Modules/GameManagement/Samples/01 Minimal Game/States/Minimal Game Catalog.asset";

        private GameStateCatalog sampleCatalog;
        private StateSO extra;

        [SetUp]
        public void SetUp()
        {
            sampleCatalog = AssetDatabase.LoadAssetAtPath<GameStateCatalog>(SampleCatalogPath);
            extra = ScriptableObject.CreateInstance<StateSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(extra);
        }

        private static void ExpectOneWarning()
        {
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("inside a package"));
        }

        [Test]
        public void Add_DoesNotChangeAPackageCatalog()
        {
            Assert.IsNotNull(sampleCatalog, "The sample catalog is missing.");
            int before = sampleCatalog.States.Count;
            ExpectOneWarning();

            StateListEdits.Add(sampleCatalog, extra);

            Assert.AreEqual(before, sampleCatalog.States.Count);
        }

        [Test]
        public void RemoveMoveAndReplace_DoNotChangeAPackageCatalog()
        {
            StateSO first = sampleCatalog.States[0];
            StateSO second = sampleCatalog.States[1];
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("inside a package"));
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("inside a package"));
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("inside a package"));

            StateListEdits.RemoveAt(sampleCatalog, 0);
            StateListEdits.Move(sampleCatalog, 0, 1);
            StateListEdits.Replace(sampleCatalog, 0, extra);

            Assert.AreEqual(first, sampleCatalog.States[0]);
            Assert.AreEqual(second, sampleCatalog.States[1]);
        }

        [Test]
        public void ACatalogInTheProject_StillChanges()
        {
            GameStateCatalog catalog = ScriptableObject.CreateInstance<GameStateCatalog>();

            StateListEdits.Add(catalog, extra);

            Assert.AreEqual(1, catalog.States.Count);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void PackageSettings_AreReportedAsPackageAssets()
        {
            GameManagementSettings sample = AssetDatabase.LoadAssetAtPath<GameManagementSettings>("Packages/com.abstractpixel.library/Modules/GameManagement/Samples/01 Minimal Game/States/Minimal Game Settings.asset");

            Assert.IsTrue(GameManagementSettingsAssets.IsPackageAsset(sample));
            Assert.IsFalse(GameManagementSettingsAssets.IsPackageAsset(null));
        }
    }
}
