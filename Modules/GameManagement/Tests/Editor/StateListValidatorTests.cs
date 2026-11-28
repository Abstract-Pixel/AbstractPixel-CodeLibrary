using System.Collections.Generic;
using AbstractPixel.GameManagement.Editor;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.GameManagement.Tests
{
    public class StateListValidatorTests
    {
        private readonly List<Object> made = new List<Object>();
        private GameStateCatalog catalog;

        [SetUp]
        public void SetUp()
        {
            catalog = ScriptableObject.CreateInstance<GameStateCatalog>();
            made.Add(catalog);
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

        private StateSO State(string _name, bool _listed, params StateSO[] _parents)
        {
            StateSO state = ScriptableObject.CreateInstance<StateSO>();
            state.name = _name;
            state.Parents.AddRange(_parents);
            made.Add(state);
            if (_listed)
            {
                catalog.States.Add(state);
            }
            return state;
        }

        private StateProblem Only()
        {
            List<StateProblem> problems = StateListValidator.Find(catalog);
            Assert.AreEqual(1, problems.Count);
            return problems[0];
        }

        [Test]
        public void CleanListHasNoProblems()
        {
            StateSO play = State("Play", true);
            State("Pause", true, play);
            Assert.IsEmpty(StateListValidator.Find(catalog));
        }

        [Test]
        public void EmptySlotIsAWarningAndTheFixRemovesIt()
        {
            catalog.States.Add(null);
            StateProblem problem = Only();
            Assert.IsFalse(problem.IsError);
            Assert.AreEqual("Slot 1 in the state list is empty. It does nothing.", problem.Message);
            StateListValidator.Fix(catalog, problem);
            Assert.IsEmpty(catalog.States);
        }

        [Test]
        public void DuplicateIsAWarningAndTheFixKeepsTheHigherPlace()
        {
            StateSO pause = State("Pause", true);
            StateSO play = State("Play", true);
            catalog.States.Add(pause);
            StateProblem problem = Only();
            Assert.IsFalse(problem.IsError);
            StateListValidator.Fix(catalog, problem);
            CollectionAssert.AreEqual(new[] { pause, play }, catalog.States);
        }

        [Test]
        public void SelfParentIsAnErrorAndTheFixRemovesIt()
        {
            StateSO settings = State("Settings", true);
            settings.Parents.Add(settings);
            StateProblem problem = Only();
            Assert.IsTrue(problem.IsError);
            Assert.AreEqual("Settings lists itself as a parent, so it can never open.", problem.Message);
            StateListValidator.Fix(catalog, problem);
            Assert.IsEmpty(settings.Parents);
        }

        [Test]
        public void ParentLoopIsOneError()
        {
            StateSO settings = State("Settings", true);
            StateSO options = State("Options", true, settings);
            settings.Parents.Add(options);
            StateProblem problem = Only();
            Assert.IsTrue(problem.IsError);
            Assert.AreEqual("Settings and Options open over each other, so neither can open first.", problem.Message);
            StateListValidator.Fix(catalog, problem);
            Assert.IsEmpty(StateListValidator.Find(catalog));
        }

        [Test]
        public void EmptyParentSlotIsAWarning()
        {
            StateSO play = State("Play", true);
            StateSO settings = State("Settings", true, play, null);
            StateProblem problem = Only();
            Assert.IsFalse(problem.IsError);
            StateListValidator.Fix(catalog, problem);
            CollectionAssert.AreEqual(new[] { play }, settings.Parents);
        }

        [Test]
        public void SameParentTwiceIsAWarning()
        {
            StateSO play = State("Play", true);
            StateSO settings = State("Settings", true, play, play);
            StateProblem problem = Only();
            Assert.IsFalse(problem.IsError);
            Assert.AreEqual("Settings lists Play as a parent twice.", problem.Message);
            StateListValidator.Fix(catalog, problem);
            CollectionAssert.AreEqual(new[] { play }, settings.Parents);
        }

        [Test]
        public void SomeParentsMissingIsAWarningAndTheFixAddsThem()
        {
            StateSO menu = State("Main Menu", true);
            StateSO pause = State("Pause", false);
            State("Settings", true, menu, pause);
            StateProblem problem = Only();
            Assert.IsFalse(problem.IsError);
            Assert.AreEqual("Settings opens over Pause, but Pause isn't in the state list. It still opens over Main Menu.", problem.Message);
            Assert.AreEqual("Add Pause", problem.FixLabel);
            StateListValidator.Fix(catalog, problem);
            Assert.AreSame(pause, catalog.States[0], "a new main state shows at the bottom of the page: the lowest priority place");
        }

        [Test]
        public void AllParentsMissingIsAnError()
        {
            StateSO pause = State("Pause", false);
            State("Credits", true, pause);
            StateProblem problem = Only();
            Assert.IsTrue(problem.IsError);
            Assert.AreEqual("Credits only opens over Pause, which isn't in the state list, so Credits can never open.", problem.Message);
        }
    }
}
