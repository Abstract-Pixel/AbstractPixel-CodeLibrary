using UnityEditor.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>"Input": which input actions the states switch on and off. The dropdown's order is the order of <see cref="InputSourceKind"/>.</summary>
    internal class OnboardingInputPage : OnboardingPage
    {
        internal override string Template
        {
            get { return "OnboardingInput"; }
        }

        internal override void Wire(VisualElement _root, OnboardingContext _context)
        {
            OnboardingChoices choices = _context.Choices;
            DropdownField dropdown = _root.Q<DropdownField>("inputSourceDropdown");
            ObjectField assetField = _root.Q<ObjectField>("inputasset-chosen");
            assetField.objectType = typeof(InputActionAsset);
            assetField.allowSceneObjects = false;
            assetField.SetValueWithoutNotify(choices.InputActions);
            dropdown.index = (int)choices.InputSource;
            Show(assetField, choices.InputSource, _context);
            dropdown.RegisterValueChangedCallback(_change =>
            {
                choices.InputSource = (InputSourceKind)dropdown.index;
                Show(assetField, choices.InputSource, _context);
            });
            assetField.RegisterValueChangedCallback(_change =>
            {
                choices.InputActions = _change.newValue as InputActionAsset;
                Show(assetField, choices.InputSource, _context);
            });
        }

        // Chosen asset with no asset picked would leave the machine with no input, so Next waits for one.
        private static void Show(ObjectField _assetField, InputSourceKind _source, OnboardingContext _context)
        {
            _assetField.EnableInClassList("gm-hidden", _source != InputSourceKind.ChosenAsset);
            _context.CanContinue = _source != InputSourceKind.ChosenAsset || _context.Choices.InputActions != null;
            _context.Changed?.Invoke();
        }
    }
}
