using OpenUGD.ECS.Utilities;

namespace OpenUGD.ECS.Engine
{
    public class WorldRandom
    {
        /// <summary>
        /// can't be readonly because it's a mutable struct
        /// </summary>
        private UnsafeRandom _random;

        public WorldRandom(int seed) => _random = new UnsafeRandom(seed);
        public byte[] Serialize() => _random.Serialize();
        public void Deserialize(byte[] data) => _random.Deserialize(data);
        public void InitSeed(int seed) => _random.InitWith(seed);
        public int Next() => _random.Next();
        public int Next(int max) => _random.Next(max);
        public float Next(float max) => _random.Next((int)(max * 1000f)) / 1000f;
        public int Next(int min, int max) => _random.Next(min, max);
        public float Next(float min, float max) => _random.NextFloat() * (max - min) + min;
        public float NextFloat() => (float)_random.NextDouble();
        public bool NextBool(float percent) => NextFloat() <= percent;
    }
}
