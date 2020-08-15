using HarmonyLib;

namespace Straitjacket.Subnautica.Mods.AutoLoad.Patch
{
    [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
    internal static class Player_Awake
    {
        static void Prefix()
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

        static void Postfix()
        {
            AutoLoad.RunCoroutine(AutoLoad.PauseOnLoad());
        }
    }
}
