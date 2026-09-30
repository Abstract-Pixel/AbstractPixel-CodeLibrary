using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScreenshotTool.Editor
{
    public sealed class ScreenshotToolWindow : EditorWindow
    {
        private const string PREF_KEY_SAVE_PATH = "ScreenshotTool_SavePath";
        private const string PREF_KEY_INTERVAL = "ScreenshotTool_Interval";
        private const string PREF_KEY_ASPECT = "ScreenshotTool_Aspect";
        private const string PREF_KEY_RESOLUTION = "ScreenshotTool_Resolution";
        private const string PREF_KEY_FORMAT = "ScreenshotTool_Format";
        private const string PREF_KEY_CUSTOM_WIDTH = "ScreenshotTool_CustomWidth";
        private const string PREF_KEY_CUSTOM_HEIGHT = "ScreenshotTool_CustomHeight";

        private readonly Color COLOR_ACCENT_BLUE = new Color(0.35f, 0.75f, 1f);
        private readonly Color COLOR_DISABLED_GRAY = new Color(0.28f, 0.28f, 0.28f);
        private readonly Color COLOR_START_GREEN = new Color(0.18f, 0.55f, 0.34f);
        private readonly Color COLOR_STOP_RED = new Color(0.75f, 0.22f, 0.17f);
        private readonly Color COLOR_UNLOCK_ORANGE = new Color(1f, 0.65f, 0.2f);

        private static ScreenshotCaptureService staticCaptureService;
        private static ScreenshotSessionTracker staticSessionTracker;

        private TextField directoryPathTextField;
        private Button lockToggleButton;
        private Button openDirectoryButton;
        private EnumField aspectRatioEnumField;
        private EnumField resolutionTierEnumField;
        private EnumField fileFormatEnumField;
        private Slider intervalSlider;
        private IntegerField customWidthField;
        private IntegerField customHeightField;
        private Label resolutionPreviewLabel;
        private Label statusStateLabel;
        private Button toggleCaptureButton;

        private ScreenshotSessionView sessionView;
        private bool isPathUnlockedForEditing;

        [MenuItem("Tools/Screenshot Deck")]
        public static void OpenWindow()
        {
            ScreenshotToolWindow toolWindow = GetWindow<ScreenshotToolWindow>("Screenshot Deck");
            toolWindow.minSize = new Vector2(280f, 400f);
            toolWindow.Show();
        }

        private void OnEnable()
        {
            InitializeStaticServices();

            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;

            EditorApplication.quitting -= HandleEditorQuitting;
            EditorApplication.quitting += HandleEditorQuitting;

            PrefabStage.prefabStageOpened -= HandlePrefabStageOpened;
            PrefabStage.prefabStageOpened += HandlePrefabStageOpened;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.quitting -= HandleEditorQuitting;
            PrefabStage.prefabStageOpened -= HandlePrefabStageOpened;

            if (!EditorApplication.isPlaying)
            {
                HaltCaptureRoutine();
            }
        }

        private void OnLostFocus()
        {
            if (staticSessionTracker != null && staticSessionTracker.HasSessionData && !EditorApplication.isPlaying)
            {
                staticSessionTracker.CommitRenamesOnly();
            }
        }

        public void CreateGUI()
        {
            InitializeStaticServices();

            VisualElement rootContainer = rootVisualElement;
            rootContainer.style.paddingLeft = 14;
            rootContainer.style.paddingRight = 14;
            rootContainer.style.paddingTop = 14;
            rootContainer.style.paddingBottom = 14;

            ScrollView mainContentScrollView = new ScrollView();
            mainContentScrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            rootContainer.Add(mainContentScrollView);

            Label headerLabel = new Label("AUTOMATED SCREENSHOT DECK");
            headerLabel.style.fontSize = 16;
            headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            headerLabel.style.marginBottom = 12;
            mainContentScrollView.Add(headerLabel);

            VisualElement folderSection = new VisualElement();
            folderSection.style.marginBottom = 14;

            VisualElement folderHeaderRow = new VisualElement();
            folderHeaderRow.style.flexDirection = FlexDirection.Row;
            folderHeaderRow.style.justifyContent = Justify.SpaceBetween;
            folderHeaderRow.style.alignItems = Align.Center;
            folderHeaderRow.style.marginBottom = 4;

            Label folderHeaderLabel = new Label("Save Directory Path");
            folderHeaderLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            folderHeaderRow.Add(folderHeaderLabel);

            lockToggleButton = new Button(OnToggleLockClicked)
            {
                text = "🔒 Unlock to Edit"
            };
            lockToggleButton.style.height = 20;
            lockToggleButton.style.fontSize = 10;
            folderHeaderRow.Add(lockToggleButton);
            folderSection.Add(folderHeaderRow);

            string savedPath = EditorPrefs.GetString(PREF_KEY_SAVE_PATH, Path.Combine(Directory.GetCurrentDirectory(), "Screenshots"));

            directoryPathTextField = new TextField
            {
                value = savedPath,
                isReadOnly = true,
                multiline = true
            };
            directoryPathTextField.style.whiteSpace = WhiteSpace.Normal;
            directoryPathTextField.style.minHeight = 44;
            directoryPathTextField.style.marginBottom = 6;
            directoryPathTextField.RegisterValueChangedCallback(_ =>
            {
                ValidateSaveDirectoryPath();
                SaveUserPreferences();
            });
            folderSection.Add(directoryPathTextField);

            VisualElement folderButtonsRow = new VisualElement();
            folderButtonsRow.style.flexDirection = FlexDirection.Row;

            Button browseDirectoryButton = new Button(OnBrowseFolderClicked)
            {
                text = "Browse..."
            };
            browseDirectoryButton.style.flexGrow = 1;
            browseDirectoryButton.style.height = 28;
            browseDirectoryButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            browseDirectoryButton.style.marginRight = 6;
            folderButtonsRow.Add(browseDirectoryButton);

            openDirectoryButton = new Button(OnOpenDirectoryClicked)
            {
                text = "Open Directory"
            };
            openDirectoryButton.style.flexGrow = 1;
            openDirectoryButton.style.height = 28;
            openDirectoryButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            folderButtonsRow.Add(openDirectoryButton);

            folderSection.Add(folderButtonsRow);
            mainContentScrollView.Add(folderSection);

            ScreenshotAspectRatio savedAspect = (ScreenshotAspectRatio)EditorPrefs.GetInt(PREF_KEY_ASPECT, (int)ScreenshotAspectRatio.Landscape16x9);
            aspectRatioEnumField = new EnumField("Aspect Ratio", savedAspect);
            aspectRatioEnumField.RegisterValueChangedCallback(_ => RefreshResolutionPreview());
            mainContentScrollView.Add(aspectRatioEnumField);

            ScreenshotResolutionTier savedTier = (ScreenshotResolutionTier)EditorPrefs.GetInt(PREF_KEY_RESOLUTION, (int)ScreenshotResolutionTier.FullHD1080p);
            resolutionTierEnumField = new EnumField("Resolution Tier", savedTier);
            resolutionTierEnumField.RegisterValueChangedCallback(_ =>
            {
                UpdateCustomFieldsVisibility();
                RefreshResolutionPreview();
            });
            mainContentScrollView.Add(resolutionTierEnumField);

            int savedWidth = EditorPrefs.GetInt(PREF_KEY_CUSTOM_WIDTH, 1920);
            customWidthField = new IntegerField("Custom Width") { value = savedWidth };
            customWidthField.RegisterValueChangedCallback(_ => RefreshResolutionPreview());
            mainContentScrollView.Add(customWidthField);

            int savedHeight = EditorPrefs.GetInt(PREF_KEY_CUSTOM_HEIGHT, 1080);
            customHeightField = new IntegerField("Custom Height") { value = savedHeight };
            customHeightField.RegisterValueChangedCallback(_ => RefreshResolutionPreview());
            mainContentScrollView.Add(customHeightField);

            ScreenshotFileFormat savedFormat = (ScreenshotFileFormat)EditorPrefs.GetInt(PREF_KEY_FORMAT, (int)ScreenshotFileFormat.PNG);
            fileFormatEnumField = new EnumField("Image Format", savedFormat);
            mainContentScrollView.Add(fileFormatEnumField);

            float savedInterval = EditorPrefs.GetFloat(PREF_KEY_INTERVAL, 1.0f);
            intervalSlider = new Slider("Capture Interval (s)", 0.5f, 30.0f) { value = savedInterval, showInputField = true };
            mainContentScrollView.Add(intervalSlider);

            resolutionPreviewLabel = new Label("Target Dimensions: 1920 x 1080 px");
            resolutionPreviewLabel.style.marginTop = 10;
            resolutionPreviewLabel.style.marginBottom = 12;
            resolutionPreviewLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            resolutionPreviewLabel.style.color = COLOR_ACCENT_BLUE;
            mainContentScrollView.Add(resolutionPreviewLabel);

            statusStateLabel = new Label("Status: Ready (Enter Play Mode to capture)");
            statusStateLabel.style.marginBottom = 14;
            statusStateLabel.style.color = new Color(0.85f, 0.85f, 0.85f);
            mainContentScrollView.Add(statusStateLabel);

            toggleCaptureButton = new Button(OnToggleCaptureButtonClicked) { text = "START AUTOMATIC CAPTURE" };
            toggleCaptureButton.style.height = 44;
            toggleCaptureButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            toggleCaptureButton.style.backgroundColor = COLOR_START_GREEN;
            toggleCaptureButton.style.color = Color.white;
            mainContentScrollView.Add(toggleCaptureButton);

            sessionView = new ScreenshotSessionView();
            mainContentScrollView.Add(sessionView.RootElement);

            UpdateCustomFieldsVisibility();
            RefreshResolutionPreview();
            ValidateSaveDirectoryPath();

            if (staticSessionTracker != null && staticSessionTracker.HasSessionData && !EditorApplication.isPlaying)
            {
                sessionView.PopulateSession(staticSessionTracker, () => { });
            }
        }

        private void Update()
        {
            if (staticCaptureService != null && staticCaptureService.IsCapturing)
            {
                if (statusStateLabel != null)
                {
                    statusStateLabel.text = $"Status: Active | Saved: {staticCaptureService.SavedScreenshotCount} screenshots";
                }

                if (toggleCaptureButton != null && toggleCaptureButton.text != "STOP AUTOMATIC CAPTURE")
                {
                    toggleCaptureButton.text = "STOP AUTOMATIC CAPTURE";
                    toggleCaptureButton.style.backgroundColor = COLOR_STOP_RED;
                }
            }
            else
            {
                if (toggleCaptureButton != null && toggleCaptureButton.text != "START AUTOMATIC CAPTURE")
                {
                    toggleCaptureButton.text = "START AUTOMATIC CAPTURE";
                    toggleCaptureButton.style.backgroundColor = COLOR_START_GREEN;

                    if (statusStateLabel != null)
                    {
                        statusStateLabel.text = "Status: Stopped";
                    }
                }
            }
        }

        private void InitializeStaticServices()
        {
            if (staticSessionTracker == null)
            {
                staticSessionTracker = new ScreenshotSessionTracker();
            }

            if (staticCaptureService == null)
            {
                staticCaptureService = new ScreenshotCaptureService();
                staticCaptureService.OnScreenshotSaved += HandleScreenshotSavedStatic;
            }
        }

        private static void HandleScreenshotSavedStatic(string _filePath)
        {
            if (staticSessionTracker != null)
            {
                staticSessionTracker.RegisterCapturedFile(_filePath);
            }
        }

        private void OnToggleLockClicked()
        {
            isPathUnlockedForEditing = !isPathUnlockedForEditing;

            if (isPathUnlockedForEditing)
            {
                directoryPathTextField.isReadOnly = false;
                lockToggleButton.text = "🔓 Lock Path";
                lockToggleButton.style.color = COLOR_UNLOCK_ORANGE;
            }
            else
            {
                directoryPathTextField.isReadOnly = true;
                lockToggleButton.text = "🔒 Unlock to Edit";
                lockToggleButton.style.color = Color.white;
                SaveUserPreferences();
            }

            ValidateSaveDirectoryPath();
        }

        private void OnBrowseFolderClicked()
        {
            string chosenPath = EditorUtility.OpenFolderPanel("Select Screenshot Destination", directoryPathTextField.value, "");
            if (!string.IsNullOrEmpty(chosenPath))
            {
                directoryPathTextField.value = chosenPath;
                ValidateSaveDirectoryPath();
                SaveUserPreferences();
            }
        }

        private void OnOpenDirectoryClicked()
        {
            string currentPath = directoryPathTextField.value;
            if (!string.IsNullOrWhiteSpace(currentPath) && Directory.Exists(currentPath))
            {
                EditorUtility.RevealInFinder(currentPath);
            }
        }

        private bool ValidateSaveDirectoryPath()
        {
            string targetPath = directoryPathTextField.value;
            bool isPathValid = !string.IsNullOrWhiteSpace(targetPath) && Directory.Exists(targetPath);

            if (openDirectoryButton != null)
            {
                openDirectoryButton.SetEnabled(isPathValid);

                if (isPathValid)
                {
                    openDirectoryButton.style.backgroundColor = COLOR_ACCENT_BLUE;
                    openDirectoryButton.style.color = new Color(0.1f, 0.1f, 0.1f);
                }
                else
                {
                    openDirectoryButton.style.backgroundColor = COLOR_DISABLED_GRAY;
                    openDirectoryButton.style.color = new Color(0.6f, 0.6f, 0.6f);
                }
            }

            return isPathValid;
        }

        private void UpdateCustomFieldsVisibility()
        {
            ScreenshotResolutionTier selectedTier = (ScreenshotResolutionTier)resolutionTierEnumField.value;
            DisplayStyle displayMode = selectedTier == ScreenshotResolutionTier.Custom ? DisplayStyle.Flex : DisplayStyle.None;
            customWidthField.style.display = displayMode;
            customHeightField.style.display = displayMode;
        }

        private void RefreshResolutionPreview()
        {
            ScreenshotConfiguration currentConfig = BuildConfigurationFromInterface();
            (int previewWidth, int previewHeight) = ScreenshotResolutionUtility.CalculateDimensions(currentConfig);
            resolutionPreviewLabel.text = $"Target Dimensions: {previewWidth} x {previewHeight} px";
        }

        private void OnToggleCaptureButtonClicked()
        {
            InitializeStaticServices();

            if (staticCaptureService != null && staticCaptureService.IsCapturing)
            {
                HaltCaptureRoutine();
            }
            else
            {
                LaunchCaptureRoutine();
            }
        }

        private void LaunchCaptureRoutine()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Play Mode Required", "Please enter Play Mode before starting automatic background capture.", "OK");
                return;
            }

            if (!ValidateSaveDirectoryPath())
            {
                EditorUtility.DisplayDialog("Invalid Directory", "The specified folder path does not exist on your computer. Please select or create a valid directory.", "OK");
                return;
            }

            SaveUserPreferences();
            ScreenshotConfiguration config = BuildConfigurationFromInterface();

            InitializeStaticServices();
            staticCaptureService.StartCapture(config);

            if (sessionView != null)
            {
                sessionView.HideSession();
            }

            if (toggleCaptureButton != null)
            {
                toggleCaptureButton.text = "STOP AUTOMATIC CAPTURE";
                toggleCaptureButton.style.backgroundColor = COLOR_STOP_RED;
            }

            if (statusStateLabel != null)
            {
                statusStateLabel.text = "Status: Active | Recording frames...";
            }
        }

        private void HaltCaptureRoutine()
        {
            if (staticCaptureService != null)
            {
                staticCaptureService.StopCapture();
            }

            if (toggleCaptureButton != null)
            {
                toggleCaptureButton.text = "START AUTOMATIC CAPTURE";
                toggleCaptureButton.style.backgroundColor = COLOR_START_GREEN;
            }

            if (statusStateLabel != null)
            {
                statusStateLabel.text = "Status: Stopped";
            }
        }

        private void HandlePlayModeStateChanged(PlayModeStateChange _stateChange)
        {
            InitializeStaticServices();

            if (_stateChange == PlayModeStateChange.ExitingEditMode)
            {
                if (staticSessionTracker != null)
                {
                    staticSessionTracker.FinalizeAndPurgeUnmarked();
                    staticSessionTracker.ClearSession();
                }

                if (sessionView != null)
                {
                    sessionView.HideSession();
                }

                if (staticCaptureService != null)
                {
                    staticCaptureService.ResetSessionFlag();
                }
            }
            else if (_stateChange == PlayModeStateChange.ExitingPlayMode)
            {
                HaltCaptureRoutine();
            }
            else if (_stateChange == PlayModeStateChange.EnteredEditMode)
            {
                if (staticSessionTracker != null && staticSessionTracker.HasSessionData)
                {
                    Focus();

                    if (sessionView != null)
                    {
                        sessionView.PopulateSession(staticSessionTracker, () => { });
                    }
                }
                else
                {
                    if (sessionView != null)
                    {
                        sessionView.HideSession();
                    }
                }
            }
        }

        private void HandlePrefabStageOpened(PrefabStage _stage)
        {
            if (!EditorApplication.isPlaying && staticSessionTracker != null && staticSessionTracker.HasSessionData)
            {
                staticSessionTracker.FinalizeAndPurgeUnmarked();
                staticSessionTracker.ClearSession();

                if (sessionView != null)
                {
                    sessionView.HideSession();
                }
            }
        }

        private void HandleEditorQuitting()
        {
            if (staticSessionTracker != null)
            {
                staticSessionTracker.FinalizeAndPurgeUnmarked();
            }
        }

        private ScreenshotConfiguration BuildConfigurationFromInterface()
        {
            return new ScreenshotConfiguration(
                directoryPathTextField.value,
                intervalSlider.value,
                (ScreenshotAspectRatio)aspectRatioEnumField.value,
                (ScreenshotResolutionTier)resolutionTierEnumField.value,
                (ScreenshotFileFormat)fileFormatEnumField.value,
                customWidthField.value,
                customHeightField.value);
        }

        private void SaveUserPreferences()
        {
            EditorPrefs.SetString(PREF_KEY_SAVE_PATH, directoryPathTextField.value);
            EditorPrefs.SetFloat(PREF_KEY_INTERVAL, intervalSlider.value);
            EditorPrefs.SetInt(PREF_KEY_ASPECT, (int)(ScreenshotAspectRatio)aspectRatioEnumField.value);
            EditorPrefs.SetInt(PREF_KEY_RESOLUTION, (int)(ScreenshotResolutionTier)resolutionTierEnumField.value);
            EditorPrefs.SetInt(PREF_KEY_FORMAT, (int)(ScreenshotFileFormat)fileFormatEnumField.value);
            EditorPrefs.SetInt(PREF_KEY_CUSTOM_WIDTH, customWidthField.value);
            EditorPrefs.SetInt(PREF_KEY_CUSTOM_HEIGHT, customHeightField.value);
        }
    }
}