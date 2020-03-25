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
            if (AutoLoad.CheckJustLaunched()
                && AutoLoad.StartScreen != __instance)
            {
                Console.WriteLine("[AutoLoad] Initialising...");
                Console.WriteLine("[AutoLoad] Checking all mods loaded succesfully...");

                
                var modsNotLoaded = AutoLoad.GetFailedMods();
                if (modsNotLoaded.Any())
                {
                    Console.WriteLine("[AutoLoad] Detected the following mods were not loaded:");

                    foreach (var mod in modsNotLoaded)
                    {
                        Console.WriteLine($"[AutoLoad]     {mod.DisplayName}");
                    }

                    Console.WriteLine("[AutoLoad] Skipping AutoLoad.");
                }
                else
                {
                    AutoLoad.StartScreen = __instance; // Register the StartScreen so we can call its methods later

                    Console.WriteLine("[AutoLoad] All mods loaded successfully, checking saves...");
                    CoroutineHost.StartCoroutine(AutoLoad.StartScreen_Load()); // Use our custom load coroutine
                    return false; // Don't call the original method
                }
            }

            return true; // Otherwise, fall through to the original method
        }
    }
}
