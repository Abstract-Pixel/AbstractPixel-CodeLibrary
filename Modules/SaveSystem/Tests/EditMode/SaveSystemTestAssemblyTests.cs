using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace AbstractPixel.SaveSystem.Tests
{
    // Smoke test: proves the test assembly compiles, runs and can see the save system's internal members.
    public class SaveSystemTestAssemblyTests
    {
        [Test]
        public void RuntimeAssembly_ExposesInternalsToTheTestAssembly()
        {
            Assembly runtimeAssembly = typeof(SaveSystemConfigSO).Assembly;

            bool isVisible = runtimeAssembly
                .GetCustomAttributes<InternalsVisibleToAttribute>()
                .Any(attribute => attribute.AssemblyName == "AbstractPixel.SaveSystem.Tests");

            Assert.IsTrue(isVisible, "Add [assembly: InternalsVisibleTo(\"AbstractPixel.SaveSystem.Tests\")] to the save system runtime assembly.");
        }
    }
}
