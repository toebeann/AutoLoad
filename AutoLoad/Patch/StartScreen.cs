using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using QModManager.API;
using UWE;

namespace Straitjacket.Subnautica.Mods.AutoLoad.Patch
{
    [HarmonyPatch(typeof(StartScreen))]
    [HarmonyPatch("OnGuiInitialized")]
    internal static class StartScreen_OnGuiInitialized
    {
        static bool Prefix(StartScreen __instance)
        {
            AutoLoad.RunCoroutine(AutoLoad.OnGuiInitialized(__instance));
            return false;
        }
    }
}
