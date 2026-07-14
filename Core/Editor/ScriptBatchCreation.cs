using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class ScriptBatchCreation : EditorWindow
{
    public enum ScriptTemplateType
    {
        PlainClass,
        MonoBehaviour,
        ScriptableObject,
        Interface,
        Enum,
        Struct,
        AbstractClass,
        Custom,
        CompleteCustom
    }

    public enum ScriptKind
    {
        Class,
        Interface,
        ScriptableObject,
        Enum,
        Struct,
        AbstractClass
    }

    [System.Serializable]
    public class ScriptEntry
    {
        public string scriptName = "NewScript";
        public ScriptTemplateType templateType = ScriptTemplateType.MonoBehaviour;

        public ScriptKind scriptKind = ScriptKind.Class;
        public string inheritsFrom = "";
        public bool isStatic = false;

        public int selectedNamespaceIndex = 0; // Evaluated PER SCRIPT
        public string customCode = "";
        public Vector2 codeScrollPos;
    }

    [System.Serializable]
    public class ScriptBatch
    {
        public DefaultAsset folder;
        public List<ScriptEntry> scripts = new List<ScriptEntry>();
    }

    public List<ScriptBatch> scriptBatches = new List<ScriptBatch>();

    private string[] availableNamespaces = new string[] { "None" };

    private GUIStyle titleStyle;
    private GUIStyle transparentCodeStyle;
    private GUIStyle previewStyle;
    private GUIStyle lineNumberStyle;
    private Texture2D clearTexture;
    private bool stylesInitialized = false;

    private Vector2 mainScrollPos;

    private readonly Color colorRemove = new Color(0.85f, 0.5f, 0.5f);
    private readonly Color colorAdd = new Color(0.55f, 0.75f, 0.55f);
    private readonly Color colorCreate = new Color(0.5f, 0.7f, 0.9f);
    private readonly Color colorPaste = new Color(0.7f, 0.6f, 0.9f);

    // Header Color
    private readonly Color colorHeader = new Color(0.18f, 0.18f, 0.18f, 1f);

    [MenuItem("Tools/Script Batch Creation")]
    public static void ShowWindow()
    {
        GetWindow<ScriptBatchCreation>("Script Batch Creation");
    }

    private void OnEnable()
    {
        LoadAssemblies();

        if (scriptBatches.Count == 0)
        {
            AddAutoDetectedBatch();
        }
    }

    private void LoadAssemblies()
    {
        string[] guids = AssetDatabase.FindAssets("t:asmdef");
        List<string> nsList = new List<string>() { "None" };

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string content = File.ReadAllText(path);

            Match match = Regex.Match(content, @"""rootNamespace""\s*:\s*""([^""]+)""");
            string ns = match.Success ? match.Groups[1].Value : "";

            if (string.IsNullOrWhiteSpace(ns))
            {
                match = Regex.Match(content, @"""name""\s*:\s*""([^""]+)""");
                if (match.Success) ns = match.Groups[1].Value.Replace(" ", "").Replace("-", "_");
            }

            if (!string.IsNullOrWhiteSpace(ns) && !nsList.Contains(ns))
            {
                nsList.Add(ns);
            }
        }

        foreach (var existing in availableNamespaces)
        {
            if (!nsList.Contains(existing)) nsList.Add(existing);
        }

        availableNamespaces = nsList.ToArray();
    }

    private void InitStyles()
    {
        if (clearTexture == null)
        {
            clearTexture = new Texture2D(1, 1);
            clearTexture.SetPixel(0, 0, Color.clear);
            clearTexture.Apply();
        }

        titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter,
            margin = new RectOffset(0, 0, 10, 10)
        };

        Font monoFont = Font.CreateDynamicFontFromOSFont(new string[] { "Consolas", "Courier New", "Monaco", "Lucida Console" }, 13);
        if (monoFont == null) monoFont = GUI.skin.font;

        transparentCodeStyle = new GUIStyle(EditorStyles.textArea)
        {
            font = monoFont,
            richText = false,
            wordWrap = false
        };
        transparentCodeStyle.normal.textColor = Color.clear;
        transparentCodeStyle.active.textColor = Color.clear;
        transparentCodeStyle.hover.textColor = Color.clear;
        transparentCodeStyle.focused.textColor = Color.clear;
        transparentCodeStyle.normal.background = clearTexture;
        transparentCodeStyle.active.background = clearTexture;
        transparentCodeStyle.hover.background = clearTexture;
        transparentCodeStyle.focused.background = clearTexture;

        previewStyle = new GUIStyle(EditorStyles.textArea)
        {
            font = monoFont,
            richText = true,
            wordWrap = false
        };
        previewStyle.normal.background = clearTexture;
        previewStyle.active.background = clearTexture;
        previewStyle.hover.background = clearTexture;
        previewStyle.focused.background = clearTexture;

        lineNumberStyle = new GUIStyle(EditorStyles.label)
        {
            font = monoFont,
            alignment = TextAnchor.UpperRight,
            padding = new RectOffset(5, 5, 5, 5)
        };
        lineNumberStyle.normal.textColor = new Color(0.4f, 0.4f, 0.4f);

        stylesInitialized = true;
    }

    private void OnGUI()
    {
        if (!stylesInitialized || titleStyle == null) InitStyles();
        if (availableNamespaces == null || availableNamespaces.Length == 0) LoadAssemblies();

        GUILayout.Label("Script Batch Creation", titleStyle);
        DrawHorizontalLine(2);
        GUILayout.Space(10);

        mainScrollPos = EditorGUILayout.BeginScrollView(mainScrollPos);

        for (int i = 0; i < scriptBatches.Count; i++)
        {
            ScriptBatch batch = scriptBatches[i];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);

            // --- BATCH HEADER ---
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"📁 Batch {i + 1} Folder:", EditorStyles.boldLabel, GUILayout.Width(110));

            batch.folder = (DefaultAsset)EditorGUILayout.ObjectField(batch.folder, typeof(DefaultAsset), false);

            GUI.backgroundColor = colorRemove;
            if (GUILayout.Button("Remove Batch", GUILayout.Width(100)))
            {
                scriptBatches.RemoveAt(i);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUIUtility.ExitGUI();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            DrawHorizontalLine(1);
            GUILayout.Space(10);

            // --- SCRIPTS ---
            for (int j = 0; j < batch.scripts.Count; j++)
            {
                ScriptEntry script = batch.scripts[j];

                // === OUTER CONTAINER (Curved HelpBox Border) ===
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Remove default GUI margin inside the HelpBox to ensure the background stretches fully
                Rect headerRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

                if (Event.current.type == EventType.Repaint)
                {
                    EditorGUI.DrawRect(headerRect, colorHeader);
                }

                GUILayout.Space(8); // Top Padding inside header

                // --- LINE 1 ---
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(8); // Left Indent

                // C# Icon
                GUILayout.Label(EditorGUIUtility.IconContent("cs Script Icon"), GUILayout.Width(20), GUILayout.Height(20));

                // Script Identifier
                GUILayout.Label($"Script {j + 1}", EditorStyles.boldLabel, GUILayout.Width(55), GUILayout.Height(20));

                // Script Name
                script.scriptName = EditorGUILayout.TextField(script.scriptName, GUILayout.Width(250));

                GUILayout.Space(10);

                // Template Dropdown
                script.templateType = (ScriptTemplateType)EditorGUILayout.EnumPopup(script.templateType, GUILayout.Width(135));

                // Static Checkbox
                bool showStatic = script.templateType == ScriptTemplateType.PlainClass ||
                                  (script.templateType == ScriptTemplateType.Custom && script.scriptKind == ScriptKind.Class);
                if (showStatic)
                {
                    GUILayout.Space(10);
                    script.isStatic = GUILayout.Toggle(script.isStatic, "Static", GUILayout.Width(55));
                }

                // Push Remove Button to the far right, eliminating gaps
                GUILayout.FlexibleSpace();

                // Remove Button
                GUI.backgroundColor = colorRemove;
                if (GUILayout.Button("X", GUILayout.Width(22), GUILayout.Height(20)))
                {
                    batch.scripts.RemoveAt(j);
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical(); // End header
                    EditorGUILayout.EndVertical(); // End HelpBox
                    GUIUtility.ExitGUI();
                }
                GUI.backgroundColor = Color.white;
                GUILayout.Space(8); // Right Indent
                EditorGUILayout.EndHorizontal(); // End Line 1

                GUILayout.Space(6); // Space between Line 1 and Line 2

                // --- LINE 2 ---
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(8); // Indent to align perfectly with the Logo above

                GUILayout.Label("Assembly definition:", GUILayout.ExpandWidth(false));

                // Dynamic Width Calculation for Dropdown
                string currentSelection = (script.selectedNamespaceIndex >= 0 && script.selectedNamespaceIndex < availableNamespaces.Length)
                    ? availableNamespaces[script.selectedNamespaceIndex]
                    : "";

                float dynamicDropdownWidth = EditorStyles.popup.CalcSize(new GUIContent(currentSelection)).x + 20f;
                float clampedWidth = Mathf.Clamp(dynamicDropdownWidth, 100f, 400f); // Prevents it from getting too small or too insanely large

                script.selectedNamespaceIndex = EditorGUILayout.Popup(script.selectedNamespaceIndex, availableNamespaces, GUILayout.Width(clampedWidth));

                // Custom Type properties appended if Custom is selected
                if (script.templateType == ScriptTemplateType.Custom)
                {
                    GUILayout.Space(20);
                    GUILayout.Label("Type:", GUILayout.ExpandWidth(false));
                    script.scriptKind = (ScriptKind)EditorGUILayout.EnumPopup(script.scriptKind, GUILayout.Width(100));

                    GUILayout.Space(10);
                    GUILayout.Label("Inherits:", GUILayout.ExpandWidth(false));
                    script.inheritsFrom = EditorGUILayout.TextField(script.inheritsFrom, GUILayout.Width(150));
                }

                GUILayout.FlexibleSpace(); // Fills remainder space to the right
                EditorGUILayout.EndHorizontal(); // End Line 2

                GUILayout.Space(10); // Bottom Padding inside header
                EditorGUILayout.EndVertical(); // End Unified Header Container

                // === DYNAMIC CODE EDITOR ===
                if (script.templateType == ScriptTemplateType.CompleteCustom)
                {
                    DrawDynamicCodeEditor(script);
                }

                EditorGUILayout.EndVertical(); // End Outer HelpBox Container
                GUILayout.Space(15); // Gap between different scripts
            }

            // --- ADD SCRIPT BUTTON ---
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = colorAdd;
            if (GUILayout.Button("+ Add Script", GUILayout.Width(100)))
            {
                var newScript = new ScriptEntry();
                if (batch.folder != null)
                {
                    newScript.selectedNamespaceIndex = FindClosestAsmdefIndex(AssetDatabase.GetAssetPath(batch.folder));
                }
                batch.scripts.Add(newScript);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUILayout.Space(15);
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);
        DrawHorizontalLine(2);
        GUILayout.Space(10);

        // --- BOTTOM BUTTONS ---
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Script Batch", GUILayout.Height(30)))
        {
            AddAutoDetectedBatch();
        }

        GUI.backgroundColor = colorPaste;
        if (GUILayout.Button("Paste from Clipboard", GUILayout.Height(30)))
        {
            PasteFromClipboard();
        }

        bool triggerCreation = false;
        GUI.backgroundColor = colorCreate;
        if (GUILayout.Button("Create Scripts", GUILayout.Height(30)))
        {
            triggerCreation = true;
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);

        if (triggerCreation)
        {
            CreateScripts();
            GUIUtility.ExitGUI();
        }
    }

    #region Smart UX Logic
    private void AddAutoDetectedBatch()
    {
        var newBatch = new ScriptBatch();
        newBatch.folder = GetSelectedFolder();

        var defaultScript = new ScriptEntry();
        if (newBatch.folder != null)
        {
            defaultScript.selectedNamespaceIndex = FindClosestAsmdefIndex(AssetDatabase.GetAssetPath(newBatch.folder));
        }

        newBatch.scripts.Add(defaultScript);
        scriptBatches.Add(newBatch);
    }

    private DefaultAsset GetSelectedFolder()
    {
        string path = "";

        if (Selection.assetGUIDs != null && Selection.assetGUIDs.Length > 0)
        {
            path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            if (!AssetDatabase.IsValidFolder(path))
            {
                path = Path.GetDirectoryName(path);
            }
        }

        if (string.IsNullOrEmpty(path)) path = "Assets";

        path = path.Replace("\\", "/");
        return AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);
    }

    private void PasteFromClipboard()
    {
        string clipboard = EditorGUIUtility.systemCopyBuffer;
        if (string.IsNullOrWhiteSpace(clipboard))
        {
            Debug.LogWarning("Clipboard is empty.");
            return;
        }

        ScriptBatch targetBatch = scriptBatches.Count > 0 ? scriptBatches[scriptBatches.Count - 1] : null;
        if (targetBatch == null)
        {
            targetBatch = new ScriptBatch();
            targetBatch.folder = GetSelectedFolder();
            scriptBatches.Add(targetBatch);
        }

        MatchCollection matches = Regex.Matches(clipboard, @"```(?:csharp|cs)?\s*\n(.*?)\n```", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        if (matches.Count > 0)
        {
            foreach (Match match in matches)
            {
                InjectCodeIntoBatch(targetBatch, match.Groups[1].Value.Trim());
            }
        }
        else
        {
            string extractedName = ExtractClassName(clipboard);
            if (!string.IsNullOrEmpty(extractedName))
            {
                InjectCodeIntoBatch(targetBatch, clipboard);
            }
            else
            {
                Debug.LogWarning("No valid C# scripts or markdown code blocks found in clipboard.");
                return;
            }
        }

        mainScrollPos = new Vector2(0, float.MaxValue);
    }

    private void InjectCodeIntoBatch(ScriptBatch batch, string code)
    {
        string extractedName = ExtractClassName(code);

        ScriptEntry targetEntry = null;
        if (batch.scripts.Count > 0)
        {
            var lastScript = batch.scripts[batch.scripts.Count - 1];
            if (lastScript.scriptName == "NewScript" && string.IsNullOrEmpty(lastScript.customCode))
            {
                targetEntry = lastScript;
            }
        }

        if (targetEntry == null)
        {
            targetEntry = new ScriptEntry();
            if (batch.folder != null)
            {
                targetEntry.selectedNamespaceIndex = FindClosestAsmdefIndex(AssetDatabase.GetAssetPath(batch.folder));
            }
            batch.scripts.Add(targetEntry);
        }

        targetEntry.templateType = ScriptTemplateType.CompleteCustom;
        targetEntry.customCode = code;
        targetEntry.scriptName = string.IsNullOrEmpty(extractedName) ? "NewScript" : extractedName;

        UpdateNamespaceFromCode(targetEntry, code);
    }

    private string ExtractClassName(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return "";
        Match match = Regex.Match(code, @"\b(?:class|struct|interface|enum)\s+([a-zA-Z_][a-zA-Z0-9_]*)\b");
        if (match.Success) return match.Groups[1].Value;
        return "";
    }

    private string ExtractNamespace(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return "";
        Match match = Regex.Match(code, @"\bnamespace\s+([a-zA-Z_][a-zA-Z0-9_.]*)");
        if (match.Success) return match.Groups[1].Value;
        return "";
    }

    private void UpdateNamespaceFromCode(ScriptEntry entry, string code)
    {
        string extractedNs = ExtractNamespace(code);
        if (!string.IsNullOrEmpty(extractedNs))
        {
            int index = System.Array.IndexOf(availableNamespaces, extractedNs);
            if (index != -1)
            {
                entry.selectedNamespaceIndex = index;
            }
            else
            {
                List<string> nsList = new List<string>(availableNamespaces) { extractedNs };
                availableNamespaces = nsList.ToArray();
                entry.selectedNamespaceIndex = availableNamespaces.Length - 1;
            }
        }
    }
    #endregion

    private int FindClosestAsmdefIndex(string folderPath)
    {
        string currentPath = folderPath;

        while (!string.IsNullOrEmpty(currentPath) && currentPath.StartsWith("Assets"))
        {
            string[] asmdefs = Directory.GetFiles(currentPath, "*.asmdef");
            if (asmdefs.Length > 0)
            {
                string content = File.ReadAllText(asmdefs[0]);

                Match match = Regex.Match(content, @"""rootNamespace""\s*:\s*""([^""]+)""");
                string ns = match.Success ? match.Groups[1].Value : "";

                if (string.IsNullOrWhiteSpace(ns))
                {
                    match = Regex.Match(content, @"""name""\s*:\s*""([^""]+)""");
                    if (match.Success) ns = match.Groups[1].Value.Replace(" ", "").Replace("-", "_");
                }

                for (int i = 0; i < availableNamespaces.Length; i++)
                {
                    if (availableNamespaces[i] == ns) return i;
                }
            }

            int lastSlash = currentPath.LastIndexOf('/');
            if (lastSlash > 0) currentPath = currentPath.Substring(0, lastSlash);
            else break;
        }
        return 0; // Index 0 is "None"
    }

    private void DrawDynamicCodeEditor(ScriptEntry script)
    {
        // Container Background ensures it stretches out
        Rect containerRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

        if (Event.current.type == EventType.Repaint)
        {
            // Darker tone for code body
            EditorGUI.DrawRect(containerRect, new Color(0.11f, 0.11f, 0.11f, 1f));
        }

        script.codeScrollPos = EditorGUILayout.BeginScrollView(script.codeScrollPos, GUILayout.Height(200));
        EditorGUILayout.BeginHorizontal();

        if (script.customCode == null) script.customCode = "";

        // 1. Line Numbers
        string[] lines = script.customCode.Split('\n');
        string lineNumbers = "";
        for (int i = 1; i <= lines.Length; i++) lineNumbers += i + "\n";

        EditorGUILayout.BeginVertical(GUILayout.Width(35));
        GUILayout.Label(lineNumbers, lineNumberStyle);
        EditorGUILayout.EndVertical();

        // 2. Separator Line
        GUILayout.Box("", GUILayout.Width(1), GUILayout.ExpandHeight(true));

        // 3. Dynamic Code Area
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        GUIContent codeContent = new GUIContent(script.customCode + "\n");
        float calcHeight = transparentCodeStyle.CalcHeight(codeContent, position.width - 80);
        float minHeight = Mathf.Max(calcHeight, 180f);

        Rect textRect = GUILayoutUtility.GetRect(codeContent, transparentCodeStyle, GUILayout.ExpandWidth(true), GUILayout.Height(minHeight));

        if (Event.current.type == EventType.Repaint)
        {
            previewStyle.Draw(textRect, new GUIContent(ApplySyntaxHighlighting(script.customCode)), false, false, false, false);
        }

        EditorGUI.BeginChangeCheck();
        script.customCode = EditorGUI.TextArea(textRect, script.customCode, transparentCodeStyle);

        if (EditorGUI.EndChangeCheck())
        {
            string autoName = ExtractClassName(script.customCode);
            if (!string.IsNullOrEmpty(autoName))
            {
                script.scriptName = autoName;
            }

            UpdateNamespaceFromCode(script, script.customCode);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private string ApplySyntaxHighlighting(string code)
    {
        if (string.IsNullOrEmpty(code)) return "";

        code = code.Replace("<", "<\u200B");

        string pattern = @"(?<comment>//.*?$)|(?<string>"".*?"")|(?<keyword>\b(?:public|private|protected|internal|class|struct|enum|interface|abstract|virtual|override|new|static|readonly|void|int|float|string|bool|double|long|using|namespace|return|if|else|for|foreach|while|do|switch|case|break|continue|default|get|set|true|false|null|this|base)\b)|(?<type>\b[A-Z][a-zA-Z0-9_]*\b)";

        code = Regex.Replace(code, pattern, m =>
        {
            if (m.Groups["comment"].Success) return $"<color=#57A64A>{m.Value}</color>";
            if (m.Groups["string"].Success) return $"<color=#D69D85>{m.Value}</color>";
            if (m.Groups["keyword"].Success) return $"<color=#569CD6>{m.Value}</color>";
            if (m.Groups["type"].Success) return $"<color=#4EC9B0>{m.Value}</color>";
            return m.Value;
        }, RegexOptions.Multiline);

        return code;
    }

    private void DrawHorizontalLine(int height = 1)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, height);
        rect.height = height;
        EditorGUI.DrawRect(rect, new Color(0.4f, 0.4f, 0.4f, 1));
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

                if (string.IsNullOrEmpty(scriptName) || !IsValidScriptName(scriptName))
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

                string targetNamespace = scriptEntry.selectedNamespaceIndex > 0 && scriptEntry.selectedNamespaceIndex < availableNamespaces.Length
                    ? availableNamespaces[scriptEntry.selectedNamespaceIndex]
                    : "";

                string scriptContent = GenerateScriptContent(scriptEntry);

                if (scriptEntry.templateType != ScriptTemplateType.CompleteCustom)
                {
                    scriptContent = WrapInNamespace(scriptContent, targetNamespace);
                }

                File.WriteAllText(scriptPath, scriptContent);
                Debug.Log($"Created script '{scriptName}' in '{folderPath}'.");
            }
        }

        AssetDatabase.Refresh();
    }

    private string WrapInNamespace(string content, string namespaceName)
    {
        if (string.IsNullOrWhiteSpace(namespaceName) || namespaceName == "None") return content;

        StringReader reader = new StringReader(content);
        string line;
        string usings = "";
        string rest = "";

        while ((line = reader.ReadLine()) != null)
        {
            if (line.TrimStart().StartsWith("using ") && !line.Contains("{") && !line.Contains("namespace"))
            {
                usings += line + "\n";
            }
            else
            {
                rest += line + "\n";
            }
        }

        if (!string.IsNullOrEmpty(usings)) usings += "\n";

        string indentedRest = "";
        StringReader restReader = new StringReader(rest.Trim('\n', '\r'));
        while ((line = restReader.ReadLine()) != null)
        {
            if (string.IsNullOrEmpty(line))
            {
                indentedRest += "\n";
            }
            else
            {
                indentedRest += "    " + line + "\n";
            }
        }

        return $"{usings}namespace {namespaceName}\n{{\n{indentedRest}}}";
    }

    private string GenerateScriptContent(ScriptEntry scriptEntry)
    {
        string staticKeyword = scriptEntry.isStatic ? "static " : "";

        switch (scriptEntry.templateType)
        {
            case ScriptTemplateType.PlainClass:
                return $@"public {staticKeyword}class {scriptEntry.scriptName}
{{
    // Your code here
}}";

            case ScriptTemplateType.MonoBehaviour:
                return $@"using UnityEngine;

public class {scriptEntry.scriptName} : MonoBehaviour
{{
    private void Start()
    {{

    }}

    private void Update()
    {{

    }}
}}";

            case ScriptTemplateType.ScriptableObject:
                return $@"using UnityEngine;

[CreateAssetMenu(fileName = ""{scriptEntry.scriptName}"", menuName = ""ScriptableObjects/{scriptEntry.scriptName}"", order = 1)]
public class {scriptEntry.scriptName} : ScriptableObject
{{
    // Your code here
}}";

            case ScriptTemplateType.Interface:
                return $@"public interface {scriptEntry.scriptName}
{{
    // Define interface methods and properties here
}}";

            case ScriptTemplateType.Enum:
                return $@"public enum {scriptEntry.scriptName}
{{
    None,
    // Add values here
}}";

            case ScriptTemplateType.Struct:
                return $@"using UnityEngine;

[System.Serializable]
public struct {scriptEntry.scriptName}
{{
    // Add fields here
}}";

            case ScriptTemplateType.AbstractClass:
                return $@"public abstract class {scriptEntry.scriptName}
{{
    // Add abstract methods and properties here
}}";

            case ScriptTemplateType.Custom:
                return GenerateCustomScriptContent(scriptEntry);

            case ScriptTemplateType.CompleteCustom:
                return scriptEntry.customCode;

            default:
                return string.Empty;
        }
    }

    private string GenerateCustomScriptContent(ScriptEntry scriptEntry)
    {
        string scriptName = scriptEntry.scriptName;
        string inheritanceClause = string.IsNullOrEmpty(scriptEntry.inheritsFrom) ? "" : $" : {scriptEntry.inheritsFrom}";

        string staticKeyword = scriptEntry.isStatic ? "static " : "";

        switch (scriptEntry.scriptKind)
        {
            case ScriptKind.Class:
                return $@"public {staticKeyword}class {scriptName}{inheritanceClause}
{{
    // Your code here
}}";

            case ScriptKind.Interface:
                return $@"public interface {scriptName}{inheritanceClause}
{{
    // Define interface methods and properties here
}}";

            case ScriptKind.ScriptableObject:
                return $@"using UnityEngine;

[CreateAssetMenu(fileName = ""{scriptName}"", menuName = ""ScriptableObjects/{scriptName}"", order = 1)]
public class {scriptName}{inheritanceClause}
{{
    // Your code here
}}";

            case ScriptKind.Enum:
                return $@"public enum {scriptName}
{{
    None,
    // Add values here
}}";

            case ScriptKind.Struct:
                return $@"using UnityEngine;

[System.Serializable]
public struct {scriptName}
{{
    // Add fields here
}}";

            case ScriptKind.AbstractClass:
                return $@"public abstract class {scriptName}{inheritanceClause}
{{
    // Add abstract methods and properties here
}}";

            default:
                return string.Empty;
        }
    }

    private bool IsValidScriptName(string scriptName)
    {
        if (string.IsNullOrEmpty(scriptName)) return false;
        if (!char.IsLetter(scriptName[0]) && scriptName[0] != '_') return false;
        return true;
    }
}