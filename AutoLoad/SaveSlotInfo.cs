namespace Straitjacket.Subnautica.Mods.AutoLoad
{
    internal class SaveSlotInfo
    {
        public string SaveGame { get; set; }
#pragma warning disable CS0618 // Type or member is obsolete
        public GameMode GameMode { get; set; }
#pragma warning restore CS0618 // Type or member is obsolete
        public string Session { get; set; }
    }
}
