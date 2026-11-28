using AbstractPixel.GameManagement.Debugging;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement.Tests
{
    public class DebuggerToggleTests
    {
        [Test]
        public void TheListeningCopyStaysEnabledWhenAStateDisablesTheOriginalMap()
        {
            InputActionMap map = new InputActionMap("Debug");
            InputAction original = map.AddAction("Toggle", InputActionType.Button, "<Keyboard>/backquote");
            using (DebuggerToggleInput toggle = new DebuggerToggleInput(original))
            {
                map.Enable();
                map.Disable();
                Assert.IsTrue(toggle.Listening.enabled);
                Assert.AreNotSame(original, toggle.Listening);
                Assert.IsFalse(original.enabled);
                Assert.AreEqual("<Keyboard>/backquote", toggle.Listening.bindings[0].effectivePath);
            }
        }

        [Test]
        public void NoActionMeansNothingToListenTo()
        {
            using (DebuggerToggleInput toggle = new DebuggerToggleInput(null))
            {
                Assert.IsNull(toggle.Listening);
                Assert.IsFalse(toggle.WasPerformed());
            }
        }
    }
}
