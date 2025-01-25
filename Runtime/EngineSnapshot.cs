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
            var json = JsonConvert.SerializeObject(
                snapshot
            );
            var bytes = Encoding.UTF8.GetBytes(json);
            return bytes;
        }

        private SerializedSnapshot Deserialize(byte[] snapshot)
        {
            var json = Encoding.UTF8.GetString(snapshot);
            return JsonConvert.DeserializeObject<SerializedSnapshot>(json)!;
        }

        [Serializable]
        public class SerializedSnapshot
        {
            public EngineConfiguration Configuration;
            public int EngineVersion = ECSEngine.Version;
            public Input[] Inputs;
            public int RandomSeed;
        }
    }
}