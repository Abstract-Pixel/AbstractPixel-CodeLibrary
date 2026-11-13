using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.Tests
{
    // S2: the after-compile scan. None of this fixture's fields carry [Save], so handing them to Scan directly,
    // the way TypeCache would, proves the scan without compiling a broken [Save] field into the project.
    public class SavedFieldScanTests
    {
        class BadField
        {
            public GameObject unityObjectField;
        }

        static FieldInfo FieldNamed(string name) => typeof(BadField).GetField(name);

        [Test]
        public void FieldWithAProblem_LogsOneErrorAndReportsFound()
        {
            LogAssert.Expect(LogType.Error, new Regex("BadField.unityObjectField.*holds a GameObject.*Play mode won't start, and builds will fail"));
            bool foundAProblem = SavedFieldScan.Scan(new List<FieldInfo> { FieldNamed("unityObjectField") });
            Assert.IsTrue(foundAProblem);
        }

        [Test]
        public void NoFields_ReportsNothingFound()
        {
            bool foundAProblem = SavedFieldScan.Scan(new List<FieldInfo>());
            Assert.IsFalse(foundAProblem);
        }
    }
}
