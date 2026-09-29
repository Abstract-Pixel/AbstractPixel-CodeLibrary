using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScreenshotTool.Editor
{
    public sealed class ScreenshotSessionView
    {
        private const float THUMBNAIL_MAX_WIDTH = 240f;
        private const float THUMBNAIL_MAX_HEIGHT = 135f;
        private const float THUMBNAIL_BORDER_THICKNESS = 2f;
        private const float NARROW_BREAKPOINT_WIDTH = 480f;

        private readonly Color COLOR_CARD_BACKGROUND_NORMAL = new Color(0.22f, 0.22f, 0.22f, 1f);
        private readonly Color COLOR_CARD_BACKGROUND_KEEP = new Color(0.16f, 0.24f, 0.18f, 1f);
        private readonly Color COLOR_CARD_BORDER_NORMAL = new Color(0.18f, 0.18f, 0.18f, 1f);
        private readonly Color COLOR_CARD_BORDER_KEEP = new Color(0.32f, 0.62f, 0.38f, 1f);

        private readonly Color COLOR_THUMBNAIL_BORDER_NORMAL = new Color(0.92f, 0.92f, 0.92f, 0.85f);
        private readonly Color COLOR_THUMBNAIL_BORDER_KEEP = new Color(0.38f, 0.78f, 0.46f, 1f);
        private readonly Color COLOR_THUMBNAIL_BORDER_HOVER = new Color(0.35f, 0.75f, 1f, 1f);

        private readonly Color COLOR_INDEX_PILL_BACKGROUND = new Color(0.15f, 0.15f, 0.15f, 1f);
        private readonly Color COLOR_DIVIDER_LINE = new Color(0.32f, 0.32f, 0.32f, 0.5f);
        private readonly Color COLOR_BUTTON_SURFACE = new Color(0.28f, 0.28f, 0.28f, 1f);
        private readonly Color COLOR_BUTTON_BORDER = new Color(0.18f, 0.18f, 0.18f, 1f);

        private readonly VisualElement sessionContainer;
        private readonly Foldout sessionFoldout;
        private readonly Label sessionSummaryLabel;
        private readonly ScrollView itemsScrollView;
        private readonly VisualElement topToolbar;
        private Button deleteUnmarkedButton;

        public VisualElement RootElement => sessionContainer;

        public ScreenshotSessionView()
        {
            sessionContainer = new VisualElement();
            sessionContainer.style.marginTop = 14;
            sessionContainer.style.paddingTop = 10;
            sessionContainer.style.borderTopWidth = 1;
            sessionContainer.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            sessionContainer.style.display = DisplayStyle.None;

            sessionFoldout = new Foldout
            {
                text = "PLAY SESSION DATA",
                value = true
            };
            sessionFoldout.style.unityFontStyleAndWeight = FontStyle.Bold;
            sessionFoldout.style.fontSize = 12;

            sessionSummaryLabel = new Label();
            sessionSummaryLabel.style.marginTop = 4;
            sessionSummaryLabel.style.marginBottom = 8;
            sessionSummaryLabel.style.unityFontStyleAndWeight = FontStyle.Normal;
            sessionSummaryLabel.style.fontSize = 11;
            sessionSummaryLabel.style.color = new Color(0.85f, 0.85f, 0.85f);
            sessionFoldout.Add(sessionSummaryLabel);

            topToolbar = new VisualElement();
            topToolbar.style.flexDirection = FlexDirection.Row;
            topToolbar.style.flexWrap = Wrap.Wrap;
            topToolbar.style.marginBottom = 10;
            sessionFoldout.Add(topToolbar);

            itemsScrollView = new ScrollView();
            itemsScrollView.style.maxHeight = 480;
            itemsScrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            sessionFoldout.Add(itemsScrollView);

            sessionContainer.Add(sessionFoldout);
        }

        public void PopulateSession(ScreenshotSessionTracker _sessionTracker, Action _onDataModified)
        {
            itemsScrollView.Clear();
            topToolbar.Clear();

            if (_sessionTracker == null || !_sessionTracker.HasSessionData)
            {
                sessionContainer.style.display = DisplayStyle.None;
                return;
            }

            sessionContainer.style.display = DisplayStyle.Flex;
            UpdateSummaryText(_sessionTracker);

            Button keepAllButton = new Button(() =>
            {
                _sessionTracker.MarkAllToKeep(true);
                PopulateSession(_sessionTracker, _onDataModified);
                _onDataModified?.Invoke();
            })
            {
                text = "Keep All"
            };
            keepAllButton.style.flexGrow = 1;
            keepAllButton.style.minWidth = 85;
            keepAllButton.style.height = 24;
            keepAllButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            keepAllButton.style.backgroundColor = new Color(0.22f, 0.44f, 0.22f);
            keepAllButton.style.color = Color.white;
            keepAllButton.style.marginRight = 4;
            keepAllButton.style.marginBottom = 4;
            topToolbar.Add(keepAllButton);

            Button deleteAllButton = new Button(() =>
            {
                _sessionTracker.DeleteAll();
                PopulateSession(_sessionTracker, _onDataModified);
                _onDataModified?.Invoke();
            })
            {
                text = "Delete All"
            };
            deleteAllButton.style.flexGrow = 1;
            deleteAllButton.style.minWidth = 85;
            deleteAllButton.style.height = 24;
            deleteAllButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            deleteAllButton.style.backgroundColor = new Color(0.48f, 0.2f, 0.2f);
            deleteAllButton.style.color = Color.white;
            deleteAllButton.style.marginRight = 4;
            deleteAllButton.style.marginBottom = 4;
            topToolbar.Add(deleteAllButton);

            deleteUnmarkedButton = new Button(() =>
            {
                _sessionTracker.DeleteUnmarkedNow();
                PopulateSession(_sessionTracker, _onDataModified);
                _onDataModified?.Invoke();
            })
            {
                text = $"🗑️ Delete Unmarked ({_sessionTracker.UnmarkedCount})"
            };
            deleteUnmarkedButton.style.flexGrow = 1.4f;
            deleteUnmarkedButton.style.minWidth = 140;
            deleteUnmarkedButton.style.height = 24;
            deleteUnmarkedButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            deleteUnmarkedButton.style.backgroundColor = new Color(0.72f, 0.2f, 0.2f);
            deleteUnmarkedButton.style.color = Color.white;
            deleteUnmarkedButton.style.marginBottom = 4;
            deleteUnmarkedButton.SetEnabled(_sessionTracker.UnmarkedCount > 0);
            topToolbar.Add(deleteUnmarkedButton);

            for (int itemIndex = 0; itemIndex < _sessionTracker.TrackedItems.Count; itemIndex++)
            {
                ScreenshotSessionItem currentItem = _sessionTracker.TrackedItems[itemIndex];
                VisualElement card = CreateItemCard(currentItem, itemIndex + 1, _sessionTracker, _onDataModified);
                itemsScrollView.Add(card);
            }
        }

        public void HideSession()
        {
            sessionContainer.style.display = DisplayStyle.None;
            itemsScrollView.Clear();
        }

        private VisualElement CreateItemCard(
            ScreenshotSessionItem _item,
            int _index,
            ScreenshotSessionTracker _tracker,
            Action _onDataModified)
        {
            VisualElement cardContainer = new VisualElement();
            cardContainer.style.paddingLeft = 8;
            cardContainer.style.paddingRight = 8;
            cardContainer.style.paddingTop = 8;
            cardContainer.style.paddingBottom = 8;
            cardContainer.style.marginBottom = 8;
            cardContainer.style.borderLeftWidth = 1;
            cardContainer.style.borderRightWidth = 1;
            cardContainer.style.borderTopWidth = 1;
            cardContainer.style.borderBottomWidth = 1;
            cardContainer.style.borderTopLeftRadius = 4;
            cardContainer.style.borderTopRightRadius = 4;
            cardContainer.style.borderBottomLeftRadius = 4;
            cardContainer.style.borderBottomRightRadius = 4;

            VisualElement visualMediaRow = new VisualElement();
            visualMediaRow.style.flexDirection = FlexDirection.Row;
            visualMediaRow.style.alignItems = Align.FlexStart;

            VisualElement indexColumn = new VisualElement();
            indexColumn.style.width = 32;
            indexColumn.style.alignItems = Align.Center;
            indexColumn.style.justifyContent = Justify.FlexStart;
            indexColumn.style.paddingTop = 4;
            indexColumn.style.marginRight = 6;
            indexColumn.style.flexShrink = 0;

            Label indexLabel = new Label($"#{_index}");
            indexLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            indexLabel.style.fontSize = 11;
            indexLabel.style.color = Color.white;
            indexLabel.style.backgroundColor = COLOR_INDEX_PILL_BACKGROUND;
            indexLabel.style.paddingLeft = 5;
            indexLabel.style.paddingRight = 5;
            indexLabel.style.paddingTop = 2;
            indexLabel.style.paddingBottom = 2;
            indexLabel.style.borderTopLeftRadius = 3;
            indexLabel.style.borderTopRightRadius = 3;
            indexLabel.style.borderBottomLeftRadius = 3;
            indexLabel.style.borderBottomRightRadius = 3;
            indexColumn.Add(indexLabel);
            visualMediaRow.Add(indexColumn);

            VisualElement thumbnailContainer = new VisualElement();
            thumbnailContainer.style.width = THUMBNAIL_MAX_WIDTH;
            thumbnailContainer.style.height = THUMBNAIL_MAX_HEIGHT;
            thumbnailContainer.style.maxWidth = THUMBNAIL_MAX_WIDTH;
            thumbnailContainer.style.maxHeight = THUMBNAIL_MAX_HEIGHT;
            thumbnailContainer.style.flexShrink = 0;
            thumbnailContainer.style.backgroundColor = Color.black;
            thumbnailContainer.style.borderLeftWidth = THUMBNAIL_BORDER_THICKNESS;
            thumbnailContainer.style.borderRightWidth = THUMBNAIL_BORDER_THICKNESS;
            thumbnailContainer.style.borderTopWidth = THUMBNAIL_BORDER_THICKNESS;
            thumbnailContainer.style.borderBottomWidth = THUMBNAIL_BORDER_THICKNESS;
            thumbnailContainer.style.borderTopLeftRadius = 3;
            thumbnailContainer.style.borderTopRightRadius = 3;
            thumbnailContainer.style.borderBottomLeftRadius = 3;
            thumbnailContainer.style.borderBottomRightRadius = 3;
            thumbnailContainer.tooltip = "Double-click to open full image";

            Texture2D thumbnailTexture = _item.GetOrCreateThumbnail();

            Image thumbnailImage = new Image
            {
                image = thumbnailTexture,
                scaleMode = ScaleMode.ScaleToFit
            };
            thumbnailImage.style.width = StyleKeyword.Auto;
            thumbnailImage.style.height = StyleKeyword.Auto;
            thumbnailImage.style.flexGrow = 1;
            thumbnailContainer.Add(thumbnailImage);

            Action applyThumbnailBorder = () =>
            {
                Color targetBorderColor = _item.IsMarkedToKeep ? COLOR_THUMBNAIL_BORDER_KEEP : COLOR_THUMBNAIL_BORDER_NORMAL;
                thumbnailContainer.style.borderLeftColor = targetBorderColor;
                thumbnailContainer.style.borderRightColor = targetBorderColor;
                thumbnailContainer.style.borderTopColor = targetBorderColor;
                thumbnailContainer.style.borderBottomColor = targetBorderColor;
            };

            applyThumbnailBorder();

            thumbnailContainer.RegisterCallback<MouseEnterEvent>(_ =>
            {
                Color hoverColor = _item.IsMarkedToKeep ? new Color(0.55f, 0.95f, 0.65f, 1f) : COLOR_THUMBNAIL_BORDER_HOVER;
                thumbnailContainer.style.borderLeftColor = hoverColor;
                thumbnailContainer.style.borderRightColor = hoverColor;
                thumbnailContainer.style.borderTopColor = hoverColor;
                thumbnailContainer.style.borderBottomColor = hoverColor;
            });

            thumbnailContainer.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                applyThumbnailBorder();
            });

            thumbnailContainer.RegisterCallback<ClickEvent>(_clickEvent =>
            {
                if (_clickEvent.clickCount == 2)
                {
                    _tracker.OpenInDefaultViewer(_item);
                }
            });

            visualMediaRow.Add(thumbnailContainer);
            cardContainer.Add(visualMediaRow);

            VisualElement detailsColumn = new VisualElement();
            detailsColumn.style.flexGrow = 1;
            detailsColumn.style.flexShrink = 1;
            detailsColumn.style.minWidth = 0;

            VisualElement nameContainer = new VisualElement();
            nameContainer.style.marginBottom = 6;

            Label nameHeaderLabel = new Label("Name:");
            nameHeaderLabel.style.fontSize = 11;
            nameHeaderLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameHeaderLabel.style.marginBottom = 2;
            nameContainer.Add(nameHeaderLabel);

            VisualElement nameFieldRow = new VisualElement();
            nameFieldRow.style.flexDirection = FlexDirection.Row;
            nameFieldRow.style.alignItems = Align.Center;

            TextField nameField = new TextField
            {
                value = _item.DesiredFileNameWithoutExtension,
                multiline = true
            };
            nameField.style.flexGrow = 1;
            nameField.style.flexShrink = 1;
            nameField.style.minWidth = 0;
            nameField.style.whiteSpace = WhiteSpace.Normal;
            nameField.RegisterValueChangedCallback(_changeEvent =>
            {
                _item.DesiredFileNameWithoutExtension = _changeEvent.newValue;
                _onDataModified?.Invoke();
            });
            nameFieldRow.Add(nameField);

            Label extensionLabel = new Label(_item.FileExtension);
            extensionLabel.style.marginLeft = 4;
            extensionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            extensionLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
            extensionLabel.style.flexShrink = 0;
            nameFieldRow.Add(extensionLabel);

            nameContainer.Add(nameFieldRow);
            detailsColumn.Add(nameContainer);

            VisualElement actionButtonsRow = new VisualElement();
            actionButtonsRow.style.flexDirection = FlexDirection.Row;
            actionButtonsRow.style.flexWrap = Wrap.Wrap;
            actionButtonsRow.style.marginBottom = 6;

            Button revealInExplorerButton = new Button(() => _tracker.RevealInExplorer(_item))
            {
                text = "📁 Reveal in Explorer"
            };
            revealInExplorerButton.style.flexGrow = 1;
            revealInExplorerButton.style.minWidth = 110;
            revealInExplorerButton.style.height = 24;
            revealInExplorerButton.style.fontSize = 10;
            revealInExplorerButton.style.backgroundColor = COLOR_BUTTON_SURFACE;
            revealInExplorerButton.style.borderLeftColor = COLOR_BUTTON_BORDER;
            revealInExplorerButton.style.borderRightColor = COLOR_BUTTON_BORDER;
            revealInExplorerButton.style.borderTopColor = COLOR_BUTTON_BORDER;
            revealInExplorerButton.style.borderBottomColor = COLOR_BUTTON_BORDER;
            revealInExplorerButton.style.marginRight = 4;
            revealInExplorerButton.style.marginBottom = 4;
            actionButtonsRow.Add(revealInExplorerButton);

            Button openInViewerButton = new Button(() => _tracker.OpenInDefaultViewer(_item))
            {
                text = "🖼️ Open in Photos"
            };
            openInViewerButton.style.flexGrow = 1;
            openInViewerButton.style.minWidth = 110;
            openInViewerButton.style.height = 24;
            openInViewerButton.style.fontSize = 10;
            openInViewerButton.style.backgroundColor = COLOR_BUTTON_SURFACE;
            openInViewerButton.style.borderLeftColor = COLOR_BUTTON_BORDER;
            openInViewerButton.style.borderRightColor = COLOR_BUTTON_BORDER;
            openInViewerButton.style.borderTopColor = COLOR_BUTTON_BORDER;
            openInViewerButton.style.borderBottomColor = COLOR_BUTTON_BORDER;
            openInViewerButton.style.marginBottom = 4;
            actionButtonsRow.Add(openInViewerButton);

            detailsColumn.Add(actionButtonsRow);

            VisualElement dividerLine = new VisualElement();
            dividerLine.style.height = 1;
            dividerLine.style.backgroundColor = COLOR_DIVIDER_LINE;
            dividerLine.style.marginTop = 2;
            dividerLine.style.marginBottom = 6;
            detailsColumn.Add(dividerLine);

            VisualElement removalActionRow = new VisualElement();
            removalActionRow.style.flexDirection = FlexDirection.Row;
            removalActionRow.style.justifyContent = Justify.SpaceBetween;
            removalActionRow.style.alignItems = Align.Center;

            Label statusLabel = new Label();
            statusLabel.style.fontSize = 11;
            statusLabel.style.flexShrink = 1;
            statusLabel.style.whiteSpace = WhiteSpace.Normal;
            removalActionRow.Add(statusLabel);

            Button toggleKeepButton = new Button();
            toggleKeepButton.style.width = 105;
            toggleKeepButton.style.height = 24;
            toggleKeepButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            toggleKeepButton.style.flexShrink = 0;

            Action updateVisualState = () =>
            {
                if (_item.IsMarkedToKeep)
                {
                    cardContainer.style.backgroundColor = COLOR_CARD_BACKGROUND_KEEP;
                    cardContainer.style.borderLeftColor = COLOR_CARD_BORDER_KEEP;
                    cardContainer.style.borderRightColor = COLOR_CARD_BORDER_KEEP;
                    cardContainer.style.borderTopColor = COLOR_CARD_BORDER_KEEP;
                    cardContainer.style.borderBottomColor = COLOR_CARD_BORDER_KEEP;

                    statusLabel.text = "Marked to Keep";
                    statusLabel.style.color = new Color(0.65f, 0.90f, 0.68f);

                    toggleKeepButton.text = "Don't Keep";
                    toggleKeepButton.style.backgroundColor = new Color(0.35f, 0.22f, 0.22f);
                    toggleKeepButton.style.color = Color.white;
                }
                else
                {
                    cardContainer.style.backgroundColor = COLOR_CARD_BACKGROUND_NORMAL;
                    cardContainer.style.borderLeftColor = COLOR_CARD_BORDER_NORMAL;
                    cardContainer.style.borderRightColor = COLOR_CARD_BORDER_NORMAL;
                    cardContainer.style.borderTopColor = COLOR_CARD_BORDER_NORMAL;
                    cardContainer.style.borderBottomColor = COLOR_CARD_BORDER_NORMAL;

                    statusLabel.text = "Unmarked (Delete)";
                    statusLabel.style.color = new Color(0.85f, 0.85f, 0.85f);

                    toggleKeepButton.text = "Keep Image";
                    toggleKeepButton.style.backgroundColor = new Color(0.2f, 0.44f, 0.22f);
                    toggleKeepButton.style.color = Color.white;
                }

                applyThumbnailBorder();

                if (deleteUnmarkedButton != null)
                {
                    deleteUnmarkedButton.text = $"🗑️ Delete Unmarked ({_tracker.UnmarkedCount})";
                    deleteUnmarkedButton.SetEnabled(_tracker.UnmarkedCount > 0);
                }
            };

            toggleKeepButton.clicked += () =>
            {
                _item.IsMarkedToKeep = !_item.IsMarkedToKeep;
                updateVisualState();
                UpdateSummaryText(_tracker);
                _onDataModified?.Invoke();
            };

            updateVisualState();
            removalActionRow.Add(toggleKeepButton);
            detailsColumn.Add(removalActionRow);

            cardContainer.Add(detailsColumn);

            cardContainer.RegisterCallback<GeometryChangedEvent>(_geometryEvent =>
            {
                float currentWidth = _geometryEvent.newRect.width;
                if (currentWidth <= 0)
                {
                    return;
                }

                if (currentWidth < NARROW_BREAKPOINT_WIDTH)
                {
                    cardContainer.style.flexDirection = FlexDirection.Column;
                    thumbnailContainer.style.marginRight = 0;
                    thumbnailContainer.style.marginBottom = 8;
                    thumbnailContainer.style.alignSelf = Align.Center;
                    detailsColumn.style.marginLeft = 0;
                }
                else
                {
                    cardContainer.style.flexDirection = FlexDirection.Row;
                    thumbnailContainer.style.marginRight = 10;
                    thumbnailContainer.style.marginBottom = 0;
                    thumbnailContainer.style.alignSelf = Align.FlexStart;
                    detailsColumn.style.marginLeft = 0;
                }
            });

            return cardContainer;
        }

        private void UpdateSummaryText(ScreenshotSessionTracker _sessionTracker)
        {
            int total = _sessionTracker.TotalCapturedCount;
            int kept = _sessionTracker.KeptCount;
            int unmarked = _sessionTracker.UnmarkedCount;

            sessionSummaryLabel.text = $"Session: {total} taken | {kept} kept | {unmarked} to delete";
        }
    }
}