using System.Linq;
using AbstractPixel.GameManagement.Debugging;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Tests
{
    public class DebuggerOverlayViewTests
    {
        private MachineTestRig rig;

        [SetUp]
        public void SetUp()
        {
            rig = new MachineTestRig();
        }

        [TearDown]
        public void TearDown()
        {
            rig.Dispose();
        }

        private static string Text(VisualElement _root)
        {
            return string.Join("|", _root.Query<Label>().ToList().Select(_label => _label.text));
        }

        [Test]
        public void NothingInTheOverlayCanBePickedOrFocused()
        {
            rig.Machine.Request(rig.Play);
            rig.Machine.Request(rig.Settings_, "Button");
            DebuggerOverlayView view = new DebuggerOverlayView();
            view.Show(DebuggerSnapshot.From(rig.Machine, rig.Catalog), "F8 hide");

            foreach (VisualElement element in view.Root.Query<VisualElement>().ToList())
            {
                Assert.AreEqual(PickingMode.Ignore, element.pickingMode, element.GetType().Name + " " + string.Join(" ", element.GetClasses()));
                Assert.IsFalse(element.focusable);
            }
        }

        [Test]
        public void AnEmptyJournalSaysSo()
        {
            DebuggerOverlayView view = new DebuggerOverlayView();
            view.Show(DebuggerSnapshot.From(rig.Machine, rig.Catalog), "F8 hide");
            string text = Text(view.Root);
            StringAssert.Contains("No transitions yet.", text);
            StringAssert.Contains("No state active", text);
            StringAssert.Contains("F8 hide", text);
            StringAssert.DoesNotContain("LAST REFUSAL", text);
        }

        [Test]
        public void ShowingAgainReplacesTheOldContent()
        {
            DebuggerOverlayView view = new DebuggerOverlayView();
            view.Show(DebuggerSnapshot.From(rig.Machine, rig.Catalog), "");
            rig.Machine.Request(rig.Play);
            view.Show(DebuggerSnapshot.From(rig.Machine, rig.Catalog), "");
            StringAssert.DoesNotContain("No transitions yet.", Text(view.Root));
            Assert.AreEqual(1, view.Root.Query(className: "gm-stack").ToList().Count);
        }
    }
}
