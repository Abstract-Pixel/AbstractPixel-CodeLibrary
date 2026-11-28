using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>
    /// The setup window. A project with no settings sees seven pages; a project that has them (a run later) sees four, starting at
    /// "Which menus?", with the menus it already has ticked and locked. Nothing is made until "Set up" on the Ready page.
    /// </summary>
    public class OnboardingWindow : EditorWindow
    {
        public const string MenuPath = "Tools/External Project Tools/Game Management/Set Up Game Management";

        private List<OnboardingPage> pages;
        private OnboardingContext context;
        private int pageIndex;

        /// <summary>Opens the window: the full set-up in a new project, or "Which menus?" onward once settings exist.</summary>
        [MenuItem(MenuPath)]
        public static void Open()
        {
            OnboardingWindow window = GetWindow<OnboardingWindow>(true, "Set up Game Management", true);
            window.minSize = new Vector2(300, 360);
            Rect position = window.position;
            if (position.width < 440 || position.height < 460)
            {
                window.position = new Rect(position.x, position.y, 620, 640);
            }
            window.Restart();
            window.Show();
        }

        private void CreateGUI()
        {
            Restart();
        }

        // Built again on every Open, so a window left open from before shows the right pages for the project as it is now.
        private void Restart()
        {
            GameManagementSettings settings = GameManagementSettingsAssets.FindProjectSettings();
            bool settingsExist = settings != null && settings.Catalog != null;
            context = new OnboardingContext();
            context.Choices = settingsExist ? OnboardingChoices.ForExistingSettings(settings) : new OnboardingChoices();
            context.Folder = settingsExist ? System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(settings)).Replace('\\', '/') : OnboardingChoices.CleanFolder(context.Choices.SettingsFolder);
            context.PlayExists = settingsExist && OnboardingStateMaker.FindListed(settings.Catalog, OnboardingMenuInfo.PlayStateName) != null;
            context.Changed = RefreshFooter;
            context.Close = Close;
            pages = new List<OnboardingPage>();
            if (!settingsExist)
            {
                pages.Add(new OnboardingWelcomePage());
                pages.Add(new OnboardingFolderPage());
            }
            pages.Add(new OnboardingMenusPage());
            pages.Add(new OnboardingPathsPage());
            if (!settingsExist)
            {
                pages.Add(new OnboardingInputPage());
            }
            pages.Add(new OnboardingReadyPage());
            pages.Add(new OnboardingAllSetPage());
            pageIndex = 0;
            ShowPage();
        }

        private void ShowPage()
        {
            rootVisualElement.Clear();
            OnboardingPage page = pages[pageIndex];
            context.CanContinue = true;
            OnboardingPageLoader.Load(rootVisualElement, page.Template, page is OnboardingWelcomePage);
            page.Wire(rootVisualElement, context);

            Label step = rootVisualElement.Q<Label>("stepLabel");
            if (step != null)
            {
                step.text = "Step " + (pageIndex + 1) + " of " + pages.Count;
            }
            Button back = rootVisualElement.Q<Button>("backButton");
            Button next = rootVisualElement.Q<Button>("nextButton");
            next.text = page.NextText;
            back.clicked += GoBack;
            next.clicked += GoNext;
            RefreshFooter();
        }

        private void RefreshFooter()
        {
            Button back = rootVisualElement.Q<Button>("backButton");
            Button next = rootVisualElement.Q<Button>("nextButton");
            if (back == null || next == null)
            {
                return;
            }
            back.SetEnabled(pageIndex > 0 && pages[pageIndex].BackAllowed);
            next.SetEnabled(context.CanContinue);
        }

        private void GoBack()
        {
            if (pageIndex > 0)
            {
                pageIndex--;
                ShowPage();
            }
        }

        private void GoNext()
        {
            OnboardingPage page = pages[pageIndex];
            if (!page.Leave(rootVisualElement, context))
            {
                return;
            }
            if (pageIndex < pages.Count - 1)
            {
                pageIndex++;
                ShowPage();
                return;
            }
            Close();
        }
    }
}
