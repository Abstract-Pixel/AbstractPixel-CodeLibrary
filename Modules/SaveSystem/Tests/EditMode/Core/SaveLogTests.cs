using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AbstractPixel.SaveSystem.Tests
{
    public class SaveLogTests
    {
        [Test]
        public void AnExpectedMessage_IsTakenAndNotPrinted()
        {
            SaveLog.Expect(LogType.Error, new Regex("on purpose"));

            SaveLog.Error("[SaveSystem] A mistake made on purpose.");

            LogAssert.NoUnexpectedReceived();
            Assert.IsEmpty(SaveLog.TakeUnmet());
        }

        [Test]
        public void AMessageNobodyExpected_StillPrints()
        {
            // Catches what reaches Unity's logger, so the check itself doesn't print to the Console.
            var printed = new CapturingLogHandler();
            ILogHandler unityHandler = Debug.unityLogger.logHandler;
            SaveLog.Expect(LogType.Error, new Regex("something else"));
            Debug.unityLogger.logHandler = printed;
            try
            {
                SaveLog.Warning("[SaveSystem] A real warning.");
            }
            finally
            {
                Debug.unityLogger.logHandler = unityHandler;
            }

            CollectionAssert.AreEqual(new[] { "Warning: [SaveSystem] A real warning." }, printed.Messages);
            Assert.AreEqual(1, SaveLog.TakeUnmet().Count);
        }

        sealed class CapturingLogHandler : ILogHandler
        {
            public readonly List<string> Messages = new List<string>();

            public void LogFormat(LogType logType, Object context, string format, params object[] args)
            {
                Messages.Add($"{logType}: {string.Format(format, args)}");
            }

            public void LogException(Exception exception, Object context)
            {
                Messages.Add($"Exception: {exception.Message}");
            }
        }
    }
}
