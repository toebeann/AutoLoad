using HarmonyLib;

namespace Straitjacket.Subnautica.Mods.AutoLoad.Patch
{
    [HarmonyPatch(typeof(StartScreen))]
    [HarmonyPatch(nameof(StartScreen.OnGuiInitialized))]
    internal static class StartScreen_OnGuiInitialized
    {
        static bool Prefix(StartScreen __instance)
        {
            AutoLoad.RunCoroutine(AutoLoad.OnGuiInitialized(__instance));
            return false;
        }
    }
}
