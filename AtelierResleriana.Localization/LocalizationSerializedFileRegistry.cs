namespace AtelierResleriana.Localization
{
    public class LocalizationSerializedFileRegistry
    {
        public ISet<string> Whitelist { get; set; } = new HashSet<string>();
        public ISet<string> Blacklist { get; set; } = new HashSet<string>();
    }
}
