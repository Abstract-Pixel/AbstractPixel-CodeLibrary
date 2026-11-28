using AbstractPixel.GameManagement.Editor;
using NUnit.Framework;

namespace AbstractPixel.GameManagement.Tests
{
    public class DebuggerBuildRuleTests
    {
        private static readonly string[] Assemblies =
        {
            "Library/Player/AbstractPixel.GameManagement.dll",
            "Library/Player/AbstractPixel.GameManagement.Debugger.dll",
        };

        [TestCase(true, false, true)]
        [TestCase(true, true, true)]
        [TestCase(false, true, true)]
        [TestCase(false, false, false)]
        public void IncludedInDevelopmentBuildsOrWhenOptedIn(bool _development, bool _optIn, bool _expected)
        {
            Assert.AreEqual(_expected, DebuggerBuildRule.Includes(_development, _optIn));
        }

        [Test]
        public void AReleaseBuildWithoutTheOptInDropsOnlyTheDebuggerAssembly()
        {
            string[] kept = DebuggerBuildRule.Filter(Assemblies, false);
            CollectionAssert.AreEqual(new[] { Assemblies[0] }, kept);
        }

        [Test]
        public void AnIncludedDebuggerKeepsEveryAssembly()
        {
            CollectionAssert.AreEqual(Assemblies, DebuggerBuildRule.Filter(Assemblies, true));
        }
    }
}
