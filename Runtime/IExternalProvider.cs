using OpenUGD.ECS.Engine.Providers;

namespace OpenUGD.ECS.Engine
{
    public interface IExternalProvider<TWorld, TConfig> : IExternalResolver
        where TWorld : OpenUGD.ECS.World
        where TConfig : EngineConfiguration
    {
        TWorld CreateWorld(Engine<TWorld, TConfig> engine, int seed);
        IEngineCommandsProvider<TWorld> GetCommandsProvider();
        IEngineSystemsProvider<TWorld> GetEngineSystemsProvider();
        void Start(Engine<TWorld, TConfig> engine, TWorld world);
    }
}