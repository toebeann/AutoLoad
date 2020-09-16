using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.AutoLoad
{
    [Menu("AutoLoad", LoadOn = MenuAttribute.LoadEvents.MenuRegistered | MenuAttribute.LoadEvents.MenuOpened)]
    internal class Config : ConfigFile
    {
        [Choice("Pause after AutoLoad", "Off", "AutoLoaded saves only", "All saves")]
        public AutoLoadPause PauseOnLoad { get; set; } = AutoLoadPause.Off;

        [Choice("AutoLoad mode", "Most recently saved", "Most recently loaded")]
        public AutoLoadMode AutoLoadMode { get; set; } = AutoLoadMode.MostRecentlySaved;

        [Keybind("Skip AutoLoad on launch")]
        public KeyCode OverrideKey { get; set; } = KeyCode.Escape;

        [Keybind("Toggle most recent load mode")]
        public KeyCode ToggleAutoLoadModeKey { get; set; } = KeyCode.LeftControl;

        [Toggle("Start new game with most recent game mode")]
        public bool StartNewGame { get; set; } = true;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SpecificSaveSlot { get; set; } = null;
    }
}
