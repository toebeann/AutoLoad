using Harmony;

namespace Straitjacket.Subnautica.Mods.AutoLoad.Patch
{
    [HarmonyPatch(typeof(SaveLoadManager), nameof(SaveLoadManager.StartNewSession))]
    internal static class SaveLoadManager_StartNewSession
    {
        static void Postfix(string __result)
        {
            var slotInfo = AutoLoad.MostRecentlyLoadedSlot;
            slotInfo.Session = __result;
            AutoLoad.MostRecentlyLoadedSlot = slotInfo;
        }
    }
}
