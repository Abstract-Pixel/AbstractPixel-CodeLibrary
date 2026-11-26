using System.Collections.Generic;
using AbstractPixel.GameManagement.Editor;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.GameManagement.Tests
{
    /// <summary>Runs on states made in the test, never on a game's assets.</summary>
    public class StateMigratorTests
    {
        private readonly List<Object> created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object item in created)
            {
                Object.DestroyImmediate(item);
            }
            created.Clear();
        }

        private StateSO Make(string _name, int _priority)
        {
            StateSO state = ScriptableObject.CreateInstance<StateSO>();
            state.name = _name;
            state.Priority = _priority;
            created.Add(state);
            return state;
        }

        [Test]
        public void DryRunOrdersByPriorityAndChangesNothing()
        {
            StateSO play = Make("Play", 0);
            StateSO gameOver = Make("GameOver", 10);
            StateSO pause = Make("Pause", 5);
            EffectSource before = play.Effects.TimeScaleSource;

            StateMigrationPlan plan = StateMigrator.Plan(new[] { play, gameOver, pause });

            CollectionAssert.AreEqual(new[] { gameOver, pause, play }, plan.CatalogOrder);
            Assert.AreEqual(before, play.Effects.TimeScaleSource);
        }

        [Test]
        public void TiesAreReported()
        {
            StateMigrationPlan plan = StateMigrator.Plan(new[] { Make("A", 1), Make("B", 1) });
            Assert.AreEqual(1, plan.Problems.Count);
            StringAssert.Contains("share priority 1", plan.Problems[0]);
        }

        [Test]
        public void SubStatesWithoutParentsAreReported()
        {
            StateSO pause = Make("Pause", 5);
            pause.IsSubState = true;
            StateMigrationPlan plan = StateMigrator.Plan(new[] { pause });
            StringAssert.Contains("has no parents", plan.Problems[0]);
        }

        [Test]
        public void ApplyWritesTimeCursorAndHudAndTheCatalogOrder()
        {
            StateSO gameOver = Make("GameOver", 10);
            gameOver.IsTimeZeroOnExecution = true;
            gameOver.TimeScaleEaseDuration = 1f;
            gameOver.IsCursorLockedOnExecution = false;
            gameOver.IsCursorVisibleOnExecution = true;
            gameOver.ShowGameUIOnExecution = false;
            StateSO play = Make("Play", 0);
            play.IsTimeZeroOnExecution = false;
            play.IsCursorLockedOnExecution = true;
            play.IsCursorVisibleOnExecution = false;
            play.ShowGameUIOnExecution = true;
            GameStateCatalog catalog = ScriptableObject.CreateInstance<GameStateCatalog>();
            created.Add(catalog);

            StateMigrator.Apply(StateMigrator.Plan(new[] { play, gameOver }), catalog);

            Assert.AreEqual(EffectSource.Own, gameOver.Effects.TimeScaleSource);
            Assert.AreEqual(0f, gameOver.Effects.TimeScale);
            Assert.AreEqual(1f, gameOver.Effects.TimeScaleEaseSeconds);
            Assert.AreEqual(gameOver.TimeScaleEaseCurve.length, gameOver.Effects.TimeScaleEaseCurve.length);
            Assert.AreEqual(GameCursorMode.Free, gameOver.Effects.Cursor);
            Assert.IsFalse(gameOver.Effects.HudVisible);
            Assert.AreEqual(1f, play.Effects.TimeScale);
            Assert.AreEqual(GameCursorMode.LockedHidden, play.Effects.Cursor);
            Assert.IsTrue(play.Effects.HudVisible);
            CollectionAssert.AreEqual(new[] { gameOver, play }, catalog.States);
        }

        [Test]
        public void EveryOldCursorPairHasAMode()
        {
            StateSO state = Make("S", 0);
            state.IsCursorLockedOnExecution = true;
            state.IsCursorVisibleOnExecution = true;
            Assert.AreEqual(GameCursorMode.LockedVisible, StateMigrator.CursorFor(state));
            state.IsCursorLockedOnExecution = false;
            state.IsCursorVisibleOnExecution = false;
            Assert.AreEqual(GameCursorMode.FreeHidden, StateMigrator.CursorFor(state));
        }
    }
}
