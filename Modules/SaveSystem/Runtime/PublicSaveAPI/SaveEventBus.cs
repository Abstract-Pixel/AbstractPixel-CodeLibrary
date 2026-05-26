using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    public static class SaveEventBus
    {
        // STATE TRACKING FOR STICKY EVENTS 
        private static SaveCategory lastLoadedCategory = SaveCategory.None;
        private static SaveScope lastLoadedScope = SaveScope.None;

        //BACKING DELEGATE FIELDS
        private static Action onLoadAllCompleted = delegate { };
        private static Action<SaveCategory> onLoadCategoryCompleted = delegate { };
        private static Action<SaveScope> onLoadScopeCompleted= delegate { };

        // STATELESS SAVE EVENTS 
        public static event Action OnSaveAllCompleted = delegate { };
        public static event Action<SaveCategory> OnSaveCategoryCompleted= delegate { };
        public static event Action<SaveScope> OnSaveScopeCompleted= delegate { };

        //STATEFUL LOAD EVENTS (Sticky Accessor Pattern)
        public static event Action OnLoadAllCompleted
        {
            add
            {
                onLoadAllCompleted += value;
                if (SaveActions.IsDataLoaded)
                {
                    value.Invoke();
                }
            }
            remove => onLoadAllCompleted -= value;
        }

        public static event Action<SaveCategory> OnLoadCategoryCompleted
        {
            add
            {
                onLoadCategoryCompleted += value;
                if (SaveActions.IsDataLoaded)
                {
                    value.Invoke(lastLoadedCategory);
                }
            }
            remove => onLoadCategoryCompleted -= value;
        }

        public static event Action<SaveScope> OnLoadScopeCompleted
        {
            add
            {
                onLoadScopeCompleted += value;
                if (SaveActions.IsDataLoaded)
                {
                    value.Invoke(lastLoadedScope);
                }
            }
            remove => onLoadScopeCompleted -= value;
        }

        // EVENT RAISE METHODS (Saves)
        public static void RaiseOnSaveAllCompleted() => OnSaveAllCompleted?.Invoke();
        public static void RaiseOnSaveCategoryCompleted(SaveCategory _category) => OnSaveCategoryCompleted?.Invoke(_category);
        public static void RaiseOnSaveScopeCompleted(SaveScope _scope) => OnSaveScopeCompleted?.Invoke(_scope);

        // EVENT RAISE METHODS (Loads)
        public static void RaiseOnLoadAllCompleted() => onLoadAllCompleted?.Invoke();

        public static void RaiseOnLoadCategoryCompleted(SaveCategory _category)
        {
            lastLoadedCategory = _category;
            onLoadCategoryCompleted?.Invoke(_category);
        }

        public static void RaiseOnLoadScopeCompleted(SaveScope _scope)
        {
            lastLoadedScope = _scope;
            onLoadScopeCompleted?.Invoke(_scope);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetStatics()
        {
            // Reset standard auto-events
            OnSaveAllCompleted = delegate { };
            OnSaveCategoryCompleted = delegate { };
            OnSaveScopeCompleted = delegate { };

            // Reset custom backing delegates
            onLoadAllCompleted = delegate { };
            onLoadCategoryCompleted = delegate { };
            onLoadScopeCompleted = delegate { };

            // Reset state variables
            lastLoadedCategory = SaveCategory.None;
            lastLoadedScope = SaveScope.None;
        }
    }
}