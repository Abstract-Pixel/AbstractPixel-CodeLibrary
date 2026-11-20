using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Lists every scene group in the project with its problems and their one-click fixes, plus Select and Open group.
    /// </summary>
    public class CheckAllSceneGroupsWindow : EditorWindow
    {
        public const string MenuPath = "Tools/External Project Tools/Scene System/Check All Scene Groups";

        private Label summary;
        private ScrollView groupList;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            CheckAllSceneGroupsWindow window = GetWindow<CheckAllSceneGroupsWindow>();
            window.titleContent = new GUIContent("Check All Scene Groups");
            window.minSize = new Vector2(420, 240);
            window.Show();
        }

        private void OnEnable()
        {
            EditorBuildSettings.sceneListChanged += Refresh;
        }

        private void OnDisable()
        {
            EditorBuildSettings.sceneListChanged -= Refresh;
        }

        private void OnFocus()
        {
            Refresh();
        }

        public void CreateGUI()
        {
            VisualElement toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.alignItems = Align.Center;
            toolbar.style.marginTop = 4;
            toolbar.style.marginLeft = 4;
            toolbar.style.marginRight = 4;

            summary = new Label();
            summary.style.flexGrow = 1;
            summary.style.unityFontStyleAndWeight = FontStyle.Bold;
            toolbar.Add(summary);

            Button checkAgain = new Button(Refresh);
            checkAgain.text = "Check again";
            checkAgain.tooltip = "Checks every scene group in the project again.";
            toolbar.Add(checkAgain);
            rootVisualElement.Add(toolbar);

            groupList = new ScrollView();
            groupList.style.flexGrow = 1;
            groupList.style.marginTop = 4;
            rootVisualElement.Add(groupList);
            Refresh();
        }

        private void Refresh()
        {
            if (groupList == null)
            {
                return;
            }
            groupList.Clear();
            List<SceneGroup> groups = SceneGroupAssets.FindAll();
            int problemCount = 0;
            int groupsWithProblems = 0;
            foreach (SceneGroup group in groups)
            {
                int groupProblems = AddGroupRow(group);
                problemCount += groupProblems;
                if (groupProblems > 0)
                {
                    groupsWithProblems++;
                }
            }
            summary.text = groups.Count + " scene groups, " + problemCount + " problems in " + groupsWithProblems + " groups";
        }

        private int AddGroupRow(SceneGroup _group)
        {
            VisualElement row = new VisualElement();
            row.style.marginLeft = 4;
            row.style.marginRight = 4;
            row.style.marginBottom = 6;

            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;

            Label name = new Label(_group.name);
            name.tooltip = AssetDatabase.GetAssetPath(_group);
            name.style.flexGrow = 1;
            name.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(name);

            Button select = new Button(() => SelectGroup(_group));
            select.text = "Select";
            select.tooltip = "Selects this scene group in the Project window and shows it in the Inspector.";
            header.Add(select);

            Button open = new Button(() => SceneGroupOpener.Open(_group));
            open.text = "Open group";
            open.tooltip = "Opens the main scene with its manager scenes and shared scenes, as they are in Play.";
            header.Add(open);
            row.Add(header);

            SceneGroupProblemsView problemsView = new SceneGroupProblemsView(false);
            int problemCount = problemsView.Refresh(_group, Refresh);
            if (problemCount == 0)
            {
                Label fine = new Label("No problems.");
                fine.style.marginLeft = 3;
                fine.style.color = new Color(0.6f, 0.6f, 0.6f);
                row.Add(fine);
            }
            row.Add(problemsView);
            groupList.Add(row);
            return problemCount;
        }

        private static void SelectGroup(SceneGroup _group)
        {
            Selection.activeObject = _group;
            EditorGUIUtility.PingObject(_group);
        }
    }
}
