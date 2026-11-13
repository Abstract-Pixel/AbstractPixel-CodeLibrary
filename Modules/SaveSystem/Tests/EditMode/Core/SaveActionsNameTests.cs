using AbstractPixel.SaveSystem;
using NUnit.Framework;

// A namespace inside AbstractPixel but outside AbstractPixel.SaveSystem, like the library's other modules.
// Before the rename, "SaveSystem" here meant the namespace, so calls to the class didn't compile.
namespace AbstractPixel.SaveSystemTests.Other
{
    public class SaveActionsNameTests
    {
        [Test]
        public void CodeInAnotherAbstractPixelNamespace_CallsSaveActions()
        {
            SaveStartup.Start(SaveSystem.Tests.SaveSystemTestKit.CreateConfig(), new MemoryStorageService(), new SaveSystem.Tests.FakeSceneInfo(), () => 0d);

            Assert.DoesNotThrow(() => SaveActions.SaveAll());

            SaveStartup.Stop();
        }
    }
}
