using SMLHelper.V2.Options;

namespace Straitjacket.Subnautica.Mods.AutoLoad
{
    internal class Options : ModOptions
    {
        public override void BuildModOptions()
        {
            AddKeybindOption("overrideKey", "Skip AutoLoad on launch",
                GameInput.GetPrimaryDevice(), AutoLoad.Config.OverrideKey);
        }

        public Options() : base("AutoLoad")
        {
            KeybindChanged += Options_KeybindChanged;
        }

        private void Options_KeybindChanged(object sender, KeybindChangedEventArgs e)
        {
            switch (e.Id)
            {
                case "overrideKey":
                    AutoLoad.Config.OverrideKey = e.Key;
                    break;
            }
            AutoLoad.Config.Save();
        }
    }
}
