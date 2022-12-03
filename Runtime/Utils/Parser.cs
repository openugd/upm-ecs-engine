using System;
using System.Text;
using Newtonsoft.Json;

namespace OpenUGD.ECS.Engine.Utils
{
    public static class Parser
    {
        private static readonly JsonSerializerSettings DefaultSerializeSettings = CreateDefaultSettings();

        private static JsonSerializerSettings CreateDefaultSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new EnumIntegerConverter());
            return settings;
        }

        public static byte[] Serialize(object value)
        {
            var json = JsonConvert.SerializeObject(value, Formatting.Indented, DefaultSerializeSettings);
            var bytes = Encoding.UTF8.GetBytes(json);
            return bytes;
        }

        public static object Deserialize(Type type, byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject(json, type);
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json);
        }

        private class EnumIntegerConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                }
                else
                {
                    var type = Enum.GetUnderlyingType(value.GetType());
                    var result = Convert.ChangeType(value, type);
                    writer.WriteValue(result);
                }
            }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
                JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return 0;

                var flag = IsNullableType(objectType);
                var type = flag ? Nullable.GetUnderlyingType(objectType) : objectType;
                if (reader.TokenType == JsonToken.String)
                {
                    var str = reader.Value.ToString();
                    if ((str == string.Empty) & flag)
                        return 0;
                    return Enum.Parse(objectType, existingValue.ToString(), true);
                }

                if (reader.TokenType == JsonToken.Integer) return Enum.ToObject(objectType, existingValue);

                return existingValue;
            }

            public override bool CanConvert(Type objectType)
            {
                if (objectType == null) return false;

                if (IsNullableType(objectType))
                {
                    var type = Nullable.GetUnderlyingType(objectType);
                    return type != null && type.IsEnum;
                }

                return objectType.IsEnum;
            }

            private static bool IsNullableType(Type t)
            {
                if (t.IsGenericType) return t.GetGenericTypeDefinition() == typeof(Nullable<>);

                return false;
            }
        }
    }
}