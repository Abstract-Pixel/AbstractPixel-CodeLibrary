using System.Reflection;
using NUnit.Framework;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // S2: the blocking checks. None of this fixture's fields carry [Save]: SavedFieldRules.ProblemWith only
    // inspects a field's shape, so these stay invisible to the project-wide [Save] scan while still proving it.
    public class SavedFieldRulesTests
    {
        abstract class AbstractBase
        {
        }

        class BadFieldShapes
        {
            public UnityEngine.GameObject unityObjectField;
            public static int staticField;
            public readonly int readonlyField = 1;
            public const int constField = 1;
            public System.Action delegateField;
            public System.IDisposable interfaceField;
            public AbstractBase abstractField;
            public object objectField;
            public int validField;
        }

        static FieldInfo FieldNamed(string name) => typeof(BadFieldShapes).GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

        [Test]
        public void UnityObjectField_IsBlocked()
        {
            string problem = SavedFieldRules.ProblemWith(FieldNamed("unityObjectField"));
            Assert.IsNotNull(problem);
            StringAssert.Contains("BadFieldShapes.unityObjectField", problem);
            StringAssert.Contains("GameObject", problem);
        }

        [Test]
        public void StaticField_IsBlocked()
        {
            Assert.IsNotNull(SavedFieldRules.ProblemWith(FieldNamed("staticField")));
        }

        [Test]
        public void ReadonlyField_IsBlocked()
        {
            Assert.IsNotNull(SavedFieldRules.ProblemWith(FieldNamed("readonlyField")));
        }

        [Test]
        public void ConstField_IsBlocked()
        {
            Assert.IsNotNull(SavedFieldRules.ProblemWith(FieldNamed("constField")));
        }

        [Test]
        public void DelegateField_IsBlocked()
        {
            Assert.IsNotNull(SavedFieldRules.ProblemWith(FieldNamed("delegateField")));
        }

        [Test]
        public void InterfaceField_IsBlocked()
        {
            Assert.IsNotNull(SavedFieldRules.ProblemWith(FieldNamed("interfaceField")));
        }

        [Test]
        public void AbstractClassField_IsBlocked()
        {
            Assert.IsNotNull(SavedFieldRules.ProblemWith(FieldNamed("abstractField")));
        }

        [Test]
        public void ObjectField_IsBlocked()
        {
            Assert.IsNotNull(SavedFieldRules.ProblemWith(FieldNamed("objectField")));
        }

        [Test]
        public void OrdinaryField_IsNotBlocked()
        {
            Assert.IsNull(SavedFieldRules.ProblemWith(FieldNamed("validField")));
        }

        [Test]
        public void ClassWithISavable_IsBlocked()
        {
            Assert.IsNotNull(SavedFieldRules.ProblemWithClass(typeof(PlayerHealth)), "PlayerHealth implements ISavable<int, TestGameFile>.");
        }

        [Test]
        public void ClassWithNoISavable_IsNotBlocked()
        {
            Assert.IsNull(SavedFieldRules.ProblemWithClass(typeof(BadFieldShapes)));
        }
    }
}
