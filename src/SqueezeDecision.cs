using System;

namespace SqueezeItGood
{
    public enum SqueezeFixAction { None, FixValues }

    public struct SqueezeFixResult
    {
        public SqueezeFixAction Action;
        public double FixedValue;
    }

    /// <summary>
    /// Pure decision logic for determining whether to fix squeeze values.
    /// No game type dependencies — operates entirely on primitives.
    /// </summary>
    public static class SqueezeDecision
    {
        public static SqueezeFixResult Evaluate(
            double juiceLeftBefore,
            double juiceLeftNow,
            bool animFinished,
            bool hasMash,
            double squeezeRel,
            double pressSqueezeRel,
            double actualJuice)
        {
            var noFix = new SqueezeFixResult { Action = SqueezeFixAction.None };

            // If juice changed, normal extraction happened
            if (Math.Abs(juiceLeftNow - juiceLeftBefore) > 0.001)
                return noFix;

            if (!animFinished || !hasMash)
                return noFix;

            // Normal juicing requires: squeezeRel < 1 && pressSqueezeRel <= squeezeRel
            bool normalJuicingWouldFail = squeezeRel >= 1 || pressSqueezeRel > squeezeRel;
            if (!normalJuicingWouldFail)
                return noFix;

            if (actualJuice <= 0)
                return noFix;

            return new SqueezeFixResult
            {
                Action = SqueezeFixAction.FixValues,
                FixedValue = 0.99
            };
        }
    }
}
