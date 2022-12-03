using System;
using OpenUGD.ECS.Engine.Inputs;
using OpenUGD.ECS.Engine.Utils;

namespace OpenUGD.ECS.Engine
{
    [Serializable]
    public class EngineConfiguration
    {
        /// <summary>
        ///     окружение
        /// </summary>
        public EngineEnvironment Environment;

        /// <summary>
        ///     список вводных действий
        /// </summary>
        public Input[]? Inputs;

        /// <summary>
        ///     максимальное к-во тиков
        /// </summary>
        public int MaxTicks;

        /// <summary>
        ///     рандом
        /// </summary>
        public int RandomSeed;

        /// <summary>
        ///     текущий тик
        /// </summary>
        public int Tick;

        /// <summary>
        ///     клонирование
        /// </summary>
        /// <returns></returns>
        public EngineConfiguration Clone()
        {
            return DeepClone.Clone(this);
        }
    }
}