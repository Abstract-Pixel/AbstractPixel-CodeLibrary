using System;
using System.Reflection;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// Says whether the Test Runner is running tests. TestRunnerApi.IsRunActive exists in recent Test Framework
    /// versions; it is read through reflection so the editor modules do not reference the test runner.
    /// </summary>
    public static class TestRunDetector
    {
        public static bool TestsAreRunning()
        {
            Type api = Type.GetType("UnityEditor.TestTools.TestRunner.Api.TestRunnerApi, UnityEditor.TestRunner");
            if (api == null)
            {
                return false;
            }
            MethodInfo isRunActive = api.GetMethod("IsRunActive", BindingFlags.Public | BindingFlags.Static);
            if (isRunActive == null)
            {
                return false;
            }
            return (bool)isRunActive.Invoke(null, null);
        }
    }
}
