using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Tests
{
    /// <summary>Shared pieces for tests that run the whole save system on memory storage.</summary>
    internal static class SaveSystemTestKit
    {
        public const string SettingsFile = "Settings";   // Global
        public const string GameFile = "Game";           // GameProfile
        public const string ChestsFile = "Position";     // Scene, stored as "Chests"

        /// <summary>A config with one save file in each save location.</summary>
        public static SaveSystemConfigSO CreateConfig()
        {
            SaveSystemConfigSO config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            SetField(config, "<GameProfileGuidLength>k__BackingField", 6);
            SaveFileDefinition chestsRow = SaveFileDefinition.ForClass(typeof(TestChestsFile), ChestsFile, SaveLocation.Scene);
            chestsRow.StorageName = "Chests";
            config.ReplaceSaveFileList(new List<SaveFileDefinition>
            {
                SaveFileDefinition.ForClass(typeof(TestSettingsFile), SettingsFile, SaveLocation.Global),
                SaveFileDefinition.ForClass(typeof(TestGameFile), GameFile, SaveLocation.GameProfile),
                chestsRow,
            });
            return config;
        }

        public static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            field.SetValue(target, value);
        }
    }

    internal sealed class FakeSceneInfo : ISceneInfo
    {
        readonly List<string> loadedSceneNames = new List<string> { "Forest" };

        public string ActiveSceneName { get; set; } = "Forest";

        public IReadOnlyList<string> LoadedSceneNames => loadedSceneNames;

        public event Action<string> SceneReady;
        public event Action<string> SceneUnloaded;

        /// <summary>Like loading a scene additively: it joins the loaded scenes, then says it is ready.</summary>
        public void LoadAdditively(string sceneName)
        {
            SignalReady(sceneName);
        }

        /// <summary>What Unity does one frame after a loaded scene's scripts start, when its Savable Bridges have restored.</summary>
        public void SignalReady(string sceneName)
        {
            if (!loadedSceneNames.Contains(sceneName))
            {
                loadedSceneNames.Add(sceneName);
            }
            SceneReady?.Invoke(sceneName);
        }

        public void Unload(string sceneName)
        {
            loadedSceneNames.Remove(sceneName);
            SceneUnloaded?.Invoke(sceneName);
        }
    }

    internal sealed class FakeClock
    {
        public double Seconds;
        public double Now() => Seconds;
    }

    internal sealed class TestSettingsFile : ISaveFile { }
    internal sealed class TestGameFile : ISaveFile { }
    internal sealed class TestChestsFile : ISaveFile { }

    internal sealed class VolumeSetting : ISavable<int, TestSettingsFile>
    {
        public int Volume = 5;
        public int CaptureData() => Volume;
        public void RestoreData(int _loadedData) => Volume = _loadedData;
    }

    internal sealed class PlayerHealth : ISavable<int, TestGameFile>
    {
        public int Current = 100;
        public int CaptureData() => Current;
        public void RestoreData(int _loadedData) => Current = _loadedData;
    }

    internal sealed class ChestState : ISavable<bool, TestChestsFile>
    {
        public bool IsOpen;
        public bool CaptureData() => IsOpen;
        public void RestoreData(bool _loadedData) => IsOpen = _loadedData;
    }

    internal sealed class ExplodingRestore : ISavable<int, TestGameFile>
    {
        public int CaptureData() => 1;
        public void RestoreData(int _loadedData) => throw new InvalidOperationException("restore exploded");
    }
}
