using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement.Tests
{
    public class InputSourceTests
    {
        private InputActionAsset asset;
        private InputActionMap driving;
        private InputActionMap menu;

        [SetUp]
        public void SetUp()
        {
            asset = ScriptableObject.CreateInstance<InputActionAsset>();
            driving = asset.AddActionMap("Driving");
            driving.AddAction("Steer");
            menu = asset.AddActionMap("Menu");
            menu.AddAction("Back");
        }

        [TearDown]
        public void TearDown()
        {
            asset.Disable();
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void ChosenAssetEnablesOnlyTheListedMapsByName()
        {
            new InputActionAssetSource(asset).EnableOnly(new[] { "Menu" });
            Assert.IsTrue(menu.enabled);
            Assert.IsFalse(driving.enabled);
        }

        [Test]
        public void MapsCanBeListedById()
        {
            menu.Enable();
            new InputActionAssetSource(asset).EnableOnly(new[] { driving.id.ToString() });
            Assert.IsTrue(driving.enabled);
            Assert.IsFalse(menu.enabled);
        }

        [Test]
        public void SettingsWithNoInputSourceLeaveInputAlone()
        {
            GameManagementSettings settings = ScriptableObject.CreateInstance<GameManagementSettings>();
            try
            {
                Assert.IsNull(InputSourceFactory.FromSettings(settings));
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void SettingsWithAChosenAssetUseIt()
        {
            GameManagementSettings settings = ScriptableObject.CreateInstance<GameManagementSettings>();
            try
            {
                settings.InputSource = InputSourceKind.ChosenAsset;
                settings.InputActions = asset;
                IInputSource source = InputSourceFactory.FromSettings(settings);
                source.EnableOnly(new[] { "Driving" });
                Assert.IsTrue(driving.enabled);
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void ProjectWideActionsAreThoseInTheInputSystemSettings()
        {
            // Unity only accepts a saved asset here, so the test reads the project's own setting instead of setting one.
            if (InputSystem.actions == null)
            {
                UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Project Wide Actions"));
                Assert.IsNull(InputActionAssetSource.ProjectWide());
                return;
            }
            Assert.AreSame(InputSystem.actions, InputActionAssetSource.ProjectWide().Asset);
        }

        [Test]
        public void ACustomSourceReceivesTheMaps()
        {
            RecordingInputSource custom = new RecordingInputSource();
            UnityEffectSink sink = new UnityEffectSink(new FieldTimeScaleTarget(), null);
            sink.InputSource = custom;
            sink.Apply(new GameEffects { InputMapIds = new[] { "Menu" } });
            Assert.AreEqual(1, custom.Calls.Count);
        }
    }
}
