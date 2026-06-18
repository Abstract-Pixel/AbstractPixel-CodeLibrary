using UnityEngine;

namespace AbstractPixel.LevelFramework
{
    /// <summary>
    /// Provides a static, globally accessible API to interact with the CoreLevelManager.
    /// </summary>
    public abstract class LevelActions<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset, TSaveEntry>
        where TStageDefinition : BaseStageDefinition<TLevelDefinition, TSceneAsset>
        where TLevelDefinition : BaseLevelDefinition<TSceneAsset>
        where TLevelSaveData : BaseLevelData
        where TSceneAsset : ScriptableObject
        where TSaveEntry : class
    {
        // C# does not allow calling static properties (like Instance) on a generic type parameter (TManager).
        // We route it through the base class which holds the PersistentSingleton implementation.
        private static CoreLevelManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset, TSaveEntry> Manager
            => CoreLevelManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset, TSaveEntry>.Instance;

        public static bool IsInitialized => Manager != null && Manager.IsInitialized;
        public static TStageDefinition CurrentStage => Manager.activeStageDefinition ;
        public static TLevelDefinition CurrentLevel => Manager.activeLevelDefinition ;
        public static TSceneAsset CurrentSceneAsset => Manager.activeSceneAsset ;
        public static TLevelSaveData CurrentLevelData
        {
            get
            {
                if (Manager == null || CurrentSceneAsset == null) return null;
                return Manager.GetLevelSaveData(CurrentSceneAsset);
            }
        }

        public static void LoadNextLevel() => Manager?.LoadNextLevel();

        public static void LoadToLevel(TSceneAsset _sceneAsset) => Manager?.LoadToLevel(_sceneAsset);

        public static void MarkCurrentLevelForCompletion(TLevelSaveData _levelDataToOverride) => Manager?.MarkCurrentLevelForCompletion(_levelDataToOverride);

        public static void UnlockNextLevel() => Manager?.UnlockNextLevel();

        public static void ResetManager() => Manager?.ResetManager();

        public static TSceneAsset GetLastPlayedLevel() => Manager?.GetLastPlayedLevel();

        public static TLevelDefinition GetLevelDefinitionOf(TSceneAsset _sceneAsset) => Manager?.GetLevelDefinition(_sceneAsset);

        public static TLevelSaveData GetLevelSaveDataOf(TSceneAsset _sceneAsset) => Manager?.GetLevelSaveData(_sceneAsset);
    }
}