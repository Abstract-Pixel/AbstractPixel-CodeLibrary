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
        [field:SerializeField] public Button LevelButton {  get; private set; }
        [SerializeField] protected TMP_Text buttonText;

        public virtual void Initialize(TLevelDefinition _definition, TLevelSaveData _saveData, Action _onClick)
        {
            LevelButton.onClick.RemoveAllListeners();
            LevelButton.onClick.AddListener(() => _onClick?.Invoke());

            buttonText.text = _definition.LevelDisplayName;

            UpdateVisuals(_saveData, _definition);
        }

        public abstract void UpdateVisuals(TLevelSaveData _data, TLevelDefinition _definition);

        private void OnDestroy()
        {
            LevelButton.onClick?.RemoveAllListeners();
        }
    }
}