using System.Threading.Tasks;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>What only the Addressables loader owes: one open copy per scene, and keeping the handle until close.</summary>
    public class AddressablesSceneLoaderTests
    {
        private AddressablesLoaderHarness harness;

        [SetUp]
        public void CreateLoader()
        {
            harness = new AddressablesLoaderHarness();
        }

        [Test]
        public async Task LoadingASceneThatIsAlreadyOpenDoesNotOpenItAgain()
        {
            SceneReference level = harness.ReferenceTo("OpenOnce");
            await harness.Loader.LoadScene(level, true, true, true);

            await harness.Loader.LoadScene(level, true, true, true);

            Assert.AreEqual(1, harness.Api.LoadedKeys.Count);
        }

        [Test]
        public async Task TheHandleIsKeptWhileTheSceneIsOpenAndReleasedWhenItCloses()
        {
            SceneReference level = harness.ReferenceTo("HandleKept");
            await harness.Loader.LoadScene(level, true, true, true);
            Assert.AreEqual(1, harness.Api.HandlesInUse, "the handle lives while the scene is open");

            await harness.Loader.UnloadScene(level);

            Assert.AreEqual(0, harness.Api.HandlesInUse, "closing releases the handle, so the assets can leave memory");
        }

        [Test]
        public async Task ClosingAHeldSceneAlsoReleasesItsHandle()
        {
            SceneReference level = harness.ReferenceTo("HeldHandle");
            await harness.Loader.LoadScene(level, true, false, true);

            await harness.Loader.UnloadScene(level);

            Assert.AreEqual(0, harness.Api.HandlesInUse);
        }

        [Test]
        public async Task ASceneIsOnlyClosedOnceEvenIfAskedTwice()
        {
            SceneReference level = harness.ReferenceTo("ClosedTwice");
            await harness.Loader.LoadScene(level, true, true, true);
            await harness.Loader.UnloadScene(level);

            await harness.Loader.UnloadScene(level);

            Assert.AreEqual(1, harness.Api.ClosedKeys.Count);
        }

        [Test]
        public async Task ASceneReferenceWithoutAGuidIsRefusedWithAnError()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("needs its asset GUID"));

            await harness.Loader.LoadScene(new SceneReference("NameOnly"), true, true, true);

            Assert.AreEqual(0, harness.Api.LoadedKeys.Count, "nothing opens without a GUID");
        }
    }
}
