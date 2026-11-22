using AbstractPixel.Core.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// One row of the scene picker's list: icon, name, grey detail, scene status chip, a separator and the Select
    /// button. A heading row shows only its text. The view is reused by the ListView, so Bind replaces everything.
    /// </summary>
    public class ScenePickerRowView : VisualElement
    {
        public const string SelectTooltip = "Select in the Project window";

        private readonly Image icon;
        private readonly Label title;
        private readonly Label detail;
        private readonly VisualElement statusChip;
        private readonly VisualElement separator;
        private readonly Button selectButton;
        private readonly Label heading;

        public ScenePickerRow Row { get; private set; }

        public ScenePickerRowView(System.Action<ScenePickerRow> _opened, System.Action<ScenePickerRow> _selected)
        {
            AddToClassList("scene-picker-row");

            icon = new Image();
            icon.AddToClassList("scene-picker-row__icon");
            title = new Label();
            title.AddToClassList("scene-picker-row__title");
            detail = new Label();
            detail.AddToClassList("scene-picker-row__detail");
            statusChip = new VisualElement();
            statusChip.AddToClassList("scene-picker-row__status");
            separator = new VisualElement();
            separator.AddToClassList("scene-picker-row__separator");
            selectButton = new Button(() => _selected(Row));
            selectButton.AddToClassList("scene-picker-row__select");
            selectButton.tooltip = SelectTooltip;
            Image selectIcon = new Image();
            selectIcon.image = ScenePickerIcons.Get(ScenePickerIcons.SelectIcon);
            selectIcon.AddToClassList("scene-picker-row__select-icon");
            selectButton.Add(selectIcon);
            heading = new Label();
            heading.AddToClassList("scene-picker-row__heading");

            Add(icon);
            Add(title);
            Add(detail);
            Add(statusChip);
            Add(separator);
            Add(selectButton);
            Add(heading);

            RegisterCallback<ClickEvent>(_click => OnRowClicked(_click, _opened));
        }

        public void Bind(ScenePickerRow _row, string[] _searchWords)
        {
            Row = _row;
            bool isHeading = _row == null || _row.IsHeading;
            EnableInClassList("scene-picker-row--heading", isHeading);
            heading.text = isHeading && _row != null ? _row.Title : string.Empty;
            if (isHeading)
            {
                tooltip = string.Empty;
                return;
            }

            icon.image = ScenePickerIcons.ForAsset(_row);
            title.text = ScenePickerSearch.BoldMatches(_row.Title, _searchWords);
            detail.text = _row.Detail;
            EnableInClassList("scene-picker-row--cannot-open", !_row.CanOpen);
            statusChip.style.backgroundColor = SceneStatusColours.For(_row.Status);
            statusChip.tooltip = StatusText(_row.Status);
            tooltip = RowTooltip(_row);
        }

        // A click on the row opens; a click on the Select button only selects (the button handles it).
        private void OnRowClicked(ClickEvent _click, System.Action<ScenePickerRow> _opened)
        {
            VisualElement target = _click.target as VisualElement;
            bool onSelectButton = target != null && (target == selectButton || selectButton.Contains(target));
            if (onSelectButton || Row == null || Row.IsHeading)
            {
                return;
            }
            _opened(Row);
        }

        private static string RowTooltip(ScenePickerRow _row)
        {
            if (!_row.CanOpen)
            {
                return _row.Title + " has no main scene, so it cannot open. Select it to set its main scene in the Inspector.";
            }
            return _row.AssetPath;
        }

        private static string StatusText(SceneStatus _status)
        {
            switch (_status)
            {
                case SceneStatus.InBuild:
                    return "In build";
                case SceneStatus.Addressable:
                    return "Addressable";
                case SceneStatus.NotInBuild:
                    return "Not in build";
                case SceneStatus.Missing:
                    return "Missing";
                default:
                    return "No scene";
            }
        }
    }
}
