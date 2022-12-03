using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using OpenUGD.ECS.Engine.Inputs;
using OpenUGD.ECS.Engine.Utils;

namespace OpenUGD.ECS.Engine
{
    public class EngineSnapshot
    {
        public const string FileExtension = ".snapshot";
        public const string FileExtensionWithoutDot = "snapshot";

        private readonly byte[] _bytes;

        public EngineSnapshot(SerializedSnapshot snapshot)
        {
            _bytes = Serialize(snapshot);
            Snapshot = snapshot;
        }

        public EngineSnapshot(byte[] bytes)
        {
            _bytes = bytes;
            Snapshot = Deserialize(bytes);
        }

        public EngineSnapshot(EngineConfiguration configuration, Input[] inputs, int randomSeed)
        {
            Snapshot = new SerializedSnapshot
            {
                Configuration = DeepClone.Clone(configuration),
                RandomSeed = randomSeed,
                Inputs = DeepClone.Clone(inputs.Where(i => i.GetType().IsDefined(typeof(SerializableAttribute), true))
                    .ToArray())
            };
            _bytes = Serialize(Snapshot);
        }

        public SerializedSnapshot Snapshot { get; }

        public byte[] ToBytes()
        {
            return _bytes;
        }

        public EngineConfiguration Build()
        {
            var snapshot = Deserialize(_bytes);

            var config = snapshot.Configuration;
            config.Environment |= EngineEnvironment.Snapshot;
            config.RandomSeed = snapshot.RandomSeed;
            config.Inputs = snapshot.Inputs;

            return config;
        }

        private byte[] Serialize(SerializedSnapshot snapshot)
        {
            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter
                {
                    AssemblyFormat = FormatterAssemblyStyle.Simple,
                    TypeFormat = FormatterTypeStyle.TypesWhenNeeded,
                    Binder = new PreMergeToMergedDeserializationBinder()
                };
                formatter.Serialize(memoryStream, snapshot);
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        private SerializedSnapshot Deserialize(byte[] snapshot)
        {
            using (var memoryStream = new MemoryStream(snapshot))
            {
                var formatter = new BinaryFormatter
                {
                    AssemblyFormat = FormatterAssemblyStyle.Simple,
                    TypeFormat = FormatterTypeStyle.TypesWhenNeeded,
                    Binder = new PreMergeToMergedDeserializationBinder()
                };
                memoryStream.Position = 0;
                return (SerializedSnapshot)formatter.Deserialize(memoryStream);
            }
        }

        [Serializable]
        public class SerializedSnapshot
        {
            public EngineConfiguration Configuration;
            public int EngineVersion = ECSEngine.Version;
            public Input[] Inputs;
            public int RandomSeed;
        }

        private sealed class PreMergeToMergedDeserializationBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                Type typeToDeserialize = null;
                var engineAssembly = typeof(EngineSnapshot).Assembly.FullName;
                typeToDeserialize = Type.GetType($"{typeName}, {engineAssembly}");
                return typeToDeserialize;
            }
        }
    }
}