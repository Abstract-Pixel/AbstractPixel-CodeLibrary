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

        public virtual void Initialize(TLevelDefinition _definition, TLevelSaveData _saveData, Action _onClick)
        {
            levelButton.onClick.RemoveAllListeners();
            levelButton.onClick.AddListener(() => _onClick?.Invoke());

            buttonText.text = _definition.LevelDisplayName;

            UpdateVisuals(_saveData, _definition);
        }

        public abstract void UpdateVisuals(TLevelSaveData _data, TLevelDefinition _definition);

        private void OnDisable()
        {
            levelButton.onClick?.RemoveAllListeners();
        }
    }
}