using System;
using UnityEngine;
using UnityEngine.Events;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Lets designers save, load and manage game profiles from UnityEvents, with no code.</summary>
    [DisallowMultipleComponent]
    public class MonoSaveActions : MonoBehaviour
    {
        /// <summary>A UnityEvent with the save-failed reason, so a designer can wire it straight to a Text component.</summary>
        [Serializable] public class SaveFailedEvent : UnityEvent<string> { }

        [SerializeField, Tooltip("The save file Save Chosen File, Reload Chosen File and the On Loaded/On Saved/On Save Failed events use. Pick it from the config's save file list.")]
        SaveFileReference chosenSaveFile;

        [SerializeField] UnityEvent onLoaded = new UnityEvent();
        [SerializeField] UnityEvent onSaved = new UnityEvent();
        [SerializeField] SaveFailedEvent onSaveFailed = new SaveFailedEvent();
        [SerializeField] UnityEvent onGameProfileOpened = new UnityEvent();

        /// <summary>The chosen save file loaded, or was reloaded. Sticky: fires right away if it already loaded.</summary>
        public UnityEvent OnLoaded => onLoaded;

        /// <summary>The chosen save file was written successfully.</summary>
        public UnityEvent OnSaved => onSaved;

        /// <summary>The chosen save file could not be written. Gives a reason to show the player.</summary>
        public SaveFailedEvent OnSaveFailed => onSaveFailed;

        /// <summary>A game profile opened.</summary>
        public UnityEvent OnGameProfileOpened => onGameProfileOpened;

        void OnEnable()
        {
            SaveEventBus.OnLoadCompleted += HandleLoadCompleted;
            SaveEventBus.OnSaveCompleted += HandleSaveCompleted;
            SaveEventBus.OnSaveFailed += HandleSaveFailed;
            SaveEventBus.OnGameProfileOpened += HandleGameProfileOpened;
        }

        void OnDisable()
        {
            SaveEventBus.OnLoadCompleted -= HandleLoadCompleted;
            SaveEventBus.OnSaveCompleted -= HandleSaveCompleted;
            SaveEventBus.OnSaveFailed -= HandleSaveFailed;
            SaveEventBus.OnGameProfileOpened -= HandleGameProfileOpened;
        }

        public void SaveAll() => SaveActions.SaveAll();

        /// <summary>Saves the save file picked in the inspector. A UnityEvent can call this, because it takes no arguments.</summary>
        public void SaveChosenFile()
        {
            if (!RequireChosenFile(nameof(SaveChosenFile)))
            {
                return;
            }
            SaveActions.Save(chosenSaveFile);
        }

        /// <summary>Reloads the save file picked in the inspector, throwing away unsaved changes.</summary>
        public void ReloadChosenFile()
        {
            if (!RequireChosenFile(nameof(ReloadChosenFile)))
            {
                return;
            }
            SaveActions.Reload(chosenSaveFile);
        }

        /// <summary>Saves one save file. In code, pass <c>SaveFileReference.To&lt;GameSaveFiles.Ghosts&gt;()</c>.</summary>
        public void Save(SaveFileReference saveFile) => SaveActions.Save(saveFile);

        /// <summary>Opens the game profile in this slot. Does nothing, and logs a warning, if the slot is empty.</summary>
        public void OpenSlot(int slotNumber)
        {
            GameProfile profile = SaveActions.GameProfiles.Get(slotNumber);
            if (profile == null)
            {
                Debug.LogWarning($"[SaveSystem] '{name}' was asked to open slot {slotNumber}, but it is empty. Call NewGame instead, or check SaveActions.GameProfiles.Count first.", this);
                return;
            }
            profile.Open();
        }

        /// <summary>Creates a new game profile in the lowest free slot and opens it.</summary>
        public void NewGame() => SaveActions.GameProfiles.CreateNew()?.Open();

        bool RequireChosenFile(string methodName)
        {
            if (chosenSaveFile.IsSet)
            {
                return true;
            }
            Debug.LogError($"[SaveSystem] '{name}' was asked to {methodName}, but no save file is picked. Pick one in its MonoSaveActions inspector.", this);
            return false;
        }

        void HandleLoadCompleted(Type saveFileClass)
        {
            if (IsChosenFile(saveFileClass))
            {
                onLoaded.Invoke();
            }
        }

        void HandleSaveCompleted(Type saveFileClass)
        {
            if (IsChosenFile(saveFileClass))
            {
                onSaved.Invoke();
            }
        }

        void HandleSaveFailed(Type saveFileClass, string reason)
        {
            if (IsChosenFile(saveFileClass))
            {
                onSaveFailed.Invoke(reason);
            }
        }

        void HandleGameProfileOpened(GameProfile profile) => onGameProfileOpened.Invoke();

        bool IsChosenFile(Type saveFileClass) => chosenSaveFile.IsSet && saveFileClass.FullName == chosenSaveFile.ClassFullName;
    }
}
