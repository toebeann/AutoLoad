using SMLHelper.V2.Options;
using SMLHelper.V2.Utility;

namespace Straitjacket.Subnautica.Mods.AutoLoad
{
    internal class Options : ModOptions
    {
        public override void BuildModOptions()
        {
            AddToggleOption("pauseOnLoad", "Pause after AutoLoad", AutoLoad.Config.PauseOnLoad);
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
            ToggleChanged += Options_ToggleChanged;
            ChoiceChanged += Options_ChoiceChanged;
            KeybindChanged += Options_KeybindChanged;
        }

        private void Options_ToggleChanged(object sender, ToggleChangedEventArgs e)
        {
            switch (e.Id)
            {
                case "pauseOnLoad":
                    AutoLoad.Config.PauseOnLoad = e.Value;
                    break;
                case "startNewGame":
                    AutoLoad.Config.StartNewGame = e.Value;
                    break;
            }
            AutoLoad.Config.Save();
        }

        private void Options_ChoiceChanged(object sender, ChoiceChangedEventArgs e)
        {

            switch (e.Id)
            {
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
    }
}
