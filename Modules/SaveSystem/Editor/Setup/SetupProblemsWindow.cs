using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>Lists every setup problem SetupPlayModeGuard blocked play mode on, each with its own fix.</summary>
    internal sealed class SetupProblemsWindow : EditorWindow
    {
        const string GuidePage = "Pages/problems.html";
        const string StyleSheetPath = "Packages/com.abstractpixel.library/Modules/SaveSystem/Editor/Setup/SetupProblemsWindow.uss";

        IReadOnlyList<SetupProblem> problems;
        VisualElement rowsContainer;

        public static void Open(IReadOnlyList<SetupProblem> problemsToShow)
        {
            SetupProblemsWindow window = CreateInstance<SetupProblemsWindow>();
            window.titleContent = new GUIContent("Save System: fix before playing");
            window.problems = problemsToShow;
            window.ShowUtility();
            window.Focus();
        }

        void CreateGUI()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }

            ScrollView scroll = new ScrollView();
            rowsContainer = new VisualElement();
            scroll.Add(rowsContainer);
            rootVisualElement.Add(scroll);

            VisualElement buttonRow = new VisualElement();
            buttonRow.AddToClassList("setup-problems-window__button-row");
            buttonRow.Add(new Button(CheckAgain) { text = "Check again" });
            buttonRow.Add(new Button(() => SaveSystemGuide.Open(GuidePage)) { text = "Open guide" });
            rootVisualElement.Add(buttonRow);

            Rebuild();
        }

        void Rebuild()
        {
            rowsContainer.Clear();
            foreach (SetupProblem problem in problems)
            {
                rowsContainer.Add(BuildRow(problem));
            }
        }

        // Re-runs the real check (not just the problems this window opened with), so a fix made while it was open counts.
        void CheckAgain()
        {
            SetupCheck.Forget();
            problems = SetupCheck.Problems;
            if (!SetupPlayModeGuard.ShouldBlockPlay(problems))
            {
                Close();
                return;
            }
            Rebuild();
        }

        static VisualElement BuildRow(SetupProblem problem)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("setup-problems-window__row");

            VisualElement header = new VisualElement();
            header.AddToClassList("setup-problems-window__row-header");
            header.Add(BuildScriptButton(problem));
            if (problem.Target != null)
            {
                UnityEngine.Object target = problem.Target;
                header.Add(new Button(() => EditorGUIUtility.PingObject(target)) { text = "Ping" });
            }
            row.Add(header);

            row.Add(new Label(problem.WhatIsWrong));
            row.Add(new Label(problem.HowToFix));
            return row;
        }

        static VisualElement BuildScriptButton(SetupProblem problem)
        {
            if (problem.ScriptType == null)
            {
                return new Label("Save System");
            }
            Type scriptType = problem.ScriptType;
            return new Button(() => OpenScript(scriptType)) { text = scriptType.Name };
        }

        static void OpenScript(Type scriptType)
        {
            UnityEngine.Object scriptAsset = SetupCheck.FindScriptAsset(scriptType);
            if (scriptAsset != null)
            {
                AssetDatabase.OpenAsset(scriptAsset);
            }
        }
    }
}
