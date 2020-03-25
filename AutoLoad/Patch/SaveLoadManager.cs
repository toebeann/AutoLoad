using System;
using System.Linq;
using Harmony;
using QModManager.API;
using UWE;

namespace Straitjacket.Subnautica.Mods.AutoLoad.Patch
{
    [HarmonyPatch(typeof(SaveLoadManager))]
    [HarmonyPatch("Awake")]
    internal static class SaveLoadManager_Awake
    {
        static void Postfix(SaveLoadManager __instance)
        {
            if (AutoLoad.CheckJustLaunched() && !AutoLoad.CheckAnyFailedMods())
            {
                // Start loading the save slots immediately, as long as no mods have failed to load.
                __instance._earlySlotLoading = CoroutineHost.StartCoroutine(__instance.LoadSlotsAsync());
            }
        }
    }
}
