using NUnit.Framework;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation.Tests
{
    /// <summary>Step lists tick in order, from the messages their steps wait for.</summary>
    public class DocsStepListTests
    {
        private static DocsStepList Steps(params string[] _doneWhen)
        {
            DocsStepList steps = new DocsStepList();
            foreach (string message in _doneWhen)
            {
                steps.AddStep(new Label(), message);
            }
            return steps;
        }

        [Test]
        public void ALaterStepNeverTicksEarly()
        {
            DocsStepList steps = Steps("OnGameStarted", "OnNewSceneGroupTransitionTo");
            Assert.IsFalse(steps.Report("OnNewSceneGroupTransitionTo"));
            Assert.IsFalse(steps.IsDone(1));
            Assert.IsTrue(steps.Report("OnGameStarted"));
            Assert.IsTrue(steps.Report("OnNewSceneGroupTransitionTo"));
            Assert.IsTrue(steps.IsDone(1));
        }

        [Test]
        public void StepsThatWaitForNothingTickWithTheNextOne()
        {
            DocsStepList steps = Steps(null, "OnGameStarted", null);
            steps.Report("OnGameStarted");
            Assert.IsTrue(steps.IsDone(0));
            Assert.IsTrue(steps.IsDone(1));
            Assert.IsFalse(steps.IsDone(2));
        }

        [Test]
        public void ResetStartsAgain()
        {
            DocsStepList steps = Steps("OnGameStarted");
            steps.Report("OnGameStarted");
            steps.Reset();
            Assert.IsFalse(steps.IsDone(0));
        }
    }
}
