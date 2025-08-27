namespace AtelierResleriana.Unity
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SerializedFileObjectPropertyNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public SerializedFileObjectPropertyNameAttribute(string name)
        {
            Name = name;
        }
    }
}
