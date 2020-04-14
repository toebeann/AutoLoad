﻿using SMLHelper.V2.Options;
using SMLHelper.V2.Utility;

namespace Straitjacket.Subnautica.Mods.AutoLoad
{
    internal class Options : ModOptions
    {
        public override void BuildModOptions()
        {
            AddChoiceOption("pauseOnLoad", "Pause after AutoLoad", new string[] { "Off", "AutoLoaded saves only", "All saves" },
                (int)AutoLoad.Config.PauseOnLoad);
            AddChoiceOption("mode", "AutoLoad mode", new string[] { "Most recently saved", "Most recently loaded" },
                (int)AutoLoad.Config.AutoLoadMode);
            AddKeybindOption("overrideKey", "Skip AutoLoad on launch",
                GameInput.GetPrimaryDevice(), AutoLoad.Config.OverrideKey);
            AddKeybindOption("overrideModeKey", "Toggle most recent load mode",
                GameInput.GetPrimaryDevice(), AutoLoad.Config.ToggleAutoLoadModeKey);
            AddToggleOption("startNewGame", "Start new game with most recent game mode", AutoLoad.Config.StartNewGame);
        }

        public Options() : base("AutoLoad")
        {
            ChoiceChanged += Options_ChoiceChanged;
            KeybindChanged += Options_KeybindChanged;
            ToggleChanged += Options_ToggleChanged;
        }

        private void Options_ChoiceChanged(object sender, ChoiceChangedEventArgs e)
        {

            switch (e.Id)
            {
                case "pauseOnLoad":
                    AutoLoad.Config.PauseOnLoad = (AutoLoadPause)e.Index;
                    break;
                case "mode":
                    AutoLoad.Config.AutoLoadMode = (AutoLoadMode)e.Index;
                    break;
            }
            AutoLoad.Config.Save();
        }

        private void Options_KeybindChanged(object sender, KeybindChangedEventArgs e)
        {
            switch (e.Id)
            {
                case "overrideKey":
                    AutoLoad.Config.OverrideKey = e.Key;
                    break;
                case "overrideModeKey":
                    AutoLoad.Config.ToggleAutoLoadModeKey = e.Key;
                    break;
            }
            AutoLoad.Config.Save();
        }

        private void Options_ToggleChanged(object sender, ToggleChangedEventArgs e)
        {
            switch (e.Id)
            {
                case "startNewGame":
                    AutoLoad.Config.StartNewGame = e.Value;
                    break;
            }
            AutoLoad.Config.Save();
        }
    }
}
