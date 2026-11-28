using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>"All set": one amber card for each menu still to be finished by hand, or one plain card when nothing is left.
    /// Also says what was renamed or skipped, since that is only known now.</summary>
    internal class OnboardingAllSetPage : OnboardingPage
    {
        internal override string Template
        {
            get { return "OnboardingAllSet"; }
        }

        internal override string NextText
        {
            get { return "Finish"; }
        }

        // The menus are made; going back would only offer to make them again.
        internal override bool BackAllowed
        {
            get { return false; }
        }

        internal override void Wire(VisualElement _root, OnboardingContext _context)
        {
            VisualElement container = _root.Q<VisualElement>("allSetContainer");
            OnboardingResult result = _context.Result;
            if (result.ToDo.Count == 0)
            {
                bool madeMenus = _context.Choices.NewMenus().Count > 0;
                container.Add(PlainCard(madeMenus ? "Nothing left: every menu you picked is copied from the library." : "Nothing left: no menu was picked, so only the Play state was made."));
            }
            foreach (OnboardingToDo item in result.ToDo)
            {
                container.Add(ToDoCard(item));
            }
            foreach (string note in result.Renamed)
            {
                container.Add(Warning(note));
            }
            foreach (string note in result.Skipped)
            {
                container.Add(Warning(note));
            }
        }

        private static VisualElement PlainCard(string _text)
        {
            VisualElement card = new VisualElement();
            card.AddToClassList("gm-card");
            card.AddToClassList("gm-card-flat");
            Label text = new Label(_text);
            text.AddToClassList("gm-note-text");
            card.Add(text);
            return card;
        }

        private static VisualElement ToDoCard(OnboardingToDo _item)
        {
            VisualElement card = new VisualElement();
            card.AddToClassList("gm-card");
            card.AddToClassList("gm-all-set-card");

            VisualElement tagRow = new VisualElement();
            tagRow.AddToClassList("gm-all-set-tag-row");
            VisualElement dot = new VisualElement();
            dot.AddToClassList("gm-all-set-tag-dot");
            tagRow.Add(dot);
            Label tag = new Label("TO DO");
            tag.AddToClassList("gm-all-set-tag");
            tagRow.Add(tag);
            card.Add(tagRow);

            Label name = new Label(_item.MenuName);
            name.AddToClassList("gm-all-set-name");
            name.style.whiteSpace = WhiteSpace.Normal;
            card.Add(name);
            Label text = new Label(_item.Text);
            text.AddToClassList("gm-note-text");
            card.Add(text);

            Button learnMore = new Button(GameManagementSettingsProvider.OpenReadMe);
            learnMore.text = "Learn more";
            learnMore.AddToClassList("gm-link");
            learnMore.AddToClassList("gm-inline-link");
            card.Add(learnMore);
            return card;
        }

        private static VisualElement Warning(string _text)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("gm-warning-row");
            row.Add(new Label(_text));
            return row;
        }
    }
}
