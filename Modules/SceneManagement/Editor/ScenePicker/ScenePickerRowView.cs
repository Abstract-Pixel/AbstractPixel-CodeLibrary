using AbstractPixel.Core.Editor.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// One row of the scene picker's list (session 5A2, decision 1). Left: the number (Recent rows, Scene Groups tab),
    /// the icon and the name. Right-aligned: separator, label button, separator, status button, separator, folder
    /// button. The three buttons are flat, like text and icons, and show a background only on hover. A heading row shows
    /// only its text. The view is reused by the ListView, so Bind replaces everything.
    /// </summary>
    public class ScenePickerRowView : VisualElement
    {
        public const string FolderTooltip = "Select in the Project window";

        private readonly Label number;
        private readonly Image icon;
        private readonly Label title;
        private readonly Button labelButton;
        private readonly Button statusButton;
        private readonly VisualElement statusDot;
        private readonly Button folderButton;
        private readonly Label heading;
        private readonly ScenePickerRowCallbacks callbacks;

        public ScenePickerRow Row { get; private set; }

        public ScenePickerRowView(ScenePickerRowCallbacks _callbacks)
        {
            callbacks = _callbacks;
            AddToClassList("scene-picker-row");

            number = NewLabel("scene-picker-row__number");
            icon = new Image();
            icon.AddToClassList("scene-picker-row__icon");
            title = NewLabel("scene-picker-row__title");
            VisualElement gap = new VisualElement();
            gap.AddToClassList("scene-picker-row__gap");

            labelButton = new Button(() => Raise(callbacks.LabelClicked));
            labelButton.AddToClassList("scene-picker-row__flat");
            labelButton.AddToClassList("scene-picker-row__label");
            // Its tooltip says what a click selects; Unity would otherwise replace it with the cut-short text.
            labelButton.displayTooltipWhenElided = false;

            statusButton = new Button(() => Raise(callbacks.StatusClicked));
            statusButton.AddToClassList("scene-picker-row__flat");
            statusButton.AddToClassList("scene-picker-row__status");
            statusDot = new VisualElement();
            statusDot.AddToClassList("scene-picker-row__status-dot");
            statusDot.pickingMode = PickingMode.Ignore;
            statusButton.Add(statusDot);

            folderButton = new Button(() => Raise(callbacks.FolderClicked));
            folderButton.AddToClassList("scene-picker-row__flat");
            folderButton.AddToClassList("scene-picker-row__folder");
            folderButton.tooltip = FolderTooltip;
            Image folderIcon = new Image();
            folderIcon.image = ScenePickerIcons.Get(ScenePickerIcons.SelectIcon);
            folderIcon.AddToClassList("scene-picker-row__folder-icon");
            folderIcon.pickingMode = PickingMode.Ignore;
            folderButton.Add(folderIcon);

            heading = NewLabel("scene-picker-row__heading");

            Add(number);
            Add(icon);
            Add(title);
            Add(gap);
            Add(Separator());
            Add(labelButton);
            Add(Separator());
            Add(statusButton);
            Add(Separator());
            Add(folderButton);
            Add(heading);

            RegisterCallback<ClickEvent>(OnRowClicked);
            this.AddManipulator(new ContextualMenuManipulator(OnMenuOpening));
        }

        /// <param name="_showNumbers">True when any shown row has a number, so names line up in one column.</param>
        public void Bind(ScenePickerRow _row, string[] _searchWords, ScenePickerProjectFacts _facts, bool _showNumbers)
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

            number.style.display = _showNumbers ? DisplayStyle.Flex : DisplayStyle.None;
            number.text = _row.Number > 0 ? _row.Number.ToString() : string.Empty;
            icon.image = ScenePickerIcons.ForAsset(_row);
            title.text = ScenePickerSearch.BoldMatches(_row.Title, _searchWords);
            EnableInClassList("scene-picker-row--cannot-open", !_row.CanOpen);
            BindLabel(_row);
            BindStatus(_row, _facts);
            tooltip = RowTooltip(_row);
        }

        // "No group" and "No scene set" are plain grey text: no hover, and a click goes to the row.
        private void BindLabel(ScenePickerRow _row)
        {
            bool clickable = ScenePickerLabelRules.IsClickable(_row);
            labelButton.text = _row.Detail;
            labelButton.tooltip = ScenePickerLabelRules.Tooltip(_row);
            labelButton.EnableInClassList("scene-picker-row__label--plain", !clickable);
            labelButton.pickingMode = clickable ? PickingMode.Position : PickingMode.Ignore;
        }

        private void BindStatus(ScenePickerRow _row, ScenePickerProjectFacts _facts)
        {
            ScenePickerChip chip = ScenePickerStatusRules.Chip(_row);
            statusDot.style.backgroundColor = ChipColour(chip);
            statusButton.tooltip = ScenePickerStatusRules.Tooltip(_row, _facts);
        }

        private static Color ChipColour(ScenePickerChip _chip)
        {
            switch (_chip)
            {
                case ScenePickerChip.InBuild:
                    return AbstractPixel.Core.Editor.SceneStatusColours.For(AbstractPixel.Core.Editor.SceneStatus.InBuild);
                case ScenePickerChip.Addressable:
                    return AbstractPixel.Core.Editor.SceneStatusColours.For(AbstractPixel.Core.Editor.SceneStatus.Addressable);
                case ScenePickerChip.Sample:
                    return SampleGrey;
                case ScenePickerChip.Unused:
                    return EditorUI.Yellow;
                default:
                    return AbstractPixel.Core.Editor.SceneStatusColours.For(AbstractPixel.Core.Editor.SceneStatus.NotInBuild);
            }
        }

        private static readonly Color SampleGrey = new Color(0.55f, 0.55f, 0.55f);

        // A click on the row opens it; a click on one of its buttons does only what that button does.
        private void OnRowClicked(ClickEvent _click)
        {
            VisualElement target = _click.target as VisualElement;
            if (IsInside(target, labelButton) || IsInside(target, statusButton) || IsInside(target, folderButton))
            {
                return;
            }
            if (Row == null || Row.IsHeading)
            {
                return;
            }
            Raise(callbacks.Opened);
        }

        private void OnMenuOpening(ContextualMenuPopulateEvent _event)
        {
            if (Row == null || Row.IsHeading || callbacks.MenuOpening == null)
            {
                return;
            }
            callbacks.MenuOpening(Row, _event.menu);
            _event.StopPropagation();
        }

        private void Raise(System.Action<ScenePickerRow> _callback)
        {
            if (_callback == null || Row == null || Row.IsHeading)
            {
                return;
            }
            _callback(Row);
        }

        private static bool IsInside(VisualElement _target, VisualElement _button)
        {
            if (_target == null || _button.pickingMode == PickingMode.Ignore)
            {
                return false;
            }
            return _target == _button || _button.Contains(_target);
        }

        private static string RowTooltip(ScenePickerRow _row)
        {
            if (_row.Kind != ScenePickerRowKind.SceneGroup)
            {
                return _row.AssetPath;
            }
            if (!_row.CanOpen)
            {
                return _row.Title + " has no main scene, so it cannot open. Select it to set its main scene in the Inspector.";
            }
            return "Main scene: " + _row.MainSceneName + "\n" + _row.AssetPath;
        }

        private static Label NewLabel(string _className)
        {
            Label label = new Label();
            label.AddToClassList(_className);
            return label;
        }

        private static VisualElement Separator()
        {
            VisualElement separator = new VisualElement();
            separator.AddToClassList("scene-picker-row__separator");
            separator.pickingMode = PickingMode.Ignore;
            return separator;
        }
    }
}
