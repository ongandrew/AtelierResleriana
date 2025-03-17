namespace AtelierResleriana.Reflection
{
    public class Field
    {
        public TypeReference Type { get; set; }
        public bool IsPublic { get; set; }
        public bool IsStatic { get; set; }
        public string Name { get; set; }
    }
}
