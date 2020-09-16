using HarmonyLib;

namespace Straitjacket.Subnautica.Mods.AutoLoad.Patches
{
    internal static class SaveLoadManagerPatch
    {
        [HarmonyPatch(typeof(SaveLoadManager), nameof(SaveLoadManager.StartNewSession))]
        [HarmonyPostfix]
        static void StartNewSessionPostfix(string __result)
        {
            var slotInfo = AutoLoad.MostRecentlyLoadedSlot;
            slotInfo.Session = __result;
            AutoLoad.MostRecentlyLoadedSlot = slotInfo;
        }
    }
}
