using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenUGD.ECS.Engine.Inputs;

namespace OpenUGD.ECS.Engine.Utils
{
    public class DeepClone
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new InputConverter() },
            TypeNameHandling = TypeNameHandling.Auto
        };

        public static T Clone<T>(T obj)
        {
            var serialized = JsonConvert.SerializeObject(obj, Settings);
            return JsonConvert.DeserializeObject<T>(serialized, Settings);
        }

        class InputConverter : JsonConverter<Input[]>
        {
            public override void WriteJson(JsonWriter writer, Input[] value, JsonSerializer serializer)
            {
                writer.WriteStartArray();
                if (value != null)
                {
                    foreach (var input in value)
                    {
                        writer.WriteValue(input.GetType().AssemblyQualifiedName);
                        serializer.Serialize(writer, input, input.GetType());
                    }
                }

                writer.WriteEndArray();
            }

            public override Input[] ReadJson(
                JsonReader reader,
                Type objectType,
                Input[] existingValue,
                bool hasExistingValue,
                JsonSerializer serializer
            )
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    return null;
                }

                if (reader.TokenType != JsonToken.StartArray)
                {
                    throw new JsonSerializationException($"Expected start array, got {reader.TokenType}");
                }

                var inputs = new List<Input>();

                while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                {
                    var typeStr = (string)reader.Value;
                    var type = Type.GetType(typeStr!);
                    reader.Read();
                    var input = serializer.Deserialize(reader, type);
                    if (input is Input inputConverted)
                    {
                        inputs.Add(inputConverted);
                    }
                    else
                    {
                        throw new JsonSerializationException($"Expected Input, got {input.GetType()}");
                    }
                }

                return inputs.ToArray();
            }
        }
    }
}