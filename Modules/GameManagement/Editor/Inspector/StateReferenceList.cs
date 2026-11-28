using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>A numbered, reorderable list of asset references with + and − under it. Parents and input actions share it.</summary>
    internal class StateReferenceList : VisualElement
    {
        private readonly SerializedObject data;
        private readonly string path;
        private readonly Type type;
        private readonly Action<VisualElement, Object> showValue;
        private readonly Action<VisualElement, ObjectField> buildBody;
        private readonly ListView list;
        private readonly Label emptyNote;

        /// <param name="_emptyText">Shown in place of the list while it has no entries.</param>
        /// <param name="_showValue">Updates a row for its value: its tooltip and anything under the field.</param>
        /// <param name="_buildBody">Adds to a new row's body. A list that does this has rows as tall as their text.</param>
        public StateReferenceList(SerializedObject _data, string _path, Type _type, string _noun, string _emptyText, Action<VisualElement, Object> _showValue,
            Action<VisualElement, ObjectField> _buildBody = null)
        {
            data = _data;
            path = _path;
            type = _type;
            showValue = _showValue;
            buildBody = _buildBody;
            list = new ListView
            {
                reorderable = true,
                showBoundCollectionSize = false,
                fixedItemHeight = 24,
                virtualizationMethod = _buildBody != null ? CollectionVirtualizationMethod.DynamicHeight : CollectionVirtualizationMethod.FixedHeight,
                selectionType = SelectionType.Single,
                makeItem = MakeRow,
                bindItem = BindRow,
                unbindItem = (_row, _index) => _row.Q<ObjectField>().Unbind()
            };
            list.AddToClassList("si-reference-list");
            if (_buildBody != null)
            {
                list.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            }
            list.itemIndexChanged += (_from, _to) => list.schedule.Execute(list.RefreshItems);
            list.BindProperty(Property());
            Add(list);

            VisualElement footer = new VisualElement();
            footer.AddToClassList("si-list-footer");
            emptyNote = new Label(_emptyText);
            emptyNote.AddToClassList("si-list-empty");
            footer.Add(emptyNote);
            footer.Add(new Button(AddSlot) { text = "+", tooltip = "Add an " + _noun });
            footer.Add(new Button(RemoveSlot) { text = "−", tooltip = "Remove the selected " + _noun + ", or the last one" });
            Add(footer);
        }

        /// <summary>Updates the rows on screen. Their values can change without the list changing.</summary>
        public void Refresh()
        {
            // Unity's own empty list is tall and says only "List is empty".
            bool isEmpty = Property().arraySize == 0;
            list.style.display = isEmpty ? DisplayStyle.None : DisplayStyle.Flex;
            emptyNote.style.display = isEmpty ? DisplayStyle.Flex : DisplayStyle.None;
            list.Query(className: "si-reference-row").ForEach(_row => showValue(_row, _row.Q<ObjectField>().value));
        }

        private SerializedProperty Property()
        {
            return data.FindProperty(path);
        }

        private VisualElement MakeRow()
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("si-reference-row");
            Label handle = new Label("≡") { tooltip = "Drag to reorder" };
            handle.AddToClassList("si-handle");
            row.Add(handle);
            Label index = new Label();
            index.AddToClassList("si-rank");
            row.Add(index);
            ObjectField field = new ObjectField { objectType = type, allowSceneObjects = false };
            field.AddToClassList("si-reference-field");
            field.RegisterValueChangedCallback(_change => showValue(row, _change.newValue));
            VisualElement body = new VisualElement();
            body.AddToClassList("si-reference-body");
            body.Add(field);
            row.Add(body);
            buildBody?.Invoke(body, field);
            return row;
        }

        private void BindRow(VisualElement _row, int _index)
        {
            _row.Q<Label>(className: "si-rank").text = (_index + 1).ToString();
            ObjectField field = _row.Q<ObjectField>();
            SerializedProperty property = Property();
            if (_index < property.arraySize)
            {
                field.BindProperty(property.GetArrayElementAtIndex(_index));
            }
            showValue(_row, field.value);
        }

        private void AddSlot()
        {
            data.Update();
            SerializedProperty property = Property();
            property.arraySize++;
            // Unity copies the last element into a new slot, which would list the same asset twice.
            property.GetArrayElementAtIndex(property.arraySize - 1).objectReferenceValue = null;
            data.ApplyModifiedProperties();
        }

        private void RemoveSlot()
        {
            data.Update();
            SerializedProperty property = Property();
            if (property.arraySize == 0)
            {
                return;
            }
            int index = list.selectedIndex >= 0 && list.selectedIndex < property.arraySize ? list.selectedIndex : property.arraySize - 1;
            property.GetArrayElementAtIndex(index).objectReferenceValue = null;
            property.DeleteArrayElementAtIndex(index);
            data.ApplyModifiedProperties();
            list.ClearSelection();
        }
    }
}
