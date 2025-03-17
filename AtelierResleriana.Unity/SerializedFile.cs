namespace AtelierResleriana.Unity
{
    public class SerializedFile
    {
        public SerializedFileHeader Header { get; set; }
        public SerializedFileMetadata Metadata { get; set; }
        public SerializedFileType[] Types { get; set; }
        public SerializedFileObject[] Objects { get; set; }
        public SerializedFileObjectReference[] ScriptReferences { get; set; }
        public SerializedFileAssetReference[] AssetReferences { get; set; }
        public SerializedFileType[] ReferenceTypes { get; set; }
        public string? UserInformation { get; set; }

        public SerializedObject GetSerializedObject(SerializedFileObject serializedFileObject)
        {
            if (serializedFileObject.Data == null || serializedFileObject.Type?.Node == null)
            {
                return null;
            }

            using Stream stream = new MemoryStream(serializedFileObject.Data);
            var reader = new SerializedObjectReader();
            return reader.Read(stream, Header, serializedFileObject);
        }

        public T GetUnityObject<T>(SerializedFileObject serializedFileObject) where T : UnityObject
        {
            var serializedObject = GetSerializedObject(serializedFileObject);
            if (serializedObject == null)
                return null;

            return UnityObject.FromSerializedObject<T>(serializedObject);
        }

        public T GetUnityObject<T>(long pathId) where T : UnityObject
        {
            var obj = Objects.FirstOrDefault(o => o.PathId == pathId);
            if (obj == null)
                return null;

            return GetUnityObject<T>(obj);
        }
    }
}
