using System;
using System.Linq;

namespace OpenUGD.ECS.Engine
{
    public class WorldRandom
    {
        private readonly Random _random;

        public WorldRandom(int randomSeed)
        {
            _random = new Random(randomSeed);
        }

        public int Next()
        {
            return _random.Next();
        }

        public int Next(int max)
        {
            return _random.Next(max);
        }

        public float Next(float max)
        {
            return _random.Next((int)(max * 1000f)) / 1000f;
        }

        public int Next(int min, int max)
        {
            return _random.Next(min, max);
        }

        public float Next(float min, float max)
        {
            return _random.Next((int)(min * 1000f), (int)(max * 1000f)) / 1000f;
        }

        public float NextFloat()
        {
            return (float)_random.NextDouble();
        }

        public bool NextBool(float percent)
        {
            return NextFloat() <= percent;
        }
    }
}