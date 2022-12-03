namespace OpenUGD.ECS.Engine.Systems
{
    public interface ISystem<in TWorld> where TWorld : OpenUGD.ECS.World
    {
        void OnTick(TWorld world, int tick, IEngineContext context);
    }

    public interface ISystemInitialize<in TWorld> where TWorld : OpenUGD.ECS.World
    {
        void OnInitialize(TWorld world, IEngineContext context);
    }
}