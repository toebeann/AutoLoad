using System.Reflection;
using Harmony;
using QModManager.API.ModLoading;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.AutoLoad
{
    [QModCore]
    public class HarmonyPatcher
    {
        [QModPatch]
        public static void ApplyPatches()
        {
            var harmony = HarmonyInstance.Create("com.tobeyblaber.straitjacket.subnautica.autoload.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [QModPostPatch("B51B2A74117249DFF775B52A07FBDF72")]
        public static void PostPatch()
        {
            AutoLoad.CheckLoadedMods();
        }
    }
}
