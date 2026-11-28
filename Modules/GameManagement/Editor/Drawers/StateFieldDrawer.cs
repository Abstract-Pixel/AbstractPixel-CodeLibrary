using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Every state field: a normal object field, plus a red icon when the picked state is not in the state list.</summary>
    [CustomPropertyDrawer(typeof(StateSO))]
    internal class StateFieldDrawer : PropertyDrawer
    {
        private const float IconSize = 16f;
        private const string Warning = "Not in the state list, so every request for it is refused. Add it from the state's Inspector or in Project Settings > Abstract Pixel > Game Management.";

        public override VisualElement CreatePropertyGUI(SerializedProperty _property)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;

            ObjectField field = new ObjectField(preferredLabel) { objectType = typeof(StateSO), allowSceneObjects = false, tooltip = _property.tooltip };
            field.AddToClassList(ObjectField.alignedFieldUssClassName);
            field.style.flexGrow = 1;
            field.style.flexShrink = 1;
            field.style.minWidth = 0;
            field.BindProperty(_property);
            row.Add(field);

            Image icon = new Image { image = ErrorIcon(), tooltip = Warning };
            icon.style.width = IconSize;
            icon.style.height = IconSize;
            icon.style.flexShrink = 0;
            row.Add(icon);

            StateAssetWatcher watcher = null;
            void Show()
            {
                GameStateCatalog catalog = Catalog();
                icon.style.display = IsUnlisted(field.value as StateSO, catalog) ? DisplayStyle.Flex : DisplayStyle.None;
                watcher.Watch(new Object[] { catalog });
            }
            watcher = new StateAssetWatcher(Show);
            row.Add(watcher);
            field.RegisterValueChangedCallback(_change => Show());
            row.RegisterCallback<AttachToPanelEvent>(_event => Show());
            return row;
        }

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            bool isUnlisted = !_property.hasMultipleDifferentValues && IsUnlisted(_property.objectReferenceValue as StateSO, Catalog());
            Rect field = _position;
            if (isUnlisted)
            {
                field.width -= IconSize + 2f;
            }
            EditorGUI.ObjectField(field, _property, typeof(StateSO), _label);
            if (isUnlisted)
            {
                Rect icon = new Rect(field.xMax + 2f, _position.y + (_position.height - IconSize) / 2f, IconSize, IconSize);
                GUI.Label(icon, new GUIContent(ErrorIcon(), Warning), GUIStyle.none);
            }
        }

        private static GameStateCatalog Catalog()
        {
            GameManagementSettings settings = GameManagementSettingsAssets.Find();
            return settings != null ? settings.Catalog : null;
        }

        private static bool IsUnlisted(StateSO _state, GameStateCatalog _catalog)
        {
            return _state != null && _catalog != null && !_catalog.Contains(_state);
        }

        private static Texture ErrorIcon()
        {
            return EditorGUIUtility.IconContent("console.erroricon.sml").image;
        }
    }
}
