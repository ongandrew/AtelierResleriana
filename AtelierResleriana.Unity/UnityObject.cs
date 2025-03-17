namespace AtelierResleriana.Unity
{
    public abstract class UnityObject
    {
        public string Name { get; set; }

        protected abstract void Deserialize(SerializedObject serializedObject);

        public static T FromSerializedObject<T>(SerializedObject serializedObject)
            where T : UnityObject
        {
            T instance = Activator.CreateInstance<T>();
            instance.Name = serializedObject.GetValue<string>("m_Name");
            instance.Deserialize(serializedObject);
            return instance;
        }
    }
}
