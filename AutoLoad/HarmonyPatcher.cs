using System.Reflection;
using Harmony;

namespace Straitjacket.Subnautica.Mods.AutoLoad
{
    internal class HarmonyPatcher
    {
        public static void ApplyPatches()
        {
            var harmony = HarmonyInstance.Create("com.tobeyblaber.straitjacket.subnautica.autoload.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
