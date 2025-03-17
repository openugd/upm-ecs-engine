using System;
using OpenUGD.ECS.Engine.Inputs;

namespace OpenUGD.ECS.Engine
{
    [Serializable]
    public class EngineConfiguration
    {
        /// <summary>
        ///     environment
        /// </summary>
        public EngineEnvironment Environment;

        /// <summary>
        ///     list of inputs
        /// </summary>
        public Input[]? Inputs;

        /// <summary>
        ///     maximum number of ticks
        /// </summary>
        public int MaxTicks;

        /// <summary>
        ///     random
        /// </summary>
        public int RandomSeed;

        /// <summary>
        ///     current tick
        /// </summary>
        public int Tick;
    }
}