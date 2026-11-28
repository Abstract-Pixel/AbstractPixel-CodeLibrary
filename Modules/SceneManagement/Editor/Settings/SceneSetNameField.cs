using AbstractPixel.Core.Editor.UI;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// A scene set's name on its card on the Scene System page (session 5A2). Hovering turns it link blue. A click
    /// folds or unfolds the card, after Unity's double-click time, so a double-click (or F2 on the card) starts a rename
    /// instead and never folds it. While renaming, Enter or clicking away renames the asset file; Esc keeps the old
    /// name; an empty, invalid or taken name shows a red line under the field and keeps it editing.
    /// </summary>
    public class SceneSetNameField : VisualElement
    {
        // How long a first click waits for a second one before it folds the card.
        private const long DoubleClickMilliseconds = 280;

        private readonly SceneSet set;
        private readonly Color restColour;
        private readonly Action toggleFold;
        private readonly Label nameLabel;
        private readonly TextField editor;
        private readonly Label problemLine;
        private IVisualElementScheduledItem pendingFold;
        private bool isEditing;

        public SceneSetNameField(SceneSet _set, Color _restColour, Action _toggleFold)
        {
            set = _set;
            restColour = _restColour;
            toggleFold = _toggleFold;
            style.flexDirection = FlexDirection.Column;
            style.justifyContent = Justify.Center;

            nameLabel = new Label(_set.name);
            nameLabel.tooltip = "Double-click or press F2 to rename this scene set.";
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.color = restColour;
            nameLabel.style.overflow = Overflow.Hidden;
            nameLabel.style.textOverflow = TextOverflow.Ellipsis;
            nameLabel.style.whiteSpace = WhiteSpace.NoWrap;
            nameLabel.style.paddingLeft = 2;
            nameLabel.RegisterCallback<PointerEnterEvent>(_event => nameLabel.style.color = EditorUI.LinkBlue);
            nameLabel.RegisterCallback<PointerLeaveEvent>(_event => nameLabel.style.color = restColour);
            nameLabel.RegisterCallback<ClickEvent>(OnNameClicked);
            Add(nameLabel);

            editor = new TextField();
            editor.isDelayed = false;
            editor.selectAllOnFocus = true;
            editor.style.display = DisplayStyle.None;
            editor.style.marginLeft = 0;
            editor.style.marginRight = 0;
            editor.RegisterCallback<KeyDownEvent>(OnEditorKeyDown, TrickleDown.TrickleDown);
            editor.RegisterCallback<FocusOutEvent>(_event => Commit());
            Add(editor);

            problemLine = new Label();
            problemLine.style.color = EditorUI.Red;
            problemLine.style.fontSize = 11;
            problemLine.style.whiteSpace = WhiteSpace.Normal;
            problemLine.style.display = DisplayStyle.None;
            Add(problemLine);
        }

        public bool IsEditing
        {
            get { return isEditing; }
        }

        /// <summary>The red line's text while a name is refused, for probes and stills.</summary>
        public string ProblemText
        {
            get { return problemLine.text; }
        }

        /// <summary>Starts a rename once the field is on screen: the text is selected and has the keyboard.</summary>
        public void BeginRename()
        {
            if (set == null)
            {
                return;
            }
            isEditing = true;
            CancelPendingFold();
            editor.SetValueWithoutNotify(set.name);
            nameLabel.style.display = DisplayStyle.None;
            editor.style.display = DisplayStyle.Flex;
            editor.schedule.Execute(() =>
            {
                editor.Focus();
                editor.SelectAll();
            });
        }

        /// <summary>Types a new name and presses Enter, as a probe would.</summary>
        public void SubmitForProbe(string _name)
        {
            editor.SetValueWithoutNotify(_name);
            Commit();
        }

        /// <summary>Presses Esc, as a probe would.</summary>
        public void CancelForProbe()
        {
            Cancel();
        }

        private void OnNameClicked(ClickEvent _click)
        {
            if (_click.clickCount >= 2)
            {
                BeginRename();
                _click.StopPropagation();
                return;
            }
            CancelPendingFold();
            pendingFold = schedule.Execute(() =>
            {
                pendingFold = null;
                if (!isEditing && toggleFold != null)
                {
                    toggleFold();
                }
            });
            pendingFold.ExecuteLater(DoubleClickMilliseconds);
        }

        private void CancelPendingFold()
        {
            if (pendingFold != null)
            {
                pendingFold.Pause();
                pendingFold = null;
            }
        }

        private void OnEditorKeyDown(KeyDownEvent _key)
        {
            bool isEnter = _key.keyCode == KeyCode.Return || _key.keyCode == KeyCode.KeypadEnter;
            if (isEnter)
            {
                Commit();
                _key.StopImmediatePropagation();
                return;
            }
            if (_key.keyCode == KeyCode.Escape)
            {
                Cancel();
                _key.StopImmediatePropagation();
            }
        }

        // Renames the asset, or shows why not and keeps editing.
        private void Commit()
        {
            if (!isEditing)
            {
                return;
            }
            string newName = editor.value;
            bool renamed = SceneSetRename.Rename(set, newName, out string error);
            if (!renamed)
            {
                ShowProblem(error);
                return;
            }
            EndEditing();
            if (newName != nameLabel.text)
            {
                nameLabel.text = set.name;
                SceneSystemSettingsAssets.NotifyChanged();
            }
        }

        // Esc: the asset keeps its name.
        private void Cancel()
        {
            EndEditing();
        }

        private void EndEditing()
        {
            isEditing = false;
            ShowProblem(string.Empty);
            editor.style.display = DisplayStyle.None;
            nameLabel.style.display = DisplayStyle.Flex;
            nameLabel.style.color = restColour;
        }

        private void ShowProblem(string _problem)
        {
            problemLine.text = _problem ?? string.Empty;
            problemLine.style.display = string.IsNullOrEmpty(_problem) ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}
