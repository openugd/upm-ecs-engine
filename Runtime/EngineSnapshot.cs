using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using OpenUGD.ECS.Engine.Inputs;
using OpenUGD.ECS.Engine.Utils;

namespace OpenUGD.ECS.Engine
{
    public class EngineSnapshot
    {
        private readonly Serializer _serializer;
        private readonly byte[] _bytes;

        public EngineSnapshot(SerializedSnapshot snapshot, Serializer serializer)
        {
            _serializer = serializer;
            _bytes = serializer.Serialize(snapshot);
            Snapshot = snapshot;
        }

        public EngineSnapshot(byte[] bytes, Serializer serializer)
        {
            _bytes = bytes;
            _serializer = serializer;
            Snapshot = serializer.Deserialize<SerializedSnapshot>(bytes);
        }

        public EngineSnapshot(
            EngineConfiguration configuration,
            Input[] inputs,
            int randomSeed,
            int tick,
            Serializer serializer
        )
        {
            _serializer = serializer;
            Snapshot = new SerializedSnapshot
            {
                Configuration = serializer.Clone(configuration),
                Tick = tick,
                RandomSeed = randomSeed,
                Inputs = serializer
                    .Clone(inputs
                        .Where(i => i.GetType().IsDefined(typeof(SerializableAttribute), true))
                        .ToArray()
                    )
            };
            _bytes = serializer.Serialize(Snapshot);
        }

        public SerializedSnapshot Snapshot { get; }

        public byte[] ToBytes()
        {
            return _bytes;
        }

        public EngineConfiguration Build()
        {
            var snapshot = _serializer.Deserialize<SerializedSnapshot>(_bytes);

            var config = snapshot.Configuration;
            config.Environment |= EngineEnvironment.Snapshot;
            config.RandomSeed = snapshot.RandomSeed;
            config.Inputs = snapshot.Inputs;
            config.Tick = snapshot.Tick;

            return config;
        }

        [Serializable]
        public class SerializedSnapshot
        {
            public EngineConfiguration Configuration;
            public int EngineVersion = ECSEngine.Version;
            public Input[] Inputs;
            public int RandomSeed;
            public int Tick;
        }
    }
}