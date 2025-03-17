using System.IO;
using System.Text;
using Universal.Common;

namespace AtelierResleriana.Text
{
    public class TextAssetReader
    {
        public TextAsset Read(Stream stream)
        {
            using BinaryReader binaryReader = new BinaryReader(stream, Encoding.ASCII, true, Endian.Little);
            int nameLength = binaryReader.ReadInt32();
            string name = Encoding.UTF8.GetString(binaryReader.ReadBytes(nameLength));
            binaryReader.Align(4);
            int scriptLength = binaryReader.ReadInt32();
            byte[] scriptBytes = binaryReader.ReadBytes(scriptLength);

            return new TextAsset()
            {
                Name = name,
                Script = scriptBytes
            };
        }

        public TextAsset Read(byte[] bytes)
        {
            using Stream stream = new MemoryStream(bytes);
            return Read(stream);
        }
    }
}
