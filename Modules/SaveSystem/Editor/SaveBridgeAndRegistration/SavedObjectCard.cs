using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// One saved script's card: a Foldout showing its saved fields or custom data, live values in play mode, and
    /// its save ID behind a button. Built from a SavedObjectDescription. Used by the bridge inspector (T1), the
    /// mistake HelpBoxes (T2) and the Saved Objects window (T5).
    /// </summary>
    internal sealed class SavedObjectCard : VisualElement
    {
        const string StyleSheetPath = "Packages/com.abstractpixel.library/Modules/SaveSystem/Editor/SaveBridgeAndRegistration/SavedObjectCard.uss";
        const int MaxValueLength = 60;

        // Tests can't reliably enter real Play Mode here: the Lab's Enter Play Mode Settings reload the scene,
        // which would destroy the test's own GameObjects before a card could read their live values. Null in
        // normal use, where the card asks the Editor directly.
        internal static bool? IsPlayingOverrideForTests;
        static bool IsPlaying => IsPlayingOverrideForTests ?? EditorApplication.isPlaying;

        readonly Foldout foldout;
        readonly VisualElement body;
        readonly Label errorLabel;
        readonly HelpBox waitingHelpBox;
        readonly List<(Label Label, SavedObjectDescription.FieldRow Row)> fieldValueLabels = new List<(Label, SavedObjectDescription.FieldRow)>();

        Label customDataValueLabel;
        Button showSaveIdButton;
        VisualElement revealedSaveIdRow;
        Label saveIdLabel;
        SavedObjectDescription description;

        public SavedObjectCard()
        {
            AddToClassList("saved-object-card");
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }

            foldout = new Foldout();
            foldout.RegisterValueChangedCallback(_ => RefreshLiveValues());
            Add(foldout);

            body = new VisualElement();
            body.AddToClassList("saved-object-card__body");
            foldout.Add(body);

            errorLabel = new Label();
            errorLabel.AddToClassList("saved-object-card__error");
            errorLabel.style.display = DisplayStyle.None;

            waitingHelpBox = new HelpBox(
                "Not saved: this copy was spawned with no SetSaveId call. Call SetSaveId right after Instantiate.",
                HelpBoxMessageType.Warning);
            waitingHelpBox.style.display = DisplayStyle.None;

            schedule.Execute(RefreshLiveValues).Every(500);
        }

        /// <summary>Fills the card from a description. initiallyOpen is the fallback the Foldout uses with no remembered state.</summary>
        public void Bind(SavedObjectDescription newDescription, bool initiallyOpen)
        {
            description = newDescription;
            fieldValueLabels.Clear();
            body.Clear();

            foldout.text = description.HeaderText;
            foldout.viewDataKey = $"AbstractPixel.SaveSystem.SavedObjectCard.{description.ViewDataKey}";
            foldout.value = initiallyOpen;

            if (description.FieldRows != null)
            {
                foreach (SavedObjectDescription.FieldRow row in description.FieldRows)
                {
                    body.Add(BuildFieldRow(row));
                }
            }
            else
            {
                body.Add(new Label($"Custom data: {description.CustomDataTypeName}"));
                customDataValueLabel = new Label();
                customDataValueLabel.AddToClassList("saved-object-card__custom-data-value");
                customDataValueLabel.style.display = DisplayStyle.None;
                body.Add(customDataValueLabel);
            }

            body.Add(errorLabel);
            body.Add(waitingHelpBox);
            BuildSaveIdArea();

            RefreshLiveValues();
        }

        VisualElement BuildFieldRow(SavedObjectDescription.FieldRow row)
        {
            VisualElement container = new VisualElement();
            container.AddToClassList("saved-object-card__field-row");

            Label nameAndType = new Label($"{row.Name} ({row.ShortTypeName})");
            nameAndType.AddToClassList("saved-object-card__field-name");
            container.Add(nameAndType);

            if (row.FormerlyName != null)
            {
                Label formerly = new Label($"was: {row.FormerlyName}");
                formerly.AddToClassList("saved-object-card__field-formerly");
                container.Add(formerly);
            }

            Label value = new Label();
            value.AddToClassList("saved-object-card__field-value");
            value.style.display = DisplayStyle.None;
            container.Add(value);
            fieldValueLabels.Add((value, row));

            return container;
        }

        void BuildSaveIdArea()
        {
            if (description.ShowsSpawnedCopyNote)
            {
                Label note = new Label("Spawned copies get their save ID from SetSaveId");
                note.AddToClassList("saved-object-card__note");
                body.Add(note);
                return;
            }

            showSaveIdButton = new Button(ToggleSaveIdReveal) { text = "Show save ID" };
            body.Add(showSaveIdButton);

            revealedSaveIdRow = new VisualElement();
            revealedSaveIdRow.AddToClassList("saved-object-card__save-id-row");
            revealedSaveIdRow.style.display = DisplayStyle.None;

            saveIdLabel = new Label();
            saveIdLabel.AddToClassList("saved-object-card__save-id-label");
            revealedSaveIdRow.Add(saveIdLabel);

            revealedSaveIdRow.Add(new Button(() => EditorGUIUtility.systemCopyBuffer = saveIdLabel.text) { text = "Copy save ID" });
            body.Add(revealedSaveIdRow);
        }

        void ToggleSaveIdReveal()
        {
            bool willReveal = revealedSaveIdRow.style.display == DisplayStyle.None;
            revealedSaveIdRow.style.display = willReveal ? DisplayStyle.Flex : DisplayStyle.None;
            showSaveIdButton.text = willReveal ? "Hide save ID" : "Show save ID";
            UpdateRevealedSaveId();
        }

        void UpdateRevealedSaveId()
        {
            if (saveIdLabel == null || revealedSaveIdRow.style.display == DisplayStyle.None)
            {
                return;
            }
            saveIdLabel.text = description.GetSaveId();
        }

        void RefreshLiveValues()
        {
            if (description == null)
            {
                return;
            }

            UpdateRevealedSaveId();
            waitingHelpBox.style.display = description.IsWaitingForSetSaveId && IsPlaying ? DisplayStyle.Flex : DisplayStyle.None;

            if (!foldout.value || !IsPlaying)
            {
                errorLabel.style.display = DisplayStyle.None;
                HideLiveValues();
                return;
            }

            try
            {
                JToken captured = description.CaptureLiveValues();
                errorLabel.style.display = DisplayStyle.None;
                if (description.FieldRows != null)
                {
                    ApplyFieldValues(captured as JObject);
                }
                else
                {
                    customDataValueLabel.text = captured.ToString(Newtonsoft.Json.Formatting.Indented);
                    customDataValueLabel.style.display = DisplayStyle.Flex;
                }
            }
            catch (Exception exception)
            {
                HideLiveValues();
                errorLabel.text = $"Couldn't read the values: {exception.Message}";
                errorLabel.style.display = DisplayStyle.Flex;
            }
        }

        void ApplyFieldValues(JObject data)
        {
            foreach ((Label label, SavedObjectDescription.FieldRow row) in fieldValueLabels)
            {
                JToken token = data?[row.Name];
                if (token == null)
                {
                    label.style.display = DisplayStyle.None;
                    continue;
                }
                string raw = token.ToString(Newtonsoft.Json.Formatting.None);
                label.text = $"= {Truncate(raw)}";
                label.tooltip = raw;
                label.style.display = DisplayStyle.Flex;
            }
        }

        void HideLiveValues()
        {
            foreach ((Label label, _) in fieldValueLabels)
            {
                label.style.display = DisplayStyle.None;
            }
            if (customDataValueLabel != null)
            {
                customDataValueLabel.style.display = DisplayStyle.None;
            }
        }

        static string Truncate(string text)
        {
            return text.Length <= MaxValueLength ? text : text.Substring(0, MaxValueLength) + "…";
        }
    }
}
