using AtelierResleriana.MessagePack;

namespace AtelierResleriana.MasterData
{
    public class MasterDataSerializer
    {
        public byte[] Serialize(string jsonString)
        {
            MessagePackSerializerOptions messagePackSerializerOptions = MessagePackSerializerOptions.Standard
                .WithCompression(MessagePackCompression.Lz4Block);

            return MessagePackSerializer.ConvertFromJson(jsonString, messagePackSerializerOptions);
        }

        public byte[] Serialize(object @object)
        {
            MessagePackSerializerOptions messagePackSerializerOptions = MessagePackSerializerOptions.Standard
                .WithCompression(MessagePackCompression.Lz4Block);

            return MessagePackSerializer.Serialize<dynamic>(@object, messagePackSerializerOptions);
        }

        public object Deserialize(byte[] bytes)
        {
            MessagePackSerializerOptions messagePackSerializerOptions = MessagePackSerializerOptions.Standard
                .WithCompression(MessagePackCompression.Lz4Block);

            return MessagePackSerializer.Deserialize<dynamic>(bytes, messagePackSerializerOptions);
        }
    }
}
