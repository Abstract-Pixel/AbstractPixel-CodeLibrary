using NUnit.Framework;

namespace AbstractPixel.SaveSystem.Tests
{
    // When a Savable Bridge keeps its GUID, and when it needs a new one. The GUID is the save ID, so changing it loses data.
    public class SavableBridgeIdentityTests
    {
        const string UniqueId = "Chest GameObject #[0f3c]";
        const string NeverSavedScene = "GlobalObjectId_V1-0-00000000000000000000000000000000-0-0";
        const string Forest = "GlobalObjectId_V1-2-26af0e3f168c3f44d82814c8567e27ed-211392003-0";
        const string ForestCopyOfTheChest = "GlobalObjectId_V1-2-26af0e3f168c3f44d82814c8567e27ed-59260639-0";
        const string ForestSavedAsCave = "GlobalObjectId_V1-2-9b1d7a0c2e5f4b8aa1c3d5e7f9012345-211392003-0";

        [Test]
        public void ABridgeWithNoGuid_NeedsOne()
        {
            Assert.IsTrue(SavableBridgeIdentity.NeedsNewGuid(string.Empty, string.Empty, NeverSavedScene));
        }

        [Test]
        public void ABridgeInANeverSavedScene_KeepsItsGuid_WhenTheSceneIsFirstSaved()
        {
            Assert.IsFalse(SavableBridgeIdentity.NeedsNewGuid(UniqueId, NeverSavedScene, Forest), "Bug 9: the first save of a scene changed every bridge's save ID.");
            Assert.IsFalse(SavableBridgeIdentity.NeedsNewGuid(UniqueId, string.Empty, Forest));
        }

        [Test]
        public void ABridgeWhoseSceneIsSavedUnderAnotherName_KeepsItsGuid()
        {
            Assert.IsFalse(SavableBridgeIdentity.NeedsNewGuid(UniqueId, Forest, ForestSavedAsCave));
        }

        [Test]
        public void ACopyOfABridge_NeedsANewGuid()
        {
            Assert.IsTrue(SavableBridgeIdentity.NeedsNewGuid(UniqueId, Forest, ForestCopyOfTheChest));
        }

        [Test]
        public void TheSameBridge_KeepsItsGuid()
        {
            Assert.IsFalse(SavableBridgeIdentity.NeedsNewGuid(UniqueId, Forest, Forest));
        }

        [Test]
        public void OnlyAnIdFromASavedScene_IsWorthRemembering()
        {
            Assert.IsFalse(SavableBridgeIdentity.IsFromASavedScene(NeverSavedScene));
            Assert.IsTrue(SavableBridgeIdentity.IsFromASavedScene(Forest));
        }
    }
}
