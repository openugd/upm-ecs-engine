using OpenUGD.ECS.Engine.Systems;

namespace OpenUGD.ECS.Engine.Providers
{
    public interface IEngineSystemsProvider<TWorld> where TWorld : OpenUGD.ECS.World
    {
        ISystem<TWorld>[] CreateSystems();
    }
}