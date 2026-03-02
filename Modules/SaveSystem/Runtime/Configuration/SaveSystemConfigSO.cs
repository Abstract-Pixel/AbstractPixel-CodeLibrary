using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    [CreateAssetMenu(fileName = "SaveSystemConfigSO", menuName = "Utility/SaveSystemConfigSO")]
    public class SaveSystemConfigSO : ScriptableObject
    {
        [field: SerializeField] public bool UseDebugPath { get; private set; } = false;
        [field: SerializeField] public int GameProfileGuidLength { get; private set; } = 6;
        [field: SerializeField] public SaveStartupPolicy StartupPolicy { get; private set; } = SaveStartupPolicy.Manual;
        [field: SerializeField] public FileExtension PrimaryFileExtension { get; private set; } = FileExtension.json;
        public FileExtension BackupFileExtension { get; private set; } = FileExtension.bak;

        [SerializeField] List<string> ignoredScenes = new List<string>();
        [SerializeField] List<SaveCatgeoryDefinition> categoryDefinitionList;

        Dictionary<SaveCategory, SaveCatgeoryDefinition> categoryDefinitionMap = new();

        private void OnValidate()
        {
            for (int i = 0; i < categoryDefinitionList.Count; i++)
            {
                string nameToDisplay = categoryDefinitionList[i].Category.ToString();
                categoryDefinitionList[i].DisplayName = nameToDisplay;
            }
        }

        public void Initialize()
        {
            categoryDefinitionMap = new Dictionary<SaveCategory, SaveCatgeoryDefinition>();
            foreach (SaveCatgeoryDefinition definition in categoryDefinitionList)
            {
                if (!categoryDefinitionMap.ContainsKey(definition.Category))
                {
                    categoryDefinitionMap.Add(definition.Category, definition);
                }
                else
                {
                    Debug.LogWarning($"[SaveSystemConfigSO] - Duplicate category definition found for category: {definition.Category}");
                }
            }

        }

        public SaveCatgeoryDefinition GetCategoryDefinition(SaveCategory category)
        {
            if (categoryDefinitionMap.TryGetValue(category, out SaveCatgeoryDefinition definition))
            {
                return definition;
            }
            Debug.LogWarning($"[SaveSystemConfigSO] - No definition found for category: {category}");
            return null;
        }

        public IReadOnlyList<SaveCatgeoryDefinition> GetAllCategoryDefinitions()
        {
            return categoryDefinitionList;
        }

        public bool IsSceneIgnored(string sceneName)
        {
            return ignoredScenes.Contains(sceneName);
        }
    }
}
