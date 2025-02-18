using System;
using OpenUGD.ECS.Engine.Systems;

namespace OpenUGD.ECS.Engine
{
    public class EngineSystems<TWorld> where TWorld : World
    {
        private readonly Func<ISystem<TWorld>[]> _factory;
        private ISystem<TWorld>[]? _systems;

        public EngineSystems(Func<ISystem<TWorld>[]> factory) => _factory = factory;

        public void Initialize(TWorld state, IEngineContext outputs)
        {
            _systems = _factory();

            foreach (var system in _systems)
            {
                var initialize = system as ISystemInitialize<TWorld>;
                if (initialize != null) initialize.OnInitialize(state, outputs);
            }
        }

        public void Tick(TWorld world, int currentTick, IEngineContext outputs)
        {
            foreach (var system in _systems)
            {
                system.OnTick(world, currentTick, outputs);
            }
        }
    }
}
