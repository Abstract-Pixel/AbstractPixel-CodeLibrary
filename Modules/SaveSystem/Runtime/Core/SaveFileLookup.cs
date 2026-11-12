using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Finds which save file a saved script uses, and that save file's definition in the config.
    /// Until a later step adds ISaveIn and the save file list, the save file comes from the script's
    /// [Savable(SaveCategory.X)] attribute. Later steps replace only this class.
    /// </summary>
    internal sealed class SaveFileLookup
    {
        readonly SaveSystemConfigSO config;

        public SaveFileLookup(SaveSystemConfigSO config)
        {
            this.config = config;
        }

        public IEnumerable<SaveCatgeoryDefinition> AllDefinitions => config.GetAllCategoryDefinitions() ?? Array.Empty<SaveCatgeoryDefinition>();

        /// <summary>The save file name, such as "Game", from the script's [Savable] attribute. Logs an error if it has none.</summary>
        public bool TryGetSaveFileName(Type scriptType, out string saveFileName)
        {
            SavableAttribute attribute = scriptType.GetCustomAttribute<SavableAttribute>();
            saveFileName = attribute?.Category.ToString();
            if (attribute == null)
            {
                Debug.LogError($"[SaveSystem] {scriptType.Name} can't be saved: it has no save file. Add [Savable(SaveCategory.X)] above the class to choose one.");
            }
            return attribute != null;
        }

        /// <summary>The config's definition for the save file. Logs an error if the config has none.</summary>
        public bool TryGetDefinition(string saveFileName, out SaveCatgeoryDefinition definition)
        {
            foreach (SaveCatgeoryDefinition candidate in AllDefinitions)
            {
                if (candidate.Category.ToString() == saveFileName)
                {
                    definition = candidate;
                    return true;
                }
            }

            definition = null;
            Debug.LogError($"[SaveSystem] The save file '{saveFileName}' isn't in the save system config, so its data can't be saved. Add an entry for '{saveFileName}' to the config's category list.");
            return false;
        }
    }
}
