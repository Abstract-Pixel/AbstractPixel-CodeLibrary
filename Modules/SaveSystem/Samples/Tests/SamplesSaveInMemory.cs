using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine;

[assembly: AbstractPixel.SaveSystem.Samples.Tests.SamplesSaveInMemory]

namespace AbstractPixel.SaveSystem.Samples.Tests
{
    // Every sample test runs on a save system kept in memory, with the project's config. A test that saved to
    // disk and failed before cleaning up would leave its data in the save files you play the samples with.
    [AttributeUsage(AttributeTargets.Assembly)]
    sealed class SamplesSaveInMemoryAttribute : Attribute, ITestAction
    {
        public ActionTargets Targets => ActionTargets.Test;

        public void BeforeTest(ITest test)
        {
            SaveSystemConfigSO config = Resources.Load<SaveSystemConfigSO>(SaveSystemConfigName.ResourceName);
            SaveStartup.Start(config, new MemoryStorageService(), new UnitySceneInfo(), () => Time.realtimeSinceStartupAsDouble);
        }

        public void AfterTest(ITest test)
        {
            SaveStartup.StartFromResources();
        }
    }
}
