namespace AtelierResleriana.Localization
{
    public class PackedTextEntryLocalization
    {
        public uint Id { get; set; }
        public Dictionary<uint, Property> Properties { get; set; } = new Dictionary<uint, Property>();

        public class Property
        {
            public string Text { get; set; }
            public Dictionary<string, string> Localizations { get; set; } = new Dictionary<string, string>();
        }
    }
}
