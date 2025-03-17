using System.Reflection.Metadata;

namespace AtelierResleriana.Reflection
{
    public class Method
    {
        public TypeReference? ReturnType { get; set; }
        public bool IsPublic { get; set; }
        public bool IsStatic { get; set; }
        public string Name { get; set; }
        public Parameter[] Parameters { get; set; }
    }
}
