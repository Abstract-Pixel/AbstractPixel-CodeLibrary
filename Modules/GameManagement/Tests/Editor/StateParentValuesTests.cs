using System.Collections.Generic;
using System.Linq;
using AbstractPixel.GameManagement.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement.Tests
{
    public class StateParentValuesTests
    {
        private readonly List<Object> made = new List<Object>();
        private InputActionReference move;
        private InputActionReference jump;
        private InputActionReference submit;

        [SetUp]
        public void SetUp()
        {
            InputActionAsset asset = Make(ScriptableObject.CreateInstance<InputActionAsset>());
            InputActionMap player = asset.AddActionMap("Player");
            InputAction moveAction = player.AddAction("Move");
            InputAction jumpAction = player.AddAction("Jump");
            InputAction submitAction = asset.AddActionMap("UI").AddAction("Submit");
            move = Make(InputActionReference.Create(moveAction));
            jump = Make(InputActionReference.Create(jumpAction));
            submit = Make(InputActionReference.Create(submitAction));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object item in made)
            {
                Object.DestroyImmediate(item);
            }
            made.Clear();
        }

        private T Make<T>(T _item) where T : Object
        {
            made.Add(_item);
            return _item;
        }

        private StateSO State(string _name, params StateSO[] _parents)
        {
            StateSO state = Make(ScriptableObject.CreateInstance<StateSO>());
            state.name = _name;
            state.Parents.AddRange(_parents);
            return state;
        }

        private static void OwnTime(StateSO _state, float _time)
        {
            _state.Effects.TimeScaleSource = StateEffectSource.Own;
            _state.Effects.TimeScale = _time;
        }

        private static void OwnInput(StateSO _state, params InputActionReference[] _actions)
        {
            _state.Effects.InputSource = StateEffectSource.Own;
            _state.Effects.InputMaps = new List<InputActionReference>(_actions);
        }

        [Test]
        public void InputMapsAreComparedAsSetsSoOrderAndRepeatsDoNotCount()
        {
            StateSO play = State("Play");
            OwnInput(play, move, submit);
            StateSO pause = State("Pause", play);
            OwnInput(pause, submit, jump, move, move);

            Assert.IsFalse(StateParentValues.Cell(pause, play, StateEffectKind.Input).IsChanged);
        }

        [Test]
        public void ADifferentMapSetIsAChange()
        {
            StateSO play = State("Play");
            OwnInput(play, move);
            StateSO pause = State("Pause", play);
            OwnInput(pause, submit);

            StateEffectCell cell = StateParentValues.Cell(pause, play, StateEffectKind.Input);

            Assert.IsTrue(cell.IsChanged);
            Assert.AreEqual("UI", cell.Text);
            StringAssert.Contains("Player", cell.Tooltip);
        }

        [Test]
        public void MapNamesTooLongForACellAreCountedAndStillListed()
        {
            InputActionAsset asset = Make(ScriptableObject.CreateInstance<InputActionAsset>());
            InputActionReference vehicle = Make(InputActionReference.Create(asset.AddActionMap("PlayerLocomotionAndVehicleControls").AddAction("Drive")));
            StateSO play = State("Play");
            StateSO pause = State("Pause", play);
            OwnInput(pause, vehicle, submit);

            StateEffectCell cell = StateParentValues.Cell(pause, play, StateEffectKind.Input);

            Assert.AreEqual("2 maps", cell.Text);
            CollectionAssert.AreEqual(new[] { "PlayerLocomotionAndVehicleControls", "UI" }, cell.ListedMaps);
        }

        [Test]
        public void OwnWithNoActionsTurnsEveryMapOff()
        {
            StateSO play = State("Play");
            StateSO pause = State("Pause", play);
            OwnInput(pause);

            Assert.AreEqual("All off", StateParentValues.Cell(pause, play, StateEffectKind.Input).Text);
        }

        [Test]
        public void SameAsParentShowsTheParentValueAndIsNotAChange()
        {
            StateSO play = State("Play");
            OwnTime(play, 0.5f);
            StateSO pause = State("Pause", play);

            StateEffectCell cell = StateParentValues.Cell(pause, play, StateEffectKind.TimeScale);

            Assert.AreEqual("0.5", cell.Text);
            Assert.IsFalse(cell.IsChanged);
        }

        [Test]
        public void OwnWithTheParentsValueIsNotAChange()
        {
            StateSO play = State("Play");
            OwnTime(play, 1f);
            StateSO pause = State("Pause", play);
            OwnTime(pause, 1f);

            Assert.IsFalse(StateParentValues.Cell(pause, play, StateEffectKind.TimeScale).IsChanged);
        }

        [Test]
        public void ValuesThatRoundToTheSameTextAreStillAChange()
        {
            StateSO play = State("Play");
            OwnTime(play, 1f);
            StateSO pause = State("Pause", play);
            OwnTime(pause, 0.9999f);

            Assert.IsTrue(StateParentValues.Cell(pause, play, StateEffectKind.TimeScale).IsChanged);
        }

        [Test]
        public void AChainWithNoOwnValueKeepsTheLastValue()
        {
            StateSO play = State("Play");
            StateSO pause = State("Pause", play);

            Assert.AreEqual("Keeps last", StateParentValues.Cell(pause, play, StateEffectKind.TimeScale).Text);
        }

        [Test]
        public void ChangingTheParentChangesTheCell()
        {
            StateSO play = State("Play");
            OwnTime(play, 1f);
            StateSO pause = State("Pause", play);
            Assert.AreEqual("1", StateParentValues.Cell(pause, play, StateEffectKind.TimeScale).Text);

            OwnTime(play, 0.25f);

            Assert.AreEqual("0.25", StateParentValues.Cell(pause, play, StateEffectKind.TimeScale).Text);
        }

        [Test]
        public void ASubStateParentWithOneResultShowsIt()
        {
            StateSO menu = State("Menu");
            StateSO play = State("Play");
            StateSO pause = State("Pause", menu, play);
            OwnTime(pause, 0f);
            StateSO settings = State("Settings", pause);

            Assert.AreEqual("0", StateParentValues.Cell(settings, pause, StateEffectKind.TimeScale).Text);
        }

        [Test]
        public void ASubStateParentWithTwoResultsSaysVariesAndNamesEachStack()
        {
            StateSO menu = State("Menu");
            OwnTime(menu, 1f);
            StateSO play = State("Play");
            OwnTime(play, 0.5f);
            StateSO pause = State("Pause", menu, play);
            StateSO settings = State("Settings", pause);

            StateEffectCell cell = StateParentValues.Cell(settings, pause, StateEffectKind.TimeScale);

            Assert.AreEqual("Varies", cell.Text);
            StringAssert.Contains("1 over Menu > Pause", cell.Tooltip);
            StringAssert.Contains("0.5 over Play > Pause", cell.Tooltip);
        }

        [Test]
        public void AParentLoopDoesNotRunForever()
        {
            StateSO first = State("First");
            StateSO second = State("Second", first);
            first.Parents.Add(second);

            Assert.DoesNotThrow(() => StateParentValues.Cell(first, second, StateEffectKind.TimeScale));
        }

        [Test]
        public void InheritedLineNamesEachParentWhenTheyDiffer()
        {
            StateSO menu = State("Menu");
            menu.Effects.HudSource = StateEffectSource.Own;
            menu.Effects.HudVisible = false;
            StateSO play = State("Play");
            play.Effects.HudSource = StateEffectSource.Own;
            play.Effects.HudVisible = true;
            StateSO settings = State("Settings", menu, play);

            Assert.AreEqual("Hidden over Menu, Shown over Play", StateParentValues.InheritedLine(settings, StateEffectKind.Hud));
        }

        [Test]
        public void InheritedLineSaysFromEachParentWhenTheyAgree()
        {
            StateSO menu = State("Menu");
            OwnInput(menu, submit);
            StateSO play = State("Play");
            OwnInput(play, submit, submit);
            StateSO settings = State("Settings", menu, play);

            Assert.AreEqual("UI, from each parent", StateParentValues.InheritedLine(settings, StateEffectKind.Input));
        }

        [Test]
        public void InheritedLineWithManyParentsGroupsThemByValueOnePerLine()
        {
            StateSO menu = State("Menu");
            StateSO play = State("Play");
            StateSO replay = State("Replay");
            foreach (StateSO parent in new[] { menu, play, replay })
            {
                parent.Effects.HudSource = StateEffectSource.Own;
                parent.Effects.HudVisible = parent == play;
            }
            StateSO settings = State("Settings", menu, play, replay);

            Assert.AreEqual("Over Menu, Replay: Hidden\nOver Play: Shown", StateParentValues.InheritedLine(settings, StateEffectKind.Hud));
        }

        [Test]
        public void InheritedLineWithLongNamesPointsAtParentNumbers()
        {
            StateSO menu = State("Main Menu With A Very Long Descriptive Name That Goes On");
            StateSO play = State("Play");
            StateSO replay = State("Replay Theatre With Another Very Long Descriptive Name");
            foreach (StateSO parent in new[] { menu, play, replay })
            {
                parent.Effects.HudSource = StateEffectSource.Own;
                parent.Effects.HudVisible = parent == play;
            }
            StateSO settings = State("Settings", menu, null, play, replay);

            Assert.AreEqual("Over parents 1, 4: Hidden\nOver parent 3: Shown", StateParentValues.InheritedLine(settings, StateEffectKind.Hud));
        }

        [Test]
        public void InheritedLineSaysInPlainWordsThatNoParentSetsTheValue()
        {
            StateSO menu = State("Menu");
            StateSO play = State("Play");
            StateSO settings = State("Settings", menu, play);

            Assert.AreEqual("No parent sets this, so it stays as it was", StateParentValues.InheritedLine(settings, StateEffectKind.Cursor));
        }

        [Test]
        public void InheritedLineOfAMainStateKeepsWhatWasSetBefore()
        {
            Assert.AreEqual("Keeps whatever was set before", StateParentValues.InheritedLine(State("Play"), StateEffectKind.Cursor));
        }

        [Test]
        public void FirstSwitchToOwnCopiesParentOne()
        {
            StateSO play = State("Play");
            OwnInput(play, move, submit);
            StateSO pause = State("Pause", play);

            StateOwnValues.SetSource(pause, StateEffectKind.Input, StateEffectSource.Own);

            CollectionAssert.AreEqual(new[] { move, submit }, pause.Effects.InputMaps);
        }

        [Test]
        public void AValueTheDeveloperPickedIsNeverOverwritten()
        {
            StateSO play = State("Play");
            OwnTime(play, 1f);
            StateSO pause = State("Pause", play);
            OwnTime(pause, 0.3f);

            StateOwnValues.SetSource(pause, StateEffectKind.TimeScale, StateEffectSource.SameAsParent);
            StateOwnValues.SetSource(pause, StateEffectKind.TimeScale, StateEffectSource.Own);

            Assert.AreEqual(0.3f, pause.Effects.TimeScale);
        }

        [Test]
        public void AnUntouchedCopyFollowsParentOneAgain()
        {
            StateSO play = State("Play");
            OwnTime(play, 0.5f);
            StateSO pause = State("Pause", play);
            StateOwnValues.SetSource(pause, StateEffectKind.TimeScale, StateEffectSource.Own);
            StateOwnValues.SetSource(pause, StateEffectKind.TimeScale, StateEffectSource.SameAsParent);
            play.Effects.TimeScale = 0.75f;

            StateOwnValues.SetSource(pause, StateEffectKind.TimeScale, StateEffectSource.Own);

            Assert.AreEqual(0.75f, pause.Effects.TimeScale);
        }

        [Test]
        public void WithNoParentValueOwnKeepsTheStoredValue()
        {
            StateSO play = State("Play");
            StateSO pause = State("Pause", play);
            pause.Effects.TimeScale = 0.2f;

            StateOwnValues.SetSource(pause, StateEffectKind.TimeScale, StateEffectSource.Own);

            Assert.AreEqual(0.2f, pause.Effects.TimeScale);
        }
    }
}
