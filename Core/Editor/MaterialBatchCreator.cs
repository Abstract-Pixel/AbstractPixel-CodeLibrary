using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class MaterialBatchCreator : EditorWindow
{
    private List<MaterialBatch> materialBatches = new List<MaterialBatch>();
    private DefaultAsset selectedFolder;
    private Shader defaultShader;
    private Vector2 scrollPosition; // Add scroll position variable

    [System.Serializable]
    private class MaterialBatch
    {
        public string batchName = "Batch"; // Not used in UI currently, can be added for clarity later
        public DefaultAsset folderAsset;
        public List<MaterialData> materials = new List<MaterialData>();
    }

    [System.Serializable]
    private class MaterialData
    {
        public string materialName = "NewMaterial";
        public Color baseColor = Color.white;
        public bool emissionEnabled = false;
        public Color emissionColor = Color.black;
    }

    [MenuItem("Tools/Material Batch Creator")]
    public static void ShowWindow()
    {
        GetWindow<MaterialBatchCreator>("Material Batch Creator");
    }

    private void OnEnable()
    {
        if (materialBatches.Count == 0)
        {
            AddNewBatch();
        }
        defaultShader = Shader.Find("Universal Render Pipeline/Lit"); // Default to HDRP Lit Shader
        if (defaultShader == null)
        {
            defaultShader = Shader.Find("Universal Render Pipeline/Lit"); // Fallback to Standard if HDRP not found
        }
        scrollPosition = Vector2.zero; // Initialize scroll position
    }

    private void OnGUI()
    {
        // Styles
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 20 };
        GUIStyle subHeaderStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };
        GUIStyle boldGreyedLabelStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.grey } };
        GUIStyle smallButtonStyle = new GUIStyle(GUI.skin.button) { padding = new RectOffset(2, 2, 2, 2), fixedWidth = 24 };
        GUIStyle lightRedButtonStyle = new GUIStyle(GUI.skin.button) { normal = { textColor = Color.white }, hover = { textColor = Color.red }, active = { textColor = Color.red } };
        GUIStyle centeredButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition); // Begin ScrollView

        // 1. Material Batch Creation Header
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Material Batch Creation", headerStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        // 2. Material Batches Sub Header
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Material Batches", subHeaderStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        // 3. Line Separator (dark ocean blue)
        DrawSeparator(Color.Lerp(Color.blue, Color.green, 0.5f), 6f); // Dark Ocean Blue

        GUILayout.Space(10);

        // 4. List of Material Batches
        for (int batchIndex = 0; batchIndex < materialBatches.Count; batchIndex++)
        {
            MaterialBatch batch = materialBatches[batchIndex];

            // Batch Separator - Big Bold Light Red
            if (batchIndex > 0)
            {
                GUILayout.Space(15);
                DrawBoldSeparator(Color.Lerp(Color.red, Color.white, 0.3f)); // Light Red
                GUILayout.Space(10);
            }

            EditorGUILayout.LabelField($"Batch No. {batchIndex + 1}", boldGreyedLabelStyle);

            // Folder Asset Selection
            batch.folderAsset = (DefaultAsset)EditorGUILayout.ObjectField("Folder Asset", batch.folderAsset, typeof(DefaultAsset), false);


            if (batch.folderAsset != null && AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(batch.folderAsset)))
            {
                // Material List
                for (int materialIndex = 0; materialIndex < batch.materials.Count; materialIndex++)
                {
                    MaterialData materialData = batch.materials[materialIndex];

                    GUILayout.BeginVertical("GroupBox"); // Group Box for each material

                    // Material Separator - Small Grey Line
                    if (materialIndex > 0)
                    {
                        DrawSeparator(Color.grey, 1);
                    }

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Material No.", boldGreyedLabelStyle, GUILayout.Width(70));
                    EditorGUILayout.LabelField($"{materialIndex + 1}", boldGreyedLabelStyle, GUILayout.Width(20));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), smallButtonStyle)) // Remove Material Button
                    {
                        batch.materials.RemoveAt(materialIndex);
                        break; // Break to avoid index out of bounds after removal
                    }
                    GUILayout.EndHorizontal();


                    EditorGUI.BeginDisabledGroup(true); // Grey out Shader Field
                    EditorGUILayout.ObjectField("Shader Used", defaultShader, typeof(Shader), false);
                    EditorGUI.EndDisabledGroup();

                    materialData.materialName = EditorGUILayout.TextField("Material Name", materialData.materialName);
                    materialData.baseColor = EditorGUILayout.ColorField("Base Color", materialData.baseColor);
                    materialData.emissionEnabled = EditorGUILayout.Toggle("Emission Enabled", materialData.emissionEnabled);
                    if (materialData.emissionEnabled)
                    {
                        materialData.emissionColor = EditorGUILayout.ColorField(
                                                                    new GUIContent("Emission Color", "[HDR]"),
                                                                    materialData.emissionColor,
                                                                    true,      // showEyeDropper: true
                                                                    true,     // showAlpha: true (Adjust if you don't need alpha)
                                                                    true      // hdr: true
 );
                    }

                    GUILayout.EndVertical(); // End GroupBox Material
                }
                if (GUILayout.Button("Add Material"))
                {
                    batch.materials.Add(new MaterialData());
                }
            }
            else if (batch.folderAsset != null)
            {
                EditorGUILayout.HelpBox("Selected asset is not a valid folder.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("Please select a folder asset.", MessageType.Info);
            }


        } // End Batch Loop

        GUILayout.Space(10);
        // Add Batch Button
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Batch"))
        {
            AddNewBatch();
        }
        if (materialBatches.Count>0)
        {
            if (GUILayout.Button("Remove Batch", lightRedButtonStyle))
            {

                materialBatches.RemoveAt(materialBatches.Count - 1);

            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        // 7. Create All Material Batches Button
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create All Material Batches", centeredButtonStyle, GUILayout.Width(200)))
        {
            CreateMaterials();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        // 8. Remove Batches Button
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Remove Batches", lightRedButtonStyle, GUILayout.Width(200)))
        {
            RemoveAllBatches();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.EndScrollView(); // End ScrollView
    }


    private void AddNewBatch()
    {
        materialBatches.Add(new MaterialBatch());
        materialBatches[materialBatches.Count - 1].materials.Add(new MaterialData()); // Add one material to the new batch by default
    }

    private void RemoveAllBatches()
    {
        materialBatches.Clear();
        AddNewBatch(); // Reset to one default batch
    }

    private void CreateMaterials()
    {
        foreach (var batch in materialBatches)
        {
            if (batch.folderAsset == null) continue; // Skip if no folder selected

            string folderPath = AssetDatabase.GetAssetPath(batch.folderAsset);

            foreach (var materialData in batch.materials)
            {
                if (string.IsNullOrEmpty(materialData.materialName))
                {
                    Debug.LogWarning("Material Name cannot be empty. Skipping material creation.");
                    continue;
                }

                Material material = new Material(defaultShader);
                material.name = materialData.materialName;
                material.color = materialData.baseColor;

                if (materialData.emissionEnabled)
                {
                    material.EnableKeyword("_EMISSION");
                    material.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack; // Ensure emission contributes to GI
                    material.SetColor("_EmissionColor", materialData.emissionColor);
                }
                else
                {
                    material.DisableKeyword("_EMISSION");
                    material.globalIlluminationFlags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack; // Ensure no emission contribution to GI
                    material.SetColor("_EmissionColor", Color.black); // Set to black if emission is disabled
                }


                string assetPath = Path.Combine(folderPath, materialData.materialName + ".mat");
                assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath); // Ensure unique name

                AssetDatabase.CreateAsset(material, assetPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Material batches created successfully!");

        RemoveAllBatches(); // Reset UI to default state after creation
    }


    private void DrawSeparator(Color color, float thickness = 2f)
    {
        Rect rect = GUILayoutUtility.GetRect(1f, thickness);
        EditorGUI.DrawRect(rect, color);
    }

    private void DrawBoldSeparator(Color color)
    {
        Rect rect = GUILayoutUtility.GetRect(1f, 6f); // Thicker separator
        EditorGUI.DrawRect(rect, color);
    }
}