using System.Threading.Tasks;
using AbstractPixel.Core;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// Scene system S: in the Editor, Addressables.InitializeAsync() could hand back a handle whose Completed never fired
    /// although Addressables had already started, so start-up waited forever on the first scene that is not in Build
    /// Settings (every sample scene). The check must not wait for a start that already happened.
    /// </summary>
    public class AddressableSceneCheckTests
    {
        private static readonly SceneReference SampleScene = new SceneReference("Sample", "0123456789abcdef0123456789abcdef", "Packages/x/Sample.unity");

        [Test]
        public void DoesNotWaitWhenAddressablesHasAlreadyStarted()
        {
            TaskCompletionSource<bool> neverEnds = new TaskCompletionSource<bool>();
            AddressableSceneCheck check = new AddressableSceneCheck(() => true, () => neverEnds.Task);

            Task<bool> answer = check.IsAddressable(SampleScene);

            Assert.IsTrue(answer.IsCompleted, "the check waited for an Addressables start that had already happened");
        }

        // Scene system S, Lab: in a project where Addressables was never built, InitializeAsync logs an error. In the Editor
        // the Addressables settings already know the answer, so the check must not start Addressables for it.
        [Test]
        public void InTheEditorTheEditorAnswerIsUsedWithoutStartingAddressables()
        {
            bool started = false;
            AddressableSceneCheck check = new AddressableSceneCheck(() => false, () =>
            {
                started = true;
                return Task.CompletedTask;
            });
            System.Func<string, bool> realAnswer = AddressableSceneCheck.EditorAnswer;
            AddressableSceneCheck.EditorAnswer = _guid => false;
            try
            {
                Task<bool> answer = check.IsAddressable(SampleScene);

                Assert.IsTrue(answer.IsCompleted);
                Assert.IsFalse(answer.Result);
                Assert.IsFalse(started, "Addressables was started although the Editor knew the answer");
            }
            finally
            {
                AddressableSceneCheck.EditorAnswer = realAnswer;
            }
        }

        [Test]
        public void WaitsForTheStartWhenAddressablesHasNotStarted()
        {
            TaskCompletionSource<bool> start = new TaskCompletionSource<bool>();
            AddressableSceneCheck check = new AddressableSceneCheck(() => false, () => start.Task);
            System.Func<string, bool> realAnswer = AddressableSceneCheck.EditorAnswer;
            AddressableSceneCheck.EditorAnswer = null;
            try
            {
                Task<bool> answer = check.IsAddressable(SampleScene);

                Assert.IsFalse(answer.IsCompleted);
                start.SetResult(true);
            }
            finally
            {
                AddressableSceneCheck.EditorAnswer = realAnswer;
            }
        }
    }
}
