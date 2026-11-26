using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.GameManagement.Tests
{
    public class StateTimeScaleTargetTests
    {
        private StateSO state;

        [SetUp]
        public void CreateState()
        {
            state = ScriptableObject.CreateInstance<StateSO>();
        }

        [TearDown]
        public void DestroyState()
        {
            Object.DestroyImmediate(state);
        }

        // Columns: time zero flag, override flag, override value, expected time scale.
        [TestCase(true, false, 0.5f, 0f)]
        [TestCase(false, false, 0.5f, 1f)]
        [TestCase(true, true, 0.2f, 0.2f)]
        [TestCase(false, true, 0f, 0f)]
        public void TargetFollowsTheOverrideFirstAndThenTheTimeZeroFlag(bool _isTimeZero, bool _isOverride, float _overrideValue, float _expected)
        {
            state.IsTimeZeroOnExecution = _isTimeZero;
            state.OverrideExecutionTimeScale = _isOverride;
            state.ExecutionTimeScale = _overrideValue;

            float target = state.GetExecutionTimeScaleTarget();

            Assert.AreEqual(_expected, target);
        }
    }
}
