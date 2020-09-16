using HarmonyLib;

namespace Straitjacket.Subnautica.Mods.AutoLoad.Patches
{
    internal static class PlayerPatch
    {
        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        [HarmonyPrefix]
        static void AwakePrefix()
        {
            var slot = SaveLoadManager.main.GetCurrentSlot();
            var gameInfo = SaveLoadManager.main.GetGameInfo(slot);
            if (gameInfo != null)
            {
                AutoLoad.MostRecentlyLoadedSlot = new SaveSlotInfo
                {
                    SaveGame = slot,
                    GameMode = gameInfo.gameMode,
                    Session = gameInfo.session
                };
            }
            else
            {
                AutoLoad.MostRecentlyLoadedSlot = new SaveSlotInfo
                {
                    SaveGame = slot,
                    GameMode = Utils.GetLegacyGameMode(),
                    Session = SaveLoadManager.main.sessionId
                };
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        [HarmonyPostfix]
        static void AwakePostfix()
        {
            AutoLoad.RunCoroutine(AutoLoad.PauseOnLoad());
        }
    }
}
