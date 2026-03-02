using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ScriptBatchCreation : EditorWindow
{
    public enum ScriptTemplateType
    {
        PlainClass,
        MonoBehaviour,
        ScriptableObject,
        Interface,
        Custom,
        CompleteCustom // New template option
    }

    public enum ScriptKind
    {
        Class,
        Interface,
        ScriptableObject
    }

    [System.Serializable]
    public class ScriptEntry
    {
        public string scriptName = "NewScript";
        public ScriptTemplateType templateType = ScriptTemplateType.MonoBehaviour;

        // Fields for Custom Template
        public ScriptKind scriptKind = ScriptKind.Class;
        public string inheritsFrom = "";

        // Field for CompleteCustom Template
        [TextArea(5, 10)]
        public string customCode = ""; // New field for custom code input
    }

    [System.Serializable]
    public class ScriptBatch
    {
        public DefaultAsset folder; // Reference to the folder
        public List<ScriptEntry> scripts = new List<ScriptEntry>(); // List of script entries
    }

    // List to hold multiple script batches
    public List<ScriptBatch> scriptBatches = new List<ScriptBatch>();

    // Custom GUIStyle for script labels
    private GUIStyle scriptLabelStyle;

    private Vector2 scrollPos;

    // Add menu item to open the window
    [MenuItem("Tools/Script Batch Creation")]
    public static void ShowWindow()
    {
        GetWindow<ScriptBatchCreation>("Script Batch Creation");
    }

    private void OnGUI()
    {
        // Initialize the custom style if it's null
        if (scriptLabelStyle == null)
        {
            scriptLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.grey }
            };
        }

        GUILayout.Label("Script Batch Creation", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // Iterate over each script batch
        for (int i = 0; i < scriptBatches.Count; i++)
        {
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical("box");
            ScriptBatch batch = scriptBatches[i];

            // Subheading for the script batch
            GUILayout.Label($"Script Batch {i + 1}", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            batch.folder = (DefaultAsset)EditorGUILayout.ObjectField("Folder", batch.folder, typeof(DefaultAsset), false);

            if (GUILayout.Button("Remove Batch"))
            {
                scriptBatches.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            // Divider after folder selection
            GUILayout.Space(5);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Space(2);

            // "Scripts to Create" heading
            GUILayout.Space(10);
            GUILayout.Label("Scripts to Create:", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // Iterate over each script entry
            for (int j = 0; j < batch.scripts.Count; j++)
            {
                EditorGUILayout.BeginVertical("box");

                // Divider between scripts
                if (j > 0)
                {
                    GUIStyle dividerStyle = new GUIStyle();
                    dividerStyle.normal.background = EditorGUIUtility.whiteTexture;
                    dividerStyle.margin = new RectOffset(0, 0, 4, 4);
                    dividerStyle.fixedHeight = 3;
                    Color originalColor = GUI.color;
                    GUI.color = Color.grey;
                    GUILayout.Box(GUIContent.none, dividerStyle, GUILayout.ExpandWidth(true));
                    GUI.color = originalColor;
                    GUILayout.Space(2);
                }

                // Label for each script
                GUILayout.Label("Script " + (j + 1).ToString(), scriptLabelStyle);

                EditorGUILayout.BeginHorizontal();

                // Script Name Field
                batch.scripts[j].scriptName = EditorGUILayout.TextField("Script Name", batch.scripts[j].scriptName);

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                // "Template Type" label
                GUILayout.Label("Template Type", GUILayout.Width(100));

                // Template Type Dropdown
                batch.scripts[j].templateType = (ScriptTemplateType)EditorGUILayout.EnumPopup(
                    batch.scripts[j].templateType, GUILayout.Width(150));

                // Remove Script Button
                if (GUILayout.Button("Remove Script"))
                {
                    batch.scripts.RemoveAt(j);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                // Additional Fields for Custom Templates
                if (batch.scripts[j].templateType == ScriptTemplateType.Custom)
                {
                    EditorGUILayout.Space();

                    // Script Kind Dropdown
                    batch.scripts[j].scriptKind = (ScriptKind)EditorGUILayout.EnumPopup("Script Type",
                        batch.scripts[j].scriptKind);

                    // Inherits From Field
                    batch.scripts[j].inheritsFrom = EditorGUILayout.TextField("Inherits From",
                        batch.scripts[j].inheritsFrom);

                    EditorGUILayout.Space();
                }
                else if (batch.scripts[j].templateType == ScriptTemplateType.CompleteCustom)
                {
                    EditorGUILayout.Space();

                    GUILayout.Label("Custom Code:");
                    batch.scripts[j].customCode = EditorGUILayout.TextArea(batch.scripts[j].customCode, GUILayout.Height(100));

                    EditorGUILayout.Space();
                }

                EditorGUILayout.EndVertical();
            }

            // Adjust Add Script button to be smaller and aligned to the right
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Script", GUILayout.Width(80))) // Adjust width to make it smaller
            {
                batch.scripts.Add(new ScriptEntry());

            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            // Add a dashed line after each batch
            // Create a label with a dashed line
            GUIStyle lineStyle = new GUIStyle(GUI.skin.label);
            lineStyle.alignment = TextAnchor.MiddleCenter;
            lineStyle.fontStyle = FontStyle.Bold;
            lineStyle.fontSize = 30;
            lineStyle.normal.textColor = new Color(0.66f,0.198f,0.166f); // You can change the color if desired
            GUILayout.Label("-----------------------------------------------------------------------------------------------------------", lineStyle);
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add Script Batch"))
        {
            scriptBatches.Add(new ScriptBatch());
            scriptBatches[ scriptBatches.Count-1].scripts.Add(new ScriptEntry());
        }
                                                                                                                    
        if (GUILayout.Button("Create Scripts"))
        {
            CreateScripts();
        }
    }

    private void CreateScripts()
    {
        foreach (var batch in scriptBatches)
        {
            if (batch.folder == null)
            {
                Debug.LogWarning("No folder assigned for a script batch. Skipping this batch.");
                continue;
            }

            string folderPath = AssetDatabase.GetAssetPath(batch.folder);

            foreach (var scriptEntry in batch.scripts)
            {
                string scriptName = scriptEntry.scriptName;

                // Validate script name
                if (string.IsNullOrEmpty(scriptName))
                {
                    Debug.LogWarning("Script name is empty. Skipping this script.");
                    continue;
                }
                if (!IsValidScriptName(scriptName))
                {
                    Debug.LogWarning($"Invalid script name '{scriptName}'. Skipping this script.");
                    continue;
                }

                string scriptPath = Path.Combine(folderPath, scriptName + ".cs");

                if (File.Exists(scriptPath))
                {
                    Debug.LogWarning($"Script '{scriptName}' already exists in '{folderPath}'. Skipping.");
                    continue;
                }

                string scriptContent = GenerateScriptContent(scriptEntry);
                File.WriteAllText(scriptPath, scriptContent);
                AssetDatabase.ImportAsset(scriptPath);
                Debug.Log($"Created script '{scriptName}' in '{folderPath}'.");
            }
        }

        AssetDatabase.Refresh();
    }

    private string GenerateScriptContent(ScriptEntry scriptEntry)
    {
        switch (scriptEntry.templateType)
        {
            case ScriptTemplateType.PlainClass:
                return
            $@"public class {scriptEntry.scriptName}
{{
    // Your code here
}}";

            case ScriptTemplateType.MonoBehaviour:
                return
            $@"using UnityEngine;

public class {scriptEntry.scriptName} : MonoBehaviour
{{
    void Start()
    {{

    }}

    void Update()
    {{

    }}
}}";

            case ScriptTemplateType.ScriptableObject:
                return
            $@"using UnityEngine;

[CreateAssetMenu(fileName = ""{scriptEntry.scriptName}"", menuName = ""ScriptableObjects/{scriptEntry.scriptName}"", order = 1)]
public class {scriptEntry.scriptName} : ScriptableObject
{{
    // Your code here
}}";

            case ScriptTemplateType.Interface:
                return
            $@"public interface {scriptEntry.scriptName}
{{
    // Define interface methods and properties here
}}";

            case ScriptTemplateType.Custom:
                return GenerateCustomScriptContent(scriptEntry);

            case ScriptTemplateType.CompleteCustom:
                return GenerateCompleteCustomScriptContent(scriptEntry);

            default:
                return string.Empty;
        }
    }

    private string GenerateCustomScriptContent(ScriptEntry scriptEntry)
    {
        string scriptName = scriptEntry.scriptName;
        ScriptKind scriptKind = scriptEntry.scriptKind;
        string inheritsFrom = scriptEntry.inheritsFrom;

        string inheritanceClause = "";

        if (!string.IsNullOrEmpty(inheritsFrom))
        {
            inheritanceClause = $" : {inheritsFrom}";
        }

        switch (scriptKind)
        {
            case ScriptKind.Class:
                return
            $@"public class {scriptName}{inheritanceClause}
{{
    // Your code here
}}";

            case ScriptKind.Interface:
                return
            $@"public interface {scriptName}{inheritanceClause}
{{
    // Define interface methods and properties here
}}";

            case ScriptKind.ScriptableObject:
                return
            $@"using UnityEngine;

[CreateAssetMenu(fileName = ""{scriptName}"", menuName = ""ScriptableObjects/{scriptName}"", order = 1)]
public class {scriptName}{inheritanceClause}
{{
    // Your code here
}}";

            default:
                return string.Empty;
        }
    }

    private string GenerateCompleteCustomScriptContent(ScriptEntry scriptEntry)
    {
        // Use the custom code provided without modifications
        return scriptEntry.customCode;
    }

    private bool IsValidScriptName(string scriptName)
    {
        // Simple validation to check if the script name starts with a letter or underscore
        if (string.IsNullOrEmpty(scriptName))
            return false;

        if (!char.IsLetter(scriptName[0]) && scriptName[0] != '_')
            return false;

        // Further validation can be added here if necessary

        return true;
    }
}
