using System;
using System.IO;
using System.Text;

namespace AtelierResleriana.Text
{
    public class PackedTextWriter
    {
        public Stream Write(PackedText packedText)
        {
            var stream = new MemoryStream();
            Write(stream, packedText);
            stream.Position = 0;
            return stream;
        }

        public void Write(Stream stream, PackedText packedText)
        {
            using var binaryWriter = new BinaryWriter(stream, Encoding.UTF8, true);

            binaryWriter.Write((uint)packedText.Properties.Count);
            binaryWriter.Write((uint)packedText.Entries.Count);

            foreach (var prop in packedText.Properties)
            {
                binaryWriter.Write(prop.Type);
                binaryWriter.Write(prop.Id);
            }

            foreach (var entry in packedText.Entries)
            {
                foreach (var prop in packedText.Properties)
                {
                    var value = entry[prop.Id];
                    switch (prop.Type)
                    {
                        case PropertyTypes.UnsignedInteger:
                            binaryWriter.Write(Convert.ToUInt32(value));
                            break;

                        case PropertyTypes.UnsignedLong:
                            binaryWriter.Write(Convert.ToUInt64(value));
                            break;

                        case PropertyTypes.String:
                            var strValue = value.ToString();
                            var strBytes = Encoding.UTF8.GetBytes(strValue);
                            binaryWriter.Write((uint)strBytes.Length);
                            binaryWriter.Write(strBytes);
                            break;
                    }
                }
            }
        }
    }
}