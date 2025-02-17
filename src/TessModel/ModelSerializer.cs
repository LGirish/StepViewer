using System;
using System.IO;
using System.Text;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Newtonsoft.Json;


namespace TessModel
{
    public static class ModelSerializer
    {
        private static MessagePackSerializerOptions? messagepackOptions;

        public static byte[] Serialize<T>(T obj) =>
            MessagePackSerializer.Serialize(obj, GetOptions());

        public static void Serialize<T>(string fileName, T obj)
        {
            using var stream = File.Open(fileName, FileMode.Create);
            MessagePackSerializer.Serialize(stream, obj, GetOptions());
        }

        public static T? Deserialize<T>(byte[] bytes) =>
            (T?)MessagePackSerializer.Deserialize<T>(bytes, GetOptions());

        public static T? Deserialize<T>(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            return MessagePackSerializer.Deserialize<T>(stream, GetOptions());
        }

        public static T? ConvertFromJson<T>(TextReader reader)
        {
            var content = reader.ReadToEnd();
            var bytes = MessagePackSerializer.ConvertFromJson(content, GetOptions());
            return Deserialize<T>(bytes);
        }

        public static string ConvertToBase64<T>(T obj)
        {
            string jsonString = MessagePackSerializer.SerializeToJson(obj, GetOptions());
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            string base64String = Convert.ToBase64String(jsonBytes);
            return base64String;
        }

        public static string ConvertToJson<T>(T obj) =>
            MessagePackSerializer.ConvertToJson(MessagePackSerializer.Serialize(obj, GetOptions()));

        public static void ConvertToJson(string filePath)
        {
            byte[] binaryData = File.ReadAllBytes(filePath);
            var deserializedObject = MessagePackSerializer.Deserialize<dynamic>(binaryData);
            string jsonString = JsonConvert.SerializeObject(deserializedObject, Formatting.Indented);
            File.WriteAllText("output.json", jsonString);
        }
            
        internal static void SerializeToJson<T>(TextWriter writer, T obj) =>
            MessagePackSerializer.SerializeToJson(writer, obj, GetOptions());

        private static MessagePackSerializerOptions GetOptions()
        {
            if (messagepackOptions != null)
            {
                return messagepackOptions;
            }

            var composite = CompositeResolver.Create(
                ContractlessStandardResolver.Instance,
                DynamicEnumAsStringResolver.Instance,
                DynamicEnumResolver.Instance,
                StandardResolver.Instance);

            messagepackOptions = MessagePackSerializerOptions.Standard
                .WithResolver(CompositeResolver.Create(
                    new IMessagePackFormatter[]
                    {
                        new TriangleFormatter(),
                        new PointFormatter(),
                    },
                    new[] { composite }));

            return messagepackOptions;
        }
    }
}
