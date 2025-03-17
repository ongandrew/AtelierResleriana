using System;
using System.IO;
using System.Reflection;

namespace AtelierResleriana.Plugin.Localization
{
    public class ManifestResources
    {
        public Assembly Assembly { get; set; }
        public Type Root { get; set; }

        public ManifestResources(Assembly assembly, Type root)
        {
            Assembly = assembly;
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
            return Assembly.GetManifestResourceStream(Root, name);
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
