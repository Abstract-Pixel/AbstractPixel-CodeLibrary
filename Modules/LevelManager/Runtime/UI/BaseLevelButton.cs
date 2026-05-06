using System;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace AbstractPixel.LevelFramework
{
    public abstract class BaseLevelButton<TLevelDefinition, TLevelSaveData, TSceneAssetType> : MonoBehaviour
        where TLevelDefinition : BaseLevelDefinition<TSceneAssetType>
        where TLevelSaveData : BaseLevelData
    {
        [SerializeField] protected Button levelButton;
        [SerializeField] protected TMP_Text buttonText;
        public virtual void Initialize(TLevelDefinition definition, TLevelSaveData saveData, Action onClick)
        {
            levelButton.onClick.RemoveAllListeners();
            levelButton.onClick.AddListener(()=>onClick?.Invoke());
            buttonText.text = definition.LevelDisplayName;
            UpdateVisuals(saveData, definition);
        }

        public abstract void UpdateVisuals(TLevelSaveData data, TLevelDefinition definition);

        private void OnDisable()
        {
            levelButton.onClick?.RemoveAllListeners();
        }

    }
}