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

        [Test]
        public void WaitsForTheStartWhenAddressablesHasNotStarted()
        {
            TaskCompletionSource<bool> start = new TaskCompletionSource<bool>();
            AddressableSceneCheck check = new AddressableSceneCheck(() => false, () => start.Task);

            Task<bool> answer = check.IsAddressable(SampleScene);

            Assert.IsFalse(answer.IsCompleted);
            start.SetResult(true);
        }
    }
}
