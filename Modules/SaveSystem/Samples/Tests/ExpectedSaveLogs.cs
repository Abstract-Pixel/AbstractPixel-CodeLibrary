using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine;

[assembly: AbstractPixel.SaveSystem.Samples.Tests.CheckExpectedSaveLogs]

namespace AbstractPixel.SaveSystem.Samples.Tests
{
    // A test that makes a mistake on purpose expects the save system's message with SaveLogAssert.Expect instead
    // of LogAssert.Expect. The message is checked the same way but not printed, so a test run leaves the Console
    // clean and a real error stands out.
    static class SaveLogAssert
    {
        public static void Expect(LogType type, Regex pattern) => SaveLog.Expect(type, pattern);
    }

    // Fails a test whose expected save system message never came, as LogAssert.Expect does.
    [AttributeUsage(AttributeTargets.Assembly)]
    sealed class CheckExpectedSaveLogsAttribute : Attribute, ITestAction
    {
        public ActionTargets Targets => ActionTargets.Test;

        public void BeforeTest(ITest test) => SaveLog.TakeUnmet();

        public void AfterTest(ITest test)
        {
            var unmet = SaveLog.TakeUnmet();
            if (unmet.Count > 0)
            {
                throw new AssertionException("Expected save system message never came: " + string.Join("; ", unmet));
            }
        }
    }
}
