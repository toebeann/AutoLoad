using Harmony;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.AutoLoad.Patch
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch(nameof(Player.Awake))]
    internal static class Player_Awake
    {
        static void Prefix()
        {
            AutoLoad.MostRecentlyLoadedSlot = SaveLoadManager.main.GetCurrentSlot();
        }
    }
}
