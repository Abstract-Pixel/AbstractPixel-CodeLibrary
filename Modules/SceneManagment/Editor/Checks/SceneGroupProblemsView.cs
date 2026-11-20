using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// A scene group's problems as Unity help boxes, each with its one-click fix. Hidden when the group has none.
    /// Used by the scene group inspector and the Scene System page's Scene Groups Debugger.
    /// </summary>
    public class SceneGroupProblemsView : VisualElement
    {
        private readonly bool showTitle;

        public SceneGroupProblemsView(bool _showTitle)
        {
            showTitle = _showTitle;
        }

        /// <summary>Shows the group's problems. &lt;paramref name="_afterFix"/&gt; runs after a fix button was clicked.</summary>
        public int Refresh(SceneGroup _group, Action _afterFix)
        {
            Clear();
            List<SceneGroupProblem> problems = SceneGroupCheck.Find(_group);
            bool hasProblems = problems.Count > 0;
            style.display = hasProblems ? DisplayStyle.Flex : DisplayStyle.None;
            if (!hasProblems)
            {
                return 0;
            }

            if (showTitle)
            {
                Label title = new Label("Problems");
                title.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
                title.style.marginTop = 4;
                title.style.marginLeft = 3;
                Add(title);
            }
            foreach (SceneGroupProblem problem in problems)
            {
                Add(MakeRow(_group, problem, _afterFix));
            }
            return problems.Count;
        }

        private static VisualElement MakeRow(SceneGroup _group, SceneGroupProblem _problem, Action _afterFix)
        {
            // Every problem stops a build with the pre-build check on, a scene listed twice included (decision Q5).
            HelpBox box = new HelpBox(_problem.Message, HelpBoxMessageType.Error);
            box.style.alignItems = Align.Center;
            if (!_problem.HasFix)
            {
                return box;
            }

            Button fixButton = new Button(() =>
            {
                SceneGroupProblemFixer.Apply(_group, _problem);
                if (_afterFix != null)
                {
                    _afterFix();
                }
            });
            fixButton.text = SceneGroupProblemFixer.FixLabel(_problem.Fix);
            fixButton.tooltip = SceneGroupProblemFixer.FixTooltip(_problem.Fix);
            fixButton.style.flexShrink = 0;
            box.Add(fixButton);
            return box;
        }
    }
}
