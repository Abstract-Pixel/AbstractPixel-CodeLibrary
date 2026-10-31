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
        private Action onClick;

        public virtual void Initialize(TLevelDefinition _definition, TLevelSaveData _saveData, Action _onClick)
        {
            onClick = _onClick;
            LevelButton.onClick.RemoveListener(InvokeClick);
            LevelButton.onClick.AddListener(InvokeClick);

            buttonText.text = _definition.LevelDisplayName;

            UpdateVisuals(_saveData, _definition);
        }

        public abstract void UpdateVisuals(TLevelSaveData _data, TLevelDefinition _definition);

        private void InvokeClick() => onClick?.Invoke();

        private void OnDestroy()
        {
            if (LevelButton != null) LevelButton.onClick.RemoveListener(InvokeClick);
        }
    }
}
