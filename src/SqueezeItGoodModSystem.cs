using Vintagestory.API.Common;
using HarmonyLib;
using System;

namespace SqueezeItGood
{
    public class SqueezeItGoodModSystem : ModSystem
    {
        private Harmony harmony;
        public static ICoreAPI Api { get; private set; }
        public static bool DebugLogging = false;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            Api = api;

            try
            {
                harmony = new Harmony("com.squeezeitgood.fruitpressmod");
                harmony.PatchAll(typeof(SqueezeItGoodModSystem).Assembly);

                // Log what was patched
                foreach (var method in harmony.GetPatchedMethods())
                {
                    api.Logger.Notification($"[SqueezeItGood] Patched: {method.DeclaringType?.Name}.{method.Name}");
                }

                api.Logger.Notification("[SqueezeItGood] Fruit press patches applied!");
            }
            catch (Exception ex)
            {
                api.Logger.Error($"[SqueezeItGood] Failed: {ex.Message}");
                api.Logger.Error($"[SqueezeItGood] {ex.StackTrace}");
            }
        }

        public static void Log(string message)
        {
            if (DebugLogging && Api != null)
            {
                Api.Logger.Notification("[SqueezeItGood] " + message);
            }
        }

        public override void Dispose()
        {
            harmony?.UnpatchAll("com.squeezeitgood.fruitpressmod");
            base.Dispose();
        }
    }
}
