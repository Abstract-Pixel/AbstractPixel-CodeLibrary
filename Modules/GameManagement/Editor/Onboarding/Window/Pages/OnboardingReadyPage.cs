using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>"Ready": what will be made, as numbered points, with a note for every name that is taken. "Set up" makes it.</summary>
    internal class OnboardingReadyPage : OnboardingPage
    {
        private VisualElement errorHost;

        internal override string Template
        {
            get { return "OnboardingReady"; }
        }

        internal override string NextText
        {
            get { return "Set up"; }
        }

        internal override void Wire(VisualElement _root, OnboardingContext _context)
        {
            VisualElement container = _root.Q<VisualElement>("readyContainer");
            List<string> lines = OnboardingPlan.ReadyLines(_context.Choices, _context.PlayExists);
            for (int index = 0; index < lines.Count; index++)
            {
                VisualElement row = Row(index + 1, lines[index]);
                row.style.marginTop = index == 0 ? 0 : 9;
                container.Add(row);
            }
            VisualElement card = _root.Q<VisualElement>("readyCard");
            foreach (string note in OnboardingClashes.Find(_context.Choices, _context.Folder, _context.PlayExists))
            {
                card.parent.Insert(card.parent.IndexOf(card) + 1, Warning(note));
            }
            errorHost = new VisualElement();
            card.parent.Insert(card.parent.IndexOf(card) + 1, errorHost);
        }

        // Makes everything. A failure deletes whatever the run made and shows what to fix, staying on this page.
        internal override bool Leave(VisualElement _root, OnboardingContext _context)
        {
            errorHost.Clear();
            try
            {
                _context.Result = OnboardingBuilder.Run(_context.Choices);
                return true;
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                HelpBox box = new HelpBox("Nothing was made. " + exception.Message, HelpBoxMessageType.Error);
                errorHost.Add(box);
                return false;
            }
        }

        private static VisualElement Row(int _number, string _text)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("gm-ready-row");
            Label number = new Label(_number + ".");
            number.AddToClassList("gm-ready-index");
            row.Add(number);
            Label text = new Label(_text);
            text.AddToClassList("gm-ready-text");
            row.Add(text);
            return row;
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
