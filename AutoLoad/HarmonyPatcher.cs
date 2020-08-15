using HarmonyLib;
using QModManager.API.ModLoading;

namespace Straitjacket.Subnautica.Mods.AutoLoad
{
    [QModCore]
    public class HarmonyPatcher
    {
        [QModPatch]
        public static void ApplyPatches()
        {
            new Harmony("com.tobeyblaber.straitjacket.subnautica.autoload.mod").PatchAll();
            AutoLoad.Initialise();
        }

        [QModPostPatch("B51B2A74117249DFF775B52A07FBDF72")]
        public static void PostPatch()
        {
            AutoLoad.CheckLoadedMods();
        }
    }
}
