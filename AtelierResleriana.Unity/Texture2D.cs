namespace AtelierResleriana.Unity
{
    public class Texture2D : UnityObject
    {
        [SerializedFileObjectPropertyName("m_ForcedFallbackFormat")]
        public int ForcedFallbackFormat { get; set; }
        [SerializedFileObjectPropertyName("m_DownscaleFallback")]
        public bool DownscaleFallback { get; set; }
        [SerializedFileObjectPropertyName("m_IsAlphaChannelOptional")]
        public bool IsAlphaChannelOptional { get; set; }
        [SerializedFileObjectPropertyName("m_Width")]
        public int Width { get; set; }
        [SerializedFileObjectPropertyName("m_Height")]
        public int Height { get; set; }
        [SerializedFileObjectPropertyName("m_CompleteImageSize")]
        public int CompleteImageSize { get; set; }
        [SerializedFileObjectPropertyName("m_MipsStripped")]
        public int MipsStripped { get; set; }
        [SerializedFileObjectPropertyName("m_TextureFormat")]
        public TextureFormat TextureFormat { get; set; }
        [SerializedFileObjectPropertyName("m_MipCount")]
        public int MipCount { get; set; }
        [SerializedFileObjectPropertyName("m_IsReadable")]
        public bool IsReadable { get; set; }
        [SerializedFileObjectPropertyName("m_IsPreProcessed")]
        public bool IsPreProcessed { get; set; }
        [SerializedFileObjectPropertyName("m_IgnoreMipmapLimit")]
        public bool IgnoreMipmapLimit { get; set; }
        [SerializedFileObjectPropertyName("m_MipmapLimitGroupName")]
        public string MipmapLimitGroupName { get; set; }
        [SerializedFileObjectPropertyName("m_StreamingMipmaps")]
        public bool StreamingMipmaps { get; set; }
        [SerializedFileObjectPropertyName("m_StreamingMipmapsPriority")]
        public int StreamingMipmapsPriority { get; set; }
        [SerializedFileObjectPropertyName("m_ImageCount")]
        public int ImageCount { get; set; }
        [SerializedFileObjectPropertyName("m_TextureDimension")]
        public int TextureDimension { get; set; }
        [SerializedFileObjectPropertyName("m_TextureSettings")]
        public TextureSettingsInfo TextureSettings { get; set; }
        [SerializedFileObjectPropertyName("m_LightmapFormat")]
        public int LightmapFormat { get; set; }
        [SerializedFileObjectPropertyName("m_ColorSpace")]
        public int ColorSpace { get; set; }
        [SerializedFileObjectPropertyName("image data")]
        public byte[] ImageData { get; set; }
        [SerializedFileObjectPropertyName("m_StreamData")]
        public StreamDataInfo? StreamData { get; set; }

        protected override void Deserialize(SerializedObject serializedObject)
        {
            // Implementation will go here
        }

        public class TextureSettingsInfo
        {
            [SerializedFileObjectPropertyName("m_FilterMode")]
            public int FilterMode { get; set; }
            [SerializedFileObjectPropertyName("m_Aniso")]
            public int Aniso { get; set; }
            [SerializedFileObjectPropertyName("m_MipBias")]
            public int MipBias { get; set; }
            [SerializedFileObjectPropertyName("m_WrapU")]
            public int WrapU { get; set; }
            [SerializedFileObjectPropertyName("m_WrapV")]
            public int WrapV { get; set; }
            [SerializedFileObjectPropertyName("m_WrapW")]
            public int WrapW { get; set; }
        }

        public class StreamDataInfo
        {
            [SerializedFileObjectPropertyName("offset")]
            public ulong Offset { get; set; }
            [SerializedFileObjectPropertyName("size")]
            public uint Size { get; set; }
            [SerializedFileObjectPropertyName("path")]
            public string Path { get; set; }
        }
    }
}