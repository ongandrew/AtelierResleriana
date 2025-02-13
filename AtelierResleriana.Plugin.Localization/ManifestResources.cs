using System;
using System.IO;

namespace AtelierResleriana.Plugin.Localization
{
    public class ManifestResources
    {
        public Type Root { get; init; }

        public ManifestResources(Type root)
        {
            Root = root;
        }

        public byte[] GetBytes(string name)
        {
            using (Stream stream = GetStream(name))
            {
                if (stream == null)
                    throw new FileNotFoundException($"Resource not found: {name}");

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        public Stream GetStream(string name)
        {
            return Root.Assembly.GetManifestResourceStream(Root, name);
        }

        public string GetString(string name)
        {
            using (Stream stream = GetStream(name))
            {
                if (stream == null)
                    throw new FileNotFoundException($"Resource not found: {name}");

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

    }
}
