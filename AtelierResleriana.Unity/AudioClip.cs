namespace AtelierResleriana.Unity
{
    public class AudioClip : UnityObject
    {
        [SerializedFileObjectPropertyName("m_LoadType")]
        public int LoadType { get; set; }

        [SerializedFileObjectPropertyName("m_Channels")]
        public int Channels { get; set; }

        [SerializedFileObjectPropertyName("m_Frequency")]
        public int Frequency { get; set; }

        [SerializedFileObjectPropertyName("m_BitsPerSample")]
        public int BitsPerSample { get; set; }

        [SerializedFileObjectPropertyName("m_Length")]
        public float Length { get; set; }

        [SerializedFileObjectPropertyName("m_IsTrackerFormat")]
        public bool IsTrackerFormat { get; set; }

        [SerializedFileObjectPropertyName("m_Ambisonic")]
        public bool Ambisonic { get; set; }

        [SerializedFileObjectPropertyName("m_SubsoundIndex")]
        public int SubsoundIndex { get; set; }

        [SerializedFileObjectPropertyName("m_PreloadAudioData")]
        public bool PreloadAudioData { get; set; }

        [SerializedFileObjectPropertyName("m_LoadInBackground")]
        public bool LoadInBackground { get; set; }

        [SerializedFileObjectPropertyName("m_Legacy3D")]
        public bool Legacy3D { get; set; }

        [SerializedFileObjectPropertyName("m_CompressionFormat")]
        public AudioCompressionFormat CompressionFormat { get; set; }

        [SerializedFileObjectPropertyName("m_AudioData")]
        public byte[]? AudioData { get; set; }

        [SerializedFileObjectPropertyName("m_Resource")]
        public ResourceInfo? Resource { get; set; }

        protected override void Deserialize(SerializedObject serializedObject)
        {
            // Implementation will be done through reflection via the attributes
        }

        public class ResourceInfo
        {
            [SerializedFileObjectPropertyName("m_Source")]
            public string Source { get; set; }

            [SerializedFileObjectPropertyName("m_Offset")]
            public ulong Offset { get; set; }

            [SerializedFileObjectPropertyName("m_Size")]
            public ulong Size { get; set; }
        }
    }
}
