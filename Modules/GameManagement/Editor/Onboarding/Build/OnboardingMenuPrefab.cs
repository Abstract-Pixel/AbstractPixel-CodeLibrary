using System.Collections.Generic;
using System.Linq;
using AbstractPixel.Core;
using AbstractPixel.Core.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Edits one open menu prefab: puts its parts under a Content object the reactor can switch, and points each button at a state.</summary>
    internal static class OnboardingMenuPrefab
    {
        private const float TopButtonY = 83f;
        private const float ButtonSpacing = 140f;

        /// <summary>Adds a State Menu Reactor to the root of the developer's own prefab, showing its first child.</summary>
        internal static void AddReactor(GameObject _root, StateSO _state)
        {
            StateMenuReactor reactor = _root.GetComponent<StateMenuReactor>();
            if (reactor == null)
            {
                reactor = _root.AddComponent<StateMenuReactor>();
            }
            GameObject content = _root.transform.childCount > 0 ? _root.transform.GetChild(0).gameObject : null;
            SetReactor(reactor, _state, content);
        }

        /// <summary>Makes a library copy work: reactor, Content, auto-select for gamepads and every button wired or removed.</summary>
        internal static void Wire(GameObject _root, OnboardingMenuWiring _wiring)
        {
            // The library's menus are saved switched off at the root. The reactor lives on the root, so it must run.
            _root.SetActive(true);
            Transform content = WrapInContent(_root);
            Transform buttonParent = content.Find("Buttons");
            if (_wiring.IsSettingsMenu)
            {
                WireSettingsMenu(_root, buttonParent, _wiring.State);
            }
            else
            {
                WirePlainMenu(_root, buttonParent, _wiring);
            }
            SetReactor(_root.AddComponent<StateMenuReactor>(), _wiring.State, content.gameObject);

            Button first = buttonParent.GetComponentsInChildren<Button>(true).OrderByDescending(_button => ((RectTransform)_button.transform).anchoredPosition.y).First();
            // The library's own gamepad support: selects the first button when a controller is in use and steps through the column.
            AutoSpatialNavigation navigation = content.gameObject.AddComponent<AutoSpatialNavigation>();
            SerializedObject navigationData = new SerializedObject(navigation);
            navigationData.FindProperty("firstSelectedOverride").objectReferenceValue = first;
            navigationData.FindProperty("navigationAxis").enumValueIndex = (int)SpatialNavigationAxis.VerticalList;
            navigationData.ApplyModifiedPropertiesWithoutUndo();
            // A keyboard has no highlight to start from, so letting go of an arrow key picks the same first button.
            SelectOnKeyboardNavigation keyboardSelect = content.gameObject.AddComponent<SelectOnKeyboardNavigation>();
            SerializedObject keyboardData = new SerializedObject(keyboardSelect);
            keyboardData.FindProperty("firstSelected").objectReferenceValue = first;
            keyboardData.ApplyModifiedPropertiesWithoutUndo();
        }

        // The reactor switches its Content on and off, so the reactor itself must sit outside it.
        private static Transform WrapInContent(GameObject _root)
        {
            GameObject made = new GameObject("Content", typeof(RectTransform));
            RectTransform content = (RectTransform)made.transform;
            content.SetParent(_root.transform, false);
            content.anchorMin = Vector2.zero;
            content.anchorMax = Vector2.one;
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            List<Transform> parts = new List<Transform>();
            foreach (Transform child in _root.transform)
            {
                if (child != content)
                {
                    parts.Add(child);
                }
            }
            foreach (Transform part in parts)
            {
                part.SetParent(content, false);
            }
            return content;
        }

        private static void SetReactor(StateMenuReactor _reactor, StateSO _state, GameObject _content)
        {
            SerializedObject data = new SerializedObject(_reactor);
            data.FindProperty("state").objectReferenceValue = _state;
            data.FindProperty("content").objectReferenceValue = _content;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireSettingsMenu(GameObject _root, Transform _buttons, StateSO _state)
        {
            BackRequester back = _root.AddComponent<BackRequester>();
            SerializedObject data = new SerializedObject(back);
            data.FindProperty("state").objectReferenceValue = _state;
            data.ApplyModifiedPropertiesWithoutUndo();
            Button button = Find(_buttons, "Back Button");
            UnityEventTools.AddPersistentListener(button.onClick, new UnityAction(back.Back));
        }

        private static void WirePlainMenu(GameObject _root, Transform _buttons, OnboardingMenuWiring _wiring)
        {
            StateButtonRequester requester = _root.AddComponent<StateButtonRequester>();
            ApplicationUtilities application = _root.AddComponent<ApplicationUtilities>();
            List<Button> order = new List<Button>();
            switch (_wiring.Kind)
            {
                case OnboardingMenuKind.MainMenu:
                    Button start = Find(_buttons, "Start Game Button");
                    EnterOnClick(start, requester, _wiring.Play);
                    order.Add(start);
                    AddSettingsButton(start, requester, _wiring, order);
                    break;
                case OnboardingMenuKind.Pause:
                    Button resume = Find(_buttons, "Resume Button");
                    AddSettingsButton(resume, requester, _wiring, order);
                    LeaveOnClick(resume, requester, _wiring.State);
                    order.Insert(0, resume);
                    RemoveButton(_buttons, "NextScene Button");
                    RemoveButton(_buttons, "RestartButton");
                    AddMainMenuButton(_buttons, "Main Menu Button", requester, _wiring, order, _wiring.State, _wiring.Play);
                    break;
                default:
                    // Game Over and Game Win outrank Play and Main Menu, so a request for either is refused until they leave.
                    Button restart = Find(_buttons, "Restart Button");
                    LeaveOnClick(restart, requester, _wiring.State);
                    EnterOnClick(restart, requester, _wiring.Play);
                    order.Add(restart);
                    AddMainMenuButton(_buttons, "Main Menu Button", requester, _wiring, order, _wiring.State);
                    break;
            }
            Button quit = Find(_buttons, "Quit Game");
            UnityEventTools.AddPersistentListener(quit.onClick, new UnityAction(application.QuitGame));
            order.Add(quit);
            Restack(order);
        }

        private static void AddSettingsButton(Button _template, StateButtonRequester _requester, OnboardingMenuWiring _wiring, List<Button> _order)
        {
            if (!_wiring.HasSettingsButton || _wiring.Settings == null)
            {
                return;
            }
            Button settings = Object.Instantiate(_template, _template.transform.parent);
            settings.name = "Settings Button";
            for (int index = settings.onClick.GetPersistentEventCount() - 1; index >= 0; index--)
            {
                UnityEventTools.RemovePersistentListener(settings.onClick, index);
            }
            settings.GetComponentInChildren<TMP_Text>(true).text = "Settings";
            settings.GetComponentInChildren<TMP_Text>(true).gameObject.name = "Settings Text";
            EnterOnClick(settings, _requester, _wiring.Settings);
            _order.Add(settings);
        }

        // Main Menu sits below Play, Pause and the end states, so the button leaves those first, then enters Main Menu.
        private static void AddMainMenuButton(Transform _buttons, string _name, StateButtonRequester _requester, OnboardingMenuWiring _wiring, List<Button> _order, params StateSO[] _leaveFirst)
        {
            if (_wiring.MainMenu == null)
            {
                RemoveButton(_buttons, _name);
                return;
            }
            Button button = Find(_buttons, _name);
            foreach (StateSO state in _leaveFirst)
            {
                LeaveOnClick(button, _requester, state);
            }
            EnterOnClick(button, _requester, _wiring.MainMenu);
            _order.Add(button);
        }

        private static void EnterOnClick(Button _button, StateButtonRequester _requester, StateSO _state)
        {
            UnityEventTools.AddObjectPersistentListener<StateSO>(_button.onClick, new UnityAction<StateSO>(_requester.Enter), _state);
        }

        private static void LeaveOnClick(Button _button, StateButtonRequester _requester, StateSO _state)
        {
            UnityEventTools.AddObjectPersistentListener<StateSO>(_button.onClick, new UnityAction<StateSO>(_requester.Leave), _state);
        }

        private static Button Find(Transform _buttons, string _name)
        {
            Transform found = _buttons.Find(_name);
            if (found == null)
            {
                throw new System.InvalidOperationException("The library menu has no button named '" + _name + "'. Reimport the library and run the setup again.");
            }
            return found.GetComponent<Button>();
        }

        private static void RemoveButton(Transform _buttons, string _name)
        {
            Transform found = _buttons.Find(_name);
            if (found != null)
            {
                Object.DestroyImmediate(found.gameObject);
            }
        }

        // The library's buttons sit at fixed heights, so a removed or added button needs the rest moved up or down.
        private static void Restack(List<Button> _order)
        {
            for (int index = 0; index < _order.Count; index++)
            {
                RectTransform rect = (RectTransform)_order[index].transform;
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, TopButtonY - ButtonSpacing * index);
            }
        }
    }
}
