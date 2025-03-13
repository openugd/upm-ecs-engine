using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using OpenUGD.ECS.Engine.Utils.Converters;

namespace OpenUGD.ECS.Engine.Utils
{
    public static class Serializer
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new EnumIntegerConverter(),
                new InputConverter()
            }
        };

        public static byte[] Serialize(object value)
        {
            var json = JsonConvert.SerializeObject(value, Formatting.Indented, SerializerSettings);
            var bytes = Encoding.UTF8.GetBytes(json);
            return bytes;
        }

        public static object Deserialize(Type type, byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject(json, type, SerializerSettings);
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
        }
    }
}