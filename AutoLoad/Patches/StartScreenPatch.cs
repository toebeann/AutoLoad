using HarmonyLib;

namespace Straitjacket.Subnautica.Mods.AutoLoad.Patches
{
    internal static class StartScreenPatch
    {
        [HarmonyPatch(typeof(StartScreen), nameof(StartScreen.OnGuiInitialized))]
        [HarmonyPrefix]
        static bool OnGuiInitializedPrefix(StartScreen __instance)
        {
            AutoLoad.RunCoroutine(AutoLoad.OnGuiInitialized(__instance));
            return false;
        }
    }
}
