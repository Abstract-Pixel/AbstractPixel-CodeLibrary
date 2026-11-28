using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Page 2: where the settings go. Checked as you type; Next stays off until the folder is inside Assets.</summary>
    internal class OnboardingFolderPage : OnboardingPage
    {
        internal override string Template
        {
            get { return "OnboardingSettingsFolder"; }
        }

        internal override void Wire(VisualElement _root, OnboardingContext _context)
        {
            TextField field = _root.Q<TextField>("folderPath");
            field.SetValueWithoutNotify(_context.Choices.SettingsFolder);
            Update(_root, _context, field.value);
            field.RegisterValueChangedCallback(_change =>
            {
                _context.Choices.SettingsFolder = _change.newValue;
                Update(_root, _context, _change.newValue);
            });
            _root.Q<Button>("browseFolderButton").clicked += () =>
            {
                string chosen = EditorUtility.OpenFolderPanel("Choose a settings folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(chosen))
                {
                    field.value = ToProjectPath(chosen);
                }
            };
        }

        private static string ToProjectPath(string _absolutePath)
        {
            string normalized = _absolutePath.Replace('\\', '/');
            string relative = FileUtil.GetProjectRelativePath(normalized);
            return string.IsNullOrEmpty(relative) ? normalized : relative;
        }

        private static void Update(VisualElement _root, OnboardingContext _context, string _folder)
        {
            string problem = OnboardingChoices.FolderProblem(_folder);
            bool good = problem == null;
            VisualElement dot = _root.Q<VisualElement>("folderStatusDot");
            dot.EnableInClassList("gm-dot-green", good);
            dot.EnableInClassList("gm-dot-red", !good);
            Label label = _root.Q<Label>("folderStatusLabel");
            label.text = good ? "Looks good." : problem;
            label.style.whiteSpace = WhiteSpace.Normal;
            _root.Q<TextField>("folderPath").tooltip = _folder;
            Label full = _root.Q<Label>("folderFull");
            full.text = good ? "Settings go in " + OnboardingChoices.CleanFolder(_folder) + "/" : "";
            full.tooltip = _folder;
            if (good)
            {
                _context.Folder = OnboardingChoices.CleanFolder(_folder);
            }
            _context.CanContinue = good;
            _context.Changed?.Invoke();
        }
    }
}
