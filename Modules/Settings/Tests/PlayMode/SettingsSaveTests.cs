using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AbstractPixel.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.Settings.PlayModeTests
{
    // Checks that the Settings module saves and restores through the save system.
    public class SettingsSaveTests
    {
        const string BridgeId = "Settings Manager GameObject #[settings-bridge]";
        const string ScriptId = "settings-manager-script";

        // One storage for the whole test, so a restarted save system reads what the first one wrote.
        MemoryStorageService storage;
        SaveSystemConfigSO config;
        int settingsLoadedCount;
        readonly List<GameObject> createdObjects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            config = CreateConfig();
            SaveStartup.Start(config, storage, new UnitySceneInfo(), () => Time.realtimeSinceStartupAsDouble);
            settingsLoadedCount = 0;
            SettingsActions.OnSettingsLoaded += CountSettingsLoaded;
        }

        [TearDown]
        public void TearDown()
        {
            SettingsActions.OnSettingsLoaded -= CountSettingsLoaded;
            foreach (GameObject createdObject in createdObjects)
            {
                Object.Destroy(createdObject);
            }
            createdObjects.Clear();
            // Put back the save system the game itself started, for the tests that run after these.
            SaveStartup.StartFromResources();
        }

        [UnityTest]
        public IEnumerator ChangedSetting_IsBackAfterTheSaveSystemRestarts_AndSettingsLoadedFiresOnce()
        {
            SettingsManager firstManager = CreateSettingsManager(out TestToggleSetting firstToggle);
            yield return null;
            Assert.AreEqual(1, settingsLoadedCount, "A first launch applies the defaults and fires OnSettingsLoaded once.");
            Assert.IsFalse(firstToggle.CurrentValue);

            firstToggle.SetValue(true);
            SaveActions.Save("Settings");
            Object.Destroy(firstManager.gameObject);
            yield return null;

            // A new save system on the same storage is what the next launch of the game sees.
            SaveStartup.Start(config, storage, new UnitySceneInfo(), () => Time.realtimeSinceStartupAsDouble);
            settingsLoadedCount = 0;
            CreateSettingsManager(out TestToggleSetting secondToggle);
            yield return null;

            Assert.IsTrue(secondToggle.CurrentValue, "The saved setting should be restored before the manager's Start.");
            Assert.AreEqual(1, settingsLoadedCount, "OnSettingsLoaded should fire once: from the restore, not again from the defaults.");
        }

        void CountSettingsLoaded()
        {
            settingsLoadedCount++;
        }

        // Every manager gets its own registry and setting, so a restored value can't come from memory left over.
        SettingsManager CreateSettingsManager(out TestToggleSetting toggle)
        {
            toggle = new TestToggleSetting();
            SettingsCategoryGroup group = new SettingsCategoryGroup();
            group.Settings.List.Add(toggle);
            SettingsRegistry registry = ScriptableObject.CreateInstance<SettingsRegistry>();
            registry.AllSettingsList.Add(group);

            GameObject managerObject = new GameObject("Settings Manager");
            createdObjects.Add(managerObject);
            managerObject.SetActive(false);
            SettingsManager manager = managerObject.AddComponent<SettingsManager>();
            SetField(manager, "activeRegistry", registry);
            SavableBridge bridge = managerObject.AddComponent<SavableBridge>();

            // The editor fills these in OnValidate; an object built in play mode sets them itself.
            SetField(bridge, "<UniqueId>k__BackingField", BridgeId);
            SetField(bridge, "savableTargets", new List<SavableTarget>
            {
                new SavableTarget(manager, new SavableIdentification(nameof(SettingsManager), ScriptId))
            });

            managerObject.SetActive(true);
            return manager;
        }

        static SaveSystemConfigSO CreateConfig()
        {
            SaveSystemConfigSO newConfig = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            newConfig.ReplaceSaveFileList(new List<SaveFileDefinition>
            {
                SaveFileDefinition.ForClass(typeof(SettingsSaveFile), "Settings", SaveLocation.Global)
            });
            return newConfig;
        }

        static void SetField(object target, string fieldName, object value)
        {
            BindingFlags anyInstanceField = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            FieldInfo field = target.GetType().GetField(fieldName, anyInstanceField);
            if (field == null)
            {
                // Private fields declared on a base class are only found on that base class.
                field = target.GetType().BaseType.GetField(fieldName, anyInstanceField);
            }
            field.SetValue(target, value);
        }
    }

    // A setting that only stores its value, so the test checks saving and nothing else.
    [System.Serializable]
    public sealed class TestToggleSetting : BoolSetting
    {
        protected override void OnInitialize()
        {
        }

        protected override void OnApplySettingLogic()
        {
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
        }
#endif
    }
}
