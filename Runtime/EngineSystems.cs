using OpenUGD.ECS.Engine.Providers;
using OpenUGD.ECS.Engine.Systems;

namespace OpenUGD.ECS.Engine
{
    public class EngineSystems<TWorld> where TWorld : OpenUGD.ECS.World
    {
        private readonly IEngineSystemsProvider<TWorld> _provider;
        private ISystem<TWorld>[]? _systems;

        public EngineSystems(IEngineSystemsProvider<TWorld> provider)
        {
            _provider = provider;
        }

        public void Initialize(TWorld state, IEngineContext outputs)
        {
            _systems = _provider.CreateSystems();

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