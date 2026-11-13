using System;
using System.Linq;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Reads save files when a session, game profile or scene starts, records that they have loaded, and raises the
    /// load and save events on <see cref="SaveEventBus"/>. Every event here comes after the data it speaks of.
    /// </summary>
    internal sealed class SaveSessionEvents
    {
        readonly SaveFileLookup saveFiles;
        readonly LoadedSaveData loadedData;
        readonly SaveFileAddresses addresses;
        readonly ISceneInfo sceneInfo;
        readonly LoadEventLog loadEvents;
        readonly SaveFileClassFinder saveFileClasses = new SaveFileClassFinder();

        public SaveSessionEvents(SaveFileLookup saveFiles, LoadedSaveData loadedData, SaveFileAddresses addresses, ISceneInfo sceneInfo, LoadEventLog loadEvents)
        {
            this.saveFiles = saveFiles;
            this.loadedData = loadedData;
            this.addresses = addresses;
            this.sceneInfo = sceneInfo;
            this.loadEvents = loadEvents;
        }

        /// <summary>Notes a save file class seen in use, so its events can name it.</summary>
        public void Remember(Type saveFileClass)
        {
            saveFileClasses.Remember(saveFileClass);
        }

        /// <summary>Reads every save file in one save location and records that each has loaded.</summary>
        public void RecordLoads(SaveLocation location, string sceneName)
        {
            foreach (SaveFileDefinition definition in saveFiles.AllDefinitions.Where(definition => definition.Location == location))
            {
                loadedData.EnsureRead(addresses.AddressOf(definition, sceneName));
                loadEvents.RecordLoad(saveFileClasses.Find(definition), location, sceneName);
            }
        }

        // Every registered object is restored by now, so each GameProfile file, and each loaded scene's Scene files,
        // can say it has loaded. Then everything has.
        public void RecordGameProfileLoads()
        {
            loadEvents.ForgetGameProfileLoads();
            RecordLoads(SaveLocation.GameProfile, null);
            foreach (string sceneName in sceneInfo.LoadedSceneNames)
            {
                RecordLoads(SaveLocation.Scene, sceneName);
            }
            loadEvents.RecordLoadAll();
        }

        public void RaiseSaveCompleted(string fileName)
        {
            bool isKnownFile = saveFiles.TryGetDefinitionByFileName(fileName, out SaveFileDefinition definition);
            if (!isKnownFile)
            {
                return;
            }
            Type saveFileClass = saveFileClasses.Find(definition);
            if (saveFileClass != null)
            {
                SaveEventBus.RaiseSaveCompleted(saveFileClass);
            }
        }
    }
}
