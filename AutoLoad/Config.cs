using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Json;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.AutoLoad
{
    internal class Config : ConfigFile
    {
        public KeyCode OverrideKey { get; set; } = KeyCode.Escape;
        public AutoLoadMode AutoLoadMode { get; set; } = AutoLoadMode.MostRecentlySaved;
        public KeyCode ToggleAutoLoadModeKey { get; set; } = KeyCode.LeftControl;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SpecificSaveSlot { get; set; } = null;
    }
}
