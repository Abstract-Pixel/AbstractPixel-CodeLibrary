using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.Serialization;

namespace AbstractPixel.SaveSystem.Tests
{
    public class SaveLocationTests
    {
        const string DefaultConfigAssetPath = "Packages/com.abstractpixel.library/Modules/SaveSystem/Runtime/SaveSystemConfigSO.asset";

        [Test]
        public void SaveLocation_KeepsTheV1NumbersAndAddsScene()
        {
            // Configs saved with v1 store these numbers, so Global and GameProfile must keep them.
            Assert.AreEqual(1, (int)SaveLocation.Global);
            Assert.AreEqual(2, (int)SaveLocation.GameProfile);
            Assert.AreEqual(3, (int)SaveLocation.Scene);
        }

        [Test]
        public void CategoryDefinition_LocationReadsDataSavedAsDirectoryScope()
        {
            FieldInfo location = typeof(SaveCatgeoryDefinition).GetField(nameof(SaveCatgeoryDefinition.Location));
            FormerlySerializedAsAttribute formerName = location.GetCustomAttribute<FormerlySerializedAsAttribute>();

            Assert.AreEqual(typeof(SaveLocation), location.FieldType);
            Assert.IsNotNull(formerName, "Location needs [FormerlySerializedAs(\"DirectoryScope\")] so v1 configs keep their values.");
            Assert.AreEqual("DirectoryScope", formerName.oldName);
        }

        [Test]
        public void CategoryDefinition_HasNoSceneSpecificFlag()
        {
            Assert.IsNull(typeof(SaveCatgeoryDefinition).GetField("IsSceneSpecific"), "Scene is now a save location, not a flag.");
        }

        [Test]
        public void DefaultConfigAsset_UsesLocationInsteadOfTheOldFields()
        {
            string assetText = File.ReadAllText(Path.GetFullPath(DefaultConfigAssetPath));

            StringAssert.Contains("Location:", assetText);
            StringAssert.DoesNotContain("DirectoryScope", assetText);
            StringAssert.DoesNotContain("IsSceneSpecific", assetText);
        }
    }
}
