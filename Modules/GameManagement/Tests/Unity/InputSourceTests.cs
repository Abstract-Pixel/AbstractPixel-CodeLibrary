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
        public void ChosenAssetEnablesOnlyTheListedMaps()
        {
            new InputActionAssetSource(asset).EnableOnly(new[] { menu });
            Assert.IsTrue(menu.enabled);
            Assert.IsFalse(driving.enabled);
        }

        [Test]
        public void AStateTurnsOnTheMapOfEachActionItLists()
        {
            menu.Enable();
            StateSO play = ScriptableObject.CreateInstance<StateSO>();
            InputActionReference steer = InputActionReference.Create(driving.FindAction("Steer"));
            try
            {
                play.Effects = new StateEffectSettings { InputSource = EffectSource.Own };
                play.Effects.InputMaps.Add(steer);
                GameEffects effects = EffectResolver.Resolve(new[] { play }, NoStateTimeScale.KeepLast);
                new InputActionAssetSource(asset).EnableOnly(effects.InputMaps);
                Assert.IsTrue(driving.enabled, "the map of the listed action is on");
                Assert.IsFalse(menu.enabled, "every other map is off");
            }
            finally
            {
                Object.DestroyImmediate(steer);
                Object.DestroyImmediate(play);
            }
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
                source.EnableOnly(new[] { driving });
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
            sink.Apply(new GameEffects { InputMaps = new[] { menu } });
            Assert.AreEqual(1, custom.Calls.Count);
        }
    }
}
