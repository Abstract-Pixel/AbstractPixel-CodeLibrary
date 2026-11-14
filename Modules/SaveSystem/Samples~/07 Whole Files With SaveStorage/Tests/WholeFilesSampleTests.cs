using System.Collections;
using System.Text.RegularExpressions;
using AbstractPixel.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SaveSystemSamples.WholeFiles.Tests
{
    public class WholeFilesSampleTests
    {
        const string SceneName = "Whole Files With SaveStorage";

        [UnityTest]
        public IEnumerator PastingSavedText_PutsTheOldValuesBack_InTheRunningGame()
        {
            yield return SampleScene.Load(SceneName);
            SaveFileViewer viewer = Object.FindAnyObjectByType<SaveFileViewer>();
            viewer.VisitCounter.CountAVisit();
            SaveActions.SaveAll();
            viewer.ReadFile();
            string savedText = viewer.FileText;
            int savedVisits = viewer.VisitCounter.Visits;
            StringAssert.Contains("visits", savedText, "The file holds the [Save] field by its name.");

            viewer.VisitCounter.CountAVisit();
            viewer.VisitCounter.CountAVisit();
            bool written = viewer.WriteFile(savedText);

            Assert.IsTrue(written);
            Assert.AreEqual(savedVisits, viewer.VisitCounter.Visits, "WriteText restores the registered objects in that file from the new text.");
        }

        [UnityTest]
        public IEnumerator PastingTextThatIsNotASaveFile_ChangesNothing()
        {
            yield return SampleScene.Load(SceneName);
            SaveFileViewer viewer = Object.FindAnyObjectByType<SaveFileViewer>();
            SaveActions.SaveAll();
            viewer.ReadFile();
            string textBefore = viewer.FileText;
            int visitsBefore = viewer.VisitCounter.Visits;

            LogAssert.Expect(LogType.Error, new Regex("is not a valid save file"));
            bool written = viewer.WriteFile("this is not json {");

            Assert.IsFalse(written);
            Assert.AreEqual(textBefore, viewer.FileText);
            Assert.AreEqual(visitsBefore, viewer.VisitCounter.Visits);
        }
    }
}
