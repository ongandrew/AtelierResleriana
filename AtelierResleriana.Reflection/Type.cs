namespace AtelierResleriana.Reflection
{
    public class Type
    {
        public string Assembly { get; set; }
        public string Name { get; set; }

        public TypeReference BaseType { get; set; }

        public Constructor[] Constructors { get; set; }
        public Field[] Fields { get; set; }
        public Property[] Properties { get; set; }
        public Method[] Methods { get; set; }
    }
}
