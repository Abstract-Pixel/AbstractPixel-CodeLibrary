using System.Collections.Generic;
using System.Reflection;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Tests
{
    // 2.5: play mode is blocked while SetupCheck has found a problem. ShouldBlockPlay is the pure decision, so it's
    // tested here without entering play mode; SetupProblemsWindow shows the problems an EditMode test can't see.
    public class SetupPlayModeGuardTests
    {
        // No [Save] attribute, so this stays invisible to the project-wide scan: see SetupCheckTests for why.
        class ScratchType
        {
            public GameObject x;
        }

        [Test]
        public void AScratchTypeWithAnInvalidField_BlocksPlay()
        {
            FieldInfo field = typeof(ScratchType).GetField(nameof(ScratchType.x));
            List<SetupProblem> problems = SetupCheck.ProblemsFromFields(new[] { field });

            Assert.IsTrue(SetupPlayModeGuard.ShouldBlockPlay(problems));
        }

        [Test]
        public void ACleanProject_DoesNotBlockPlay()
        {
            Assert.IsFalse(SetupPlayModeGuard.ShouldBlockPlay(new List<SetupProblem>()));
        }
    }
}
