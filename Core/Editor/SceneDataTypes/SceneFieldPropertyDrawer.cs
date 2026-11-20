using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// Draws a scene field: the scene picker, the scene's name (read only) and one status box saying whether the scene
    /// is In build, Addressable, Not in build or Missing, with one fix button when it is Not in build.
    /// UI Toolkit in &lt;see cref="CreatePropertyGUI"/&gt;; &lt;see cref="OnGUI"/&gt; is the IMGUI fallback with the same content.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer
    {
        private const string SceneAssetTooltip = "The scene asset. The field stores its GUID, path and name, so the scene is still found after it is moved or renamed.";
        private const string SceneNameTooltip = "The scene's name, copied from the scene asset. A player build reads this name and the stored path.";
        private const float BoxMinHeight = 30f;
        private const float SquareSize = 16f;
        private const float SquareMargin = 7f;
        private const float FixButtonWidth = 120f;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            SerializedProperty sceneAssetProperty = property.FindPropertyRelative("sceneAsset");
            SerializedProperty sceneNameProperty = property.FindPropertyRelative("sceneName");
            string propertyPath = property.propertyPath;
            SerializedObject serializedObject = property.serializedObject;

            ObjectField sceneAssetField = new ObjectField(preferredLabel);
            sceneAssetField.objectType = typeof(SceneAsset);
            sceneAssetField.allowSceneObjects = false;
            sceneAssetField.tooltip = SceneAssetTooltip;
            sceneAssetField.AddToClassList(BaseField<Object>.alignedFieldUssClassName);
            sceneAssetField.BindProperty(sceneAssetProperty);
            root.Add(sceneAssetField);

            TextField sceneNameField = new TextField("Scene Name");
            sceneNameField.tooltip = SceneNameTooltip;
            sceneNameField.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            sceneNameField.BindProperty(sceneNameProperty);
            sceneNameField.SetEnabled(false);
            root.Add(sceneNameField);

            SceneStatusBox statusBox = new SceneStatusBox();
            root.Add(statusBox);

            // The property passed in can be reused by Unity, so each refresh finds the field again by its path.
            void Refresh()
            {
                if (serializedObject == null || serializedObject.targetObject == null)
                {
                    return;
                }
                serializedObject.UpdateIfRequiredOrScript();
                SerializedProperty current = serializedObject.FindProperty(propertyPath);
                if (current == null)
                {
                    return;
                }
                SceneStatusReport report = ReadReport(current);
                statusBox.Show(report, () => ApplyFixAndRefresh(report));
            }

            void ApplyFixAndRefresh(SceneStatusReport _report)
            {
                SceneStatusReader.ApplyFix(_report);
                Refresh();
            }

            void OnProjectChanged()
            {
                Refresh();
            }

            root.AddManipulator(new ContextualMenuManipulator(_event => AddMoveItems(_event, serializedObject, propertyPath, Refresh)));
            sceneAssetField.RegisterValueChangedCallback(_change => OnSceneAssetChanged(serializedObject, propertyPath, _change.previousValue as SceneAsset, _change.newValue as SceneAsset));
            root.TrackPropertyValue(property, _changed => Refresh());
            root.RegisterCallback<AttachToPanelEvent>(_event =>
            {
                EditorBuildSettings.sceneListChanged += OnProjectChanged;
                SubscribeToAddressables(OnProjectChanged);
                Refresh();
            });
            root.RegisterCallback<DetachFromPanelEvent>(_event =>
            {
                EditorBuildSettings.sceneListChanged -= OnProjectChanged;
                UnsubscribeFromAddressables(OnProjectChanged);
            });
            Refresh();
            return root;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            SerializedProperty sceneAssetProperty = property.FindPropertyRelative("sceneAsset");
            SerializedProperty sceneNameProperty = property.FindPropertyRelative("sceneName");

            float currentY = position.y;
            Rect sceneAssetRect = new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(sceneAssetRect, sceneAssetProperty, label);
            currentY += sceneAssetRect.height + EditorGUIUtility.standardVerticalSpacing;
            if (EditorGUI.EndChangeCheck())
            {
                sceneAssetProperty.serializedObject.ApplyModifiedProperties();
                CopySceneIdentity(property.serializedObject, property.propertyPath, sceneAssetProperty.objectReferenceValue as SceneAsset);
            }

            Rect sceneNameRect = new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.PropertyField(sceneNameRect, sceneNameProperty);
            }
            currentY += sceneNameRect.height + EditorGUIUtility.standardVerticalSpacing * 3;

            SceneStatusReport report = ReadReport(property);
            Rect indentedPositionRect = EditorGUI.IndentedRect(position);
            float boxHeight = StatusBoxHeight(report, indentedPositionRect.width);
            Rect helpBoxRect = new Rect(indentedPositionRect.x, currentY, indentedPositionRect.width, boxHeight);
            DrawStatusBox(helpBoxRect, report);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SceneStatusReport report = ReadReport(property);
            float boxWidth = EditorGUIUtility.currentViewWidth - 40f;

            float totalHeight = 0;
            totalHeight += EditorGUIUtility.singleLineHeight; // the scene asset field
            totalHeight += EditorGUIUtility.standardVerticalSpacing;
            totalHeight += EditorGUIUtility.singleLineHeight; // the scene name field
            totalHeight += EditorGUIUtility.standardVerticalSpacing * 3; // spacing before the status box
            totalHeight += StatusBoxHeight(report, boxWidth);
            totalHeight += EditorGUIUtility.standardVerticalSpacing * 3; // spacing after the status box
            return totalHeight;
        }

        private static void DrawStatusBox(Rect _boxRect, SceneStatusReport _report)
        {
            int oldIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.HelpBox(_boxRect, string.Empty, MessageType.None);

            Rect squareRect = new Rect(_boxRect.x + SquareMargin, _boxRect.y + (_boxRect.height - SquareSize) / 2, SquareSize, SquareSize);
            DrawBoxWithOutline(squareRect, SceneStatusColours.For(_report.Status), SceneStatusColours.Outline);

            float labelX = squareRect.xMax + SquareMargin;
            float labelWidth = MessageWidth(_boxRect.width, _report);
            Rect labelRect = new Rect(labelX, _boxRect.y, labelWidth, _boxRect.height);
            EditorGUI.LabelField(labelRect, _report.Message, MessageStyle());

            if (_report.HasFix)
            {
                Rect buttonRect = new Rect(_boxRect.xMax - FixButtonWidth - 4f, _boxRect.y + (_boxRect.height - EditorGUIUtility.singleLineHeight) / 2, FixButtonWidth, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(buttonRect, _report.FixLabel))
                {
                    SceneStatusReader.ApplyFix(_report);
                }
            }
            EditorGUI.indentLevel = oldIndentLevel;
        }

        private static float StatusBoxHeight(SceneStatusReport _report, float _boxWidth)
        {
            float messageWidth = MessageWidth(_boxWidth, _report);
            float messageHeight = MessageStyle().CalcHeight(new GUIContent(_report.Message), messageWidth);
            return Mathf.Max(BoxMinHeight, messageHeight + 10f);
        }

        private static float MessageWidth(float _boxWidth, SceneStatusReport _report)
        {
            float width = _boxWidth - SquareSize - SquareMargin * 3;
            if (_report.HasFix)
            {
                width -= FixButtonWidth + 4f;
            }
            return Mathf.Max(40f, width);
        }

        private static GUIStyle MessageStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.richText = true;
            style.wordWrap = true;
            style.alignment = TextAnchor.MiddleLeft;
            return style;
        }

        private static SceneStatusReport ReadReport(SerializedProperty _sceneFieldProperty)
        {
            SceneAsset sceneAsset = _sceneFieldProperty.FindPropertyRelative("sceneAsset").objectReferenceValue as SceneAsset;
            string sceneName = _sceneFieldProperty.FindPropertyRelative("sceneName").stringValue;
            string sceneGuid = _sceneFieldProperty.FindPropertyRelative("sceneGuid").stringValue;
            string scenePath = _sceneFieldProperty.FindPropertyRelative("scenePath").stringValue;
            return SceneStatusReader.Read(sceneAsset, sceneName, sceneGuid, scenePath);
        }

        // Binding the field also raises change events (even from None to None), so an empty value clears the stored scene
        // only when the field showed this scene before: the user cleared it. A Missing scene keeps its old path.
        private static void OnSceneAssetChanged(SerializedObject _serializedObject, string _propertyPath, SceneAsset _previous, SceneAsset _next)
        {
            if (_next != null)
            {
                CopySceneIdentity(_serializedObject, _propertyPath, _next);
                return;
            }
            if (_previous == null || _serializedObject == null || _serializedObject.targetObject == null)
            {
                return;
            }
            SerializedProperty property = _serializedObject.FindProperty(_propertyPath);
            if (property == null)
            {
                return;
            }
            string previousGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_previous));
            string storedGuid = property.FindPropertyRelative("sceneGuid").stringValue;
            bool userClearedThisScene = previousGuid == storedGuid;
            if (!userClearedThisScene)
            {
                return;
            }
            CopySceneIdentity(_serializedObject, _propertyPath, null);
        }

        // Copies the scene asset's name, GUID and path into the field, so the scene is known by its asset.
        // Clearing the picker keeps nothing; picking the same scene again changes nothing.
        private static void CopySceneIdentity(SerializedObject _serializedObject, string _propertyPath, SceneAsset _sceneAsset)
        {
            if (_serializedObject == null || _serializedObject.targetObject == null)
            {
                return;
            }
            SerializedProperty property = _serializedObject.FindProperty(_propertyPath);
            if (property == null)
            {
                return;
            }

            string assetPath = string.Empty;
            string sceneName = string.Empty;
            if (_sceneAsset != null)
            {
                assetPath = AssetDatabase.GetAssetPath(_sceneAsset);
                sceneName = _sceneAsset.name;
            }
            string sceneGuid = AssetDatabase.AssetPathToGUID(assetPath);

            SerializedProperty sceneNameProperty = property.FindPropertyRelative("sceneName");
            SerializedProperty sceneGuidProperty = property.FindPropertyRelative("sceneGuid");
            SerializedProperty scenePathProperty = property.FindPropertyRelative("scenePath");
            bool unchanged = sceneNameProperty.stringValue == sceneName && sceneGuidProperty.stringValue == sceneGuid && scenePathProperty.stringValue == assetPath;
            if (unchanged)
            {
                return;
            }
            sceneNameProperty.stringValue = sceneName;
            sceneGuidProperty.stringValue = sceneGuid;
            scenePathProperty.stringValue = assetPath;
            _serializedObject.ApplyModifiedProperties();
        }

        // Right-click: "Move to Addressables" and "Move to Build Settings". Each move removes the other entry.
        private static void AddMoveItems(ContextualMenuPopulateEvent _event, SerializedObject _serializedObject, string _propertyPath, System.Action _afterMove)
        {
            if (_serializedObject == null || _serializedObject.targetObject == null)
            {
                return;
            }
            SerializedProperty property = _serializedObject.FindProperty(_propertyPath);
            if (property == null)
            {
                return;
            }
            SceneStatusReport report = ReadReport(property);
            string addressablesLabel = AddressableScenes.IsInstalled ? "Move to Addressables" : "Move to Addressables (needs the Addressables package)";
            AddMoveItem(_event, addressablesLabel, report, ScenePlace.Addressables, _afterMove);
            AddMoveItem(_event, "Move to Build Settings", report, ScenePlace.BuildSettings, _afterMove);
        }

        private static void AddMoveItem(ContextualMenuPopulateEvent _event, string _label, SceneStatusReport _report, ScenePlace _place, System.Action _afterMove)
        {
            bool canMove = SceneStatusReader.CanMoveTo(_report, _place);
            DropdownMenuAction.Status status = canMove ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
            _event.menu.AppendAction(_label, _action =>
            {
                SceneStatusReader.MoveTo(_report, _place);
                _afterMove();
            }, status);
        }

        private static void SubscribeToAddressables(System.Action _onChanged)
        {
            IAddressableSceneProvider provider = AddressableScenes.Provider;
            if (provider == null)
            {
                return;
            }
            provider.Changed += _onChanged;
        }

        private static void UnsubscribeFromAddressables(System.Action _onChanged)
        {
            IAddressableSceneProvider provider = AddressableScenes.Provider;
            if (provider == null)
            {
                return;
            }
            provider.Changed -= _onChanged;
        }

        private static void DrawBoxWithOutline(Rect _rect, Color _fill, Color _outline)
        {
            EditorGUI.DrawRect(_rect, _fill);
            EditorGUI.DrawRect(new Rect(_rect.x, _rect.y, _rect.width, 1), _outline);
            EditorGUI.DrawRect(new Rect(_rect.x, _rect.yMax - 1, _rect.width, 1), _outline);
            EditorGUI.DrawRect(new Rect(_rect.x, _rect.y, 1, _rect.height), _outline);
            EditorGUI.DrawRect(new Rect(_rect.xMax - 1, _rect.y, 1, _rect.height), _outline);
        }
    }
}
