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
            SetField(config, "categoryDefinitionList", new List<SaveCatgeoryDefinition>
            {
                new SaveCatgeoryDefinition { Category = SaveCategory.Settings, Location = SaveLocation.Global },
                new SaveCatgeoryDefinition { Category = SaveCategory.Game, Location = SaveLocation.GameProfile },
                new SaveCatgeoryDefinition { Category = SaveCategory.Position, Location = SaveLocation.Scene, CustomFileName = "Chests" },
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
        public string ActiveSceneName { get; set; } = "Forest";
    }

    internal sealed class FakeClock
    {
        public double Seconds;
        public double Now() => Seconds;
    }

    [Savable(SaveCategory.Settings)]
    internal sealed class VolumeSetting : ISavable<int>
    {
        public int Volume = 5;
        public int CaptureData() => Volume;
        public void RestoreData(int _loadedData) => Volume = _loadedData;
    }

    [Savable(SaveCategory.Game)]
    internal sealed class PlayerHealth : ISavable<int>
    {
        public int Current = 100;
        public int CaptureData() => Current;
        public void RestoreData(int _loadedData) => Current = _loadedData;
    }

    [Savable(SaveCategory.Position)]
    internal sealed class ChestState : ISavable<bool>
    {
        public bool IsOpen;
        public bool CaptureData() => IsOpen;
        public void RestoreData(bool _loadedData) => IsOpen = _loadedData;
    }

    [Savable(SaveCategory.Game)]
    internal sealed class ExplodingRestore : ISavable<int>
    {
        public int CaptureData() => 1;
        public void RestoreData(int _loadedData) => throw new InvalidOperationException("restore exploded");
    }
}
