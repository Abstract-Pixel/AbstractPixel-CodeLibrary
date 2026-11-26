using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.GameManagement.Tests
{
    public class TimeScaleEaseTests
    {
        [Test]
        public void WithoutACurveTheEaseIsAStraightLine()
        {
            TimeScaleEase ease = new TimeScaleEase();
            ease.Start(1f, 0f, 2f, new AnimationCurve());
            Assert.AreEqual(0.5f, ease.Step(1f), 0.0001f);
        }

        [Test]
        public void ACurveShapesTheEase()
        {
            TimeScaleEase ease = new TimeScaleEase();
            AnimationCurve snapLate = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 0f), new Keyframe(1f, 1f));
            ease.Start(1f, 0f, 2f, snapLate);
            Assert.AreEqual(1f, ease.Step(1f), 0.0001f);
            Assert.AreEqual(0f, ease.Step(1f), 0.0001f);
        }

        [Test]
        public void NewSettingsHaveNoCurve()
        {
            Assert.AreEqual(0, new StateEffectSettings().TimeScaleEaseCurve.length);
        }
    }
}
