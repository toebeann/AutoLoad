using System;
using QModManager.API.ModLoading;

namespace Straitjacket.Subnautica.Mods.AutoLoad
{
    [QModCore]
    public static class Main
    {
        [QModPatch]
        [Obsolete("Should not be used!", true)]
        public static void ApplyPatches() => AutoLoad.Initialise();

        [QModPostPatch("B51B2A74117249DFF775B52A07FBDF72")]
        public static void PostPatch()
        {
            AutoLoad.CheckLoadedMods();
        }
    }
}
