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
            if (SaveLoadManager.main.firstStart == 0 // If the game has only just been launched
                && AutoLoad.StartScreen != __instance // and this is the first time this method has been called
                && !QModServices.Main.GetAllMods().Any(x => !x.IsLoaded)) // and all the mods loaded successfully
            {
                AutoLoad.StartScreen = __instance; // Register the StartScreen so we can call its methods later
                CoroutineHost.StartCoroutine(AutoLoad.StartScreen_Load()); // Use our custom load coroutine
                return false; // Don't call the original method
            }

            return true; // Otherwise, fall through to the original method
        }
    }
}
