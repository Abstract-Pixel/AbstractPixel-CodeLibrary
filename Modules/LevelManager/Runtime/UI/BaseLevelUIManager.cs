using AbstractPixel.Core;
using UnityEngine;

namespace AbstractPixel.LevelFramework
{
    public class BaseLevelUIManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset> : MonoBehaviour
     where TStageDefinition : BaseStageDefinition<TLevelDefinition, TSceneAsset>
     where TLevelDefinition : BaseLevelDefinition<TSceneAsset>
     where TLevelSaveData : BaseLevelData
     where TSceneAsset : ScriptableObject
    {
        void Start()
        {

        }

        void Update()
        {

        }
    }
}