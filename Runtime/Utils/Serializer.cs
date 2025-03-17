using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using OpenUGD.ECS.Engine.Utils.Converters;

namespace OpenUGD.ECS.Engine.Utils
{
    public class Serializer
    {
        public readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            TypeNameHandling = TypeNameHandling.All
        };

        public Serializer(Action<JsonSerializerSettings> settings = null)
        {
            if (settings != null)
            {
                settings(Settings);
            }
        }

        public T Clone<T>(T obj)
        {
            var serialized = Serialize(obj);
            return Deserialize<T>(serialized);
        }

        public byte[] Serialize(object value)
        {
            var json = JsonConvert.SerializeObject(value, Settings);
            var bytes = Encoding.UTF8.GetBytes(json);
            return bytes;
        }

        public object Deserialize(Type type, byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject(json, type, Settings);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }
    }
}