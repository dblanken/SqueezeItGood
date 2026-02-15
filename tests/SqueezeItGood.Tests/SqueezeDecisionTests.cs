using Xunit;

namespace SqueezeItGood.Tests
{
    public class SqueezeDecisionTests
    {
        [Fact]
        public void JuiceLevelChanged_ReturnsNone()
        {
            var result = SqueezeDecision.Evaluate(
                juiceLeftBefore: 1.0, juiceLeftNow: 0.5,
                animFinished: true, hasMash: true,
                squeezeRel: 1.5, pressSqueezeRel: 1.2,
                actualJuice: 0.5);

            Assert.Equal(SqueezeFixAction.None, result.Action);
        }

        [Fact]
        public void AnimationNotFinished_ReturnsNone()
        {
            var result = SqueezeDecision.Evaluate(
                juiceLeftBefore: 0.31, juiceLeftNow: 0.31,
                animFinished: false, hasMash: true,
                squeezeRel: 1.5, pressSqueezeRel: 1.2,
                actualJuice: 0.31);

            Assert.Equal(SqueezeFixAction.None, result.Action);
        }

        [Fact]
        public void NoMashPresent_ReturnsNone()
        {
            var result = SqueezeDecision.Evaluate(
                juiceLeftBefore: 0.31, juiceLeftNow: 0.31,
                animFinished: true, hasMash: false,
                squeezeRel: 1.5, pressSqueezeRel: 1.2,
                actualJuice: 0.31);

            Assert.Equal(SqueezeFixAction.None, result.Action);
        }

        [Fact]
        public void VanillaWouldSucceed_SqueezeRelBelow1_PressBelow_ReturnsNone()
        {
            var result = SqueezeDecision.Evaluate(
                juiceLeftBefore: 5.0, juiceLeftNow: 5.0,
                animFinished: true, hasMash: true,
                squeezeRel: 0.5, pressSqueezeRel: 0.3,
                actualJuice: 5.0);

            Assert.Equal(SqueezeFixAction.None, result.Action);
        }

        [Fact]
        public void NoActualJuice_ReturnsNone()
        {
            var result = SqueezeDecision.Evaluate(
                juiceLeftBefore: 0.0, juiceLeftNow: 0.0,
                animFinished: true, hasMash: true,
                squeezeRel: 1.5, pressSqueezeRel: 1.2,
                actualJuice: 0.0);

            Assert.Equal(SqueezeFixAction.None, result.Action);
        }

        [Fact]
        public void SmallAmount_SqueezeRelAbove1_ReturnsFixValues()
        {
            var result = SqueezeDecision.Evaluate(
                juiceLeftBefore: 0.31, juiceLeftNow: 0.31,
                animFinished: true, hasMash: true,
                squeezeRel: 1.03, pressSqueezeRel: 0.5,
                actualJuice: 0.31);

            Assert.Equal(SqueezeFixAction.FixValues, result.Action);
            Assert.Equal(0.99, result.FixedValue);
        }

        [Fact]
        public void PressSqueezeRelAboveSqueezeRel_ReturnsFixValues()
        {
            var result = SqueezeDecision.Evaluate(
                juiceLeftBefore: 0.5, juiceLeftNow: 0.5,
                animFinished: true, hasMash: true,
                squeezeRel: 0.8, pressSqueezeRel: 0.9,
                actualJuice: 0.5);

            Assert.Equal(SqueezeFixAction.FixValues, result.Action);
            Assert.Equal(0.99, result.FixedValue);
        }

        [Fact]
        public void BothConditionsFailed_ReturnsFixValues()
        {
            var result = SqueezeDecision.Evaluate(
                juiceLeftBefore: 0.31, juiceLeftNow: 0.31,
                animFinished: true, hasMash: true,
                squeezeRel: 1.2, pressSqueezeRel: 1.5,
                actualJuice: 0.31);

            Assert.Equal(SqueezeFixAction.FixValues, result.Action);
            Assert.Equal(0.99, result.FixedValue);
        }

        [Fact]
        public void ZeroJuiceLeftButActualJuicePositive_ReturnsFixValues()
        {
            var result = SqueezeDecision.Evaluate(
                juiceLeftBefore: 0.0, juiceLeftNow: 0.0,
                animFinished: true, hasMash: true,
                squeezeRel: 1.5, pressSqueezeRel: 1.2,
                actualJuice: 0.31);

            Assert.Equal(SqueezeFixAction.FixValues, result.Action);
            Assert.Equal(0.99, result.FixedValue);
        }

        [Fact]
        public void VerySmallJuiceAmount_ReturnsFixValues()
        {
            var result = SqueezeDecision.Evaluate(
                juiceLeftBefore: 0.01, juiceLeftNow: 0.01,
                animFinished: true, hasMash: true,
                squeezeRel: 1.09, pressSqueezeRel: 0.5,
                actualJuice: 0.01);

            Assert.Equal(SqueezeFixAction.FixValues, result.Action);
            Assert.Equal(0.99, result.FixedValue);
        }

        [Fact]
        public void LargeJuiceAmount_VanillaWorks_ReturnsNone()
        {
            var result = SqueezeDecision.Evaluate(
                juiceLeftBefore: 10.0, juiceLeftNow: 10.0,
                animFinished: true, hasMash: true,
                squeezeRel: 0.1, pressSqueezeRel: 0.05,
                actualJuice: 10.0);

            Assert.Equal(SqueezeFixAction.None, result.Action);
        }

        [Fact]
        public void JuiceChangedWithinTolerance_TreatedAsUnchanged()
        {
            var result = SqueezeDecision.Evaluate(
                juiceLeftBefore: 0.31, juiceLeftNow: 0.3105,
                animFinished: true, hasMash: true,
                squeezeRel: 1.03, pressSqueezeRel: 0.5,
                actualJuice: 0.3105);

            Assert.Equal(SqueezeFixAction.FixValues, result.Action);
        }
    }
}
