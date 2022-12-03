namespace OpenUGD.ECS.Engine.Systems
{
    public abstract class SystemSequence<TWorld> : ISystem<TWorld>, ISystemInitialize<TWorld>
        where TWorld : OpenUGD.ECS.World
    {
        protected SystemSequence(ISystem<TWorld>[] systems)
        {
            Systems = systems;
        }

        protected ISystem<TWorld>[] Systems { get; }

        public void OnInitialize(TWorld world, IEngineContext outputs)
        {
            foreach (var system in Systems)
            {
                var initializer = system as ISystemInitialize<TWorld>;
                if (initializer != null)
                {
                    initializer.OnInitialize(world, outputs);
                }
            }
        }

        public void OnTick(TWorld world, int tick, IEngineContext outputs)
        {
            foreach (var system in Systems) system.OnTick(world, tick, outputs);
        }
    }
}