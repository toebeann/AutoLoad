using SMLHelper.V2.Options;
using SMLHelper.V2.Utility;

namespace Straitjacket.Subnautica.Mods.AutoLoad
{
    internal class Options : ModOptions
    {
        public override void BuildModOptions()
        {
            AddChoiceOption("mode", "AutoLoad mode", new string[] { "Most recently saved", "Most recently loaded" },
                (int)AutoLoad.Config.AutoLoadMode);
            AddKeybindOption("overrideKey", "Skip AutoLoad on launch",
                GameInput.GetPrimaryDevice(), AutoLoad.Config.OverrideKey);
            AddKeybindOption("overrideModeKey", "Toggle most recent load mode",
                GameInput.GetPrimaryDevice(), AutoLoad.Config.ToggleAutoLoadModeKey);
        }

        public Options() : base("AutoLoad")
        {
            ChoiceChanged += Options_ChoiceChanged;
            KeybindChanged += Options_KeybindChanged;
        }

        private void Options_ChoiceChanged(object sender, ChoiceChangedEventArgs e)
        {

            switch (e.Id)
            {
                case "mode":
                    AutoLoad.Config.AutoLoadMode = (AutoLoadMode)e.Index;
                    break;
            }
            JsonUtils.Save(AutoLoad.Config);
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
            JsonUtils.Save(AutoLoad.Config);
        }
    }
}
