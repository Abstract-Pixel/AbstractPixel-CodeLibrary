using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Every saved object that registered while the game runs, grouped by save file, each as the same card the
    /// bridge inspector draws. Read only: no Reset, Forget or Open save folder buttons.
    /// </summary>
    internal sealed class SavedObjectsWindow : EditorWindow, IHasCustomMenu
    {
        const string GuideSection = "Pages/debug.html#saved-objects";
        const string StyleSheetPath = "Packages/com.abstractpixel.library/Modules/SaveSystem/Editor/SavedObjectsWindow/SavedObjectsWindow.uss";
        internal const string EditModeMessage = "Enter play mode to see every saved object that registered.";
        internal const string NothingRegisteredMessage = "No saved objects have registered yet.";

        VisualElement content;
        string shownSignature;

        [MenuItem("Tools/External Project Tools/Save System/Saved Objects")]
        static void Open()
        {
            GetWindow<SavedObjectsWindow>("Saved Objects");
        }

        // Unity calls this to draw a small button in the window's tab bar, next to its menu, where its own
        // windows put their help and lock buttons.
        void ShowButton(Rect position)
        {
            GUIContent help = EditorGUIUtility.IconContent("_Help");
            help.tooltip = "Open the save system guide";
            if (GUI.Button(position, help, GUIStyle.none))
            {
                SaveSystemGuide.Open(GuideSection);
            }
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Open Guide"), false, () => SaveSystemGuide.Open(GuideSection));
        }

        void CreateGUI()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }

            ScrollView scroll = new ScrollView();
            content = new VisualElement();
            scroll.Add(content);
            rootVisualElement.Add(scroll);

            rootVisualElement.schedule.Execute(() => Refresh(false)).Every(500);
            Refresh(true);
        }

        void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (content != null)
            {
                Refresh(true);
            }
        }

        // Rebuilds only when something registered or unregistered, so an open card is not redrawn every tick.
        void Refresh(bool force)
        {
            List<SavedObjectEntry> entries = CurrentEntries();
            string signature = entries == null ? "edit mode" : SavedObjectGrouping.Signature(entries);
            if (!force && signature == shownSignature)
            {
                return;
            }

            shownSignature = signature;
            content.Clear();
            content.Add(entries == null ? BuildMessage(EditModeMessage) : BuildContent(entries));
        }

        static List<SavedObjectEntry> CurrentEntries()
        {
            if (!EditorApplication.isPlaying || SaveActions.Session == null)
            {
                return null;
            }
            return SaveActions.Session.SavedObjects.AllEntries.ToList();
        }

        /// <summary>The window's list for these entries: counted headers, a card for each saved object.</summary>
        internal static VisualElement BuildContent(IEnumerable<SavedObjectEntry> entries)
        {
            VisualElement list = new VisualElement();
            List<SavedObjectGroup> groups = SavedObjectGrouping.Group(entries);
            if (groups.Count == 0)
            {
                list.Add(BuildMessage(NothingRegisteredMessage));
                return list;
            }

            SaveJson serializer = new SaveJson();
            foreach (SavedObjectGroup group in groups)
            {
                list.Add(BuildGroup(group, group.Title, serializer));
            }
            return list;
        }

        static VisualElement BuildGroup(SavedObjectGroup group, string path, SaveJson serializer)
        {
            Foldout foldout = new Foldout { text = $"{group.Title} ({group.Count})", value = true };
            foldout.viewDataKey = $"AbstractPixel.SaveSystem.SavedObjectsWindow.{path}";
            foldout.AddToClassList("saved-objects-window__group");

            foreach (SavedObjectGroup child in group.Children)
            {
                foldout.Add(BuildGroup(child, $"{path}/{child.Title}", serializer));
            }
            foreach (SavedObjectEntry entry in group.Entries)
            {
                SavedObjectCard card = new SavedObjectCard();
                card.Bind(SavedObjectDescription.ForEntry(entry, serializer), false);
                foldout.Add(card);
            }
            return foldout;
        }

        static Label BuildMessage(string text)
        {
            Label message = new Label(text);
            message.AddToClassList("saved-objects-window__message");
            return message;
        }
    }
}
