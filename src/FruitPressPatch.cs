using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using System.Reflection;
using System;

namespace SqueezeItGood
{
    /// <summary>
    /// Patches BlockEntityFruitPress to allow squeezing any amount of fruit/honey.
    ///
    /// The Problem: For small amounts of fruit, the vanilla code calculates a squeezeRel >= 1,
    /// which fails the extraction condition (squeezeRel less than 1). The screw animation plays
    /// but no juice is extracted.
    ///
    /// The Fix: When we detect that extraction should happen but didn't (animation finished,
    /// mash present, threshold check failed), we adjust squeezeRel and pressSqueezeRel to 0.99
    /// so vanilla extraction can proceed on the next tick.
    /// </summary>
    [HarmonyPatch(typeof(BlockEntityFruitPress))]
    public static class FruitPressPatch
    {
        private static PropertyInfo juiceableLitresLeftProp;
        private static PropertyInfo juiceableLitresTransferedProp;
        private static FieldInfo pressSqueezeRelField;
        private static MethodInfo getJuiceablePropsMethod;
        private static bool initialized = false;

        private static void Init()
        {
            if (initialized) return;
            initialized = true;

            var type = typeof(BlockEntityFruitPress);
            var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

            juiceableLitresLeftProp = type.GetProperty("juiceableLitresLeft", flags);
            juiceableLitresTransferedProp = type.GetProperty("juiceableLitresTransfered", flags);
            pressSqueezeRelField = type.GetField("pressSqueezeRel", flags);
            getJuiceablePropsMethod = type.GetMethod("getJuiceableProps", flags);
        }

        /// <summary>
        /// Track juice level before tick to detect if normal extraction happened.
        /// </summary>
        [HarmonyPatch("onTick100msServer")]
        [HarmonyPrefix]
        public static void OnTick_Prefix(BlockEntityFruitPress __instance, out double __state)
        {
            __state = 0;
            try
            {
                Init();
                if (juiceableLitresLeftProp == null) return;
                __state = Convert.ToDouble(juiceableLitresLeftProp.GetValue(__instance) ?? 0);
            }
            catch
            {
                // Silently ignore errors in prefix
            }
        }

        /// <summary>
        /// After onTick100msServer, check if normal juicing failed and fix values if needed.
        /// </summary>
        [HarmonyPatch("onTick100msServer")]
        [HarmonyPostfix]
        public static void OnTick_Postfix(BlockEntityFruitPress __instance, double __state)
        {
            try
            {
                Init();
                if (juiceableLitresLeftProp == null) return;

                double juiceLeftBefore = __state;
                double juiceLeftNow = Convert.ToDouble(juiceableLitresLeftProp.GetValue(__instance) ?? 0);

                // If juice changed, normal extraction happened - do nothing
                if (Math.Abs(juiceLeftNow - juiceLeftBefore) > 0.001)
                {
                    return;
                }

                // Only intervene when animation is at the bottom and there's mash
                bool animFinished = __instance.CompressAnimFinished;
                bool hasMash = __instance.MashSlot != null && !__instance.MashSlot.Empty;

                if (!animFinished || !hasMash)
                {
                    return;
                }

                // Get squeeze values
                double pressSqueezeRel = 0;
                if (pressSqueezeRelField != null)
                {
                    pressSqueezeRel = Convert.ToDouble(pressSqueezeRelField.GetValue(__instance) ?? 0);
                }

                double squeezeRel = 0;
                var mashStack = __instance.MashSlot.Itemstack;
                if (mashStack?.Attributes != null)
                {
                    squeezeRel = mashStack.Attributes.GetDouble("squeezeRel", 0);
                }

                // Normal juicing requires: squeezeRel < 1 && pressSqueezeRel <= squeezeRel
                // If those conditions failed, we need to fix the values
                bool normalJuicingWouldFail = squeezeRel >= 1 || pressSqueezeRel > squeezeRel;

                if (!normalJuicingWouldFail)
                {
                    return;
                }

                // If juiceLeft is 0, calculate it from the mash
                double actualJuice = juiceLeftNow;
                if (actualJuice <= 0 && getJuiceablePropsMethod != null)
                {
                    var props = getJuiceablePropsMethod.Invoke(__instance, new object[] { mashStack });
                    if (props != null)
                    {
                        var propsType = props.GetType();
                        var litresPerItemProp = propsType.GetProperty("LitresPerItem");
                        if (litresPerItemProp != null)
                        {
                            double litresPerItem = Convert.ToDouble(litresPerItemProp.GetValue(props) ?? 0);
                            actualJuice = litresPerItem * mashStack.StackSize;
                            juiceableLitresLeftProp?.SetValue(__instance, actualJuice);
                        }
                    }
                }

                if (actualJuice <= 0)
                {
                    return;
                }

                // Fix the values so vanilla code can extract on next tick
                // Vanilla requires: squeezeRel < 1 AND pressSqueezeRel <= squeezeRel
                // Set both to 0.99 to satisfy both conditions
                double fixedValue = 0.99;

                if (mashStack?.Attributes != null && squeezeRel >= 1)
                {
                    mashStack.Attributes.SetDouble("squeezeRel", fixedValue);
                }

                if (pressSqueezeRelField != null && pressSqueezeRel > fixedValue)
                {
                    pressSqueezeRelField.SetValue(__instance, fixedValue);
                }

                __instance.MarkDirty(true);
            }
            catch
            {
                // Silently ignore errors in postfix
            }
        }
    }
}
