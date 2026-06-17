using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.LevelFramework
{
    public interface ISaveProgressionHandler<TSceneAsset, TSaveLevelData, TSaveEntry>
        where TSceneAsset : ScriptableObject
        where TSaveLevelData : BaseLevelData
        where TSaveEntry : class
    {
        Dictionary<TSceneAsset, TSaveLevelData> AllLevelDataMap { get; }
        bool IsLoaded { get; }
        TSceneAsset LastPlayedLevel { get; set; }
        List<TSaveEntry> CaptureData();
        void RestoreData(List<TSaveEntry> _loadedData);
    }
}