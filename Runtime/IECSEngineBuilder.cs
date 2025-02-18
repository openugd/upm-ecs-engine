using System;
using OpenUGD.ECS.Engine.Commands;
using OpenUGD.ECS.Engine.Systems;

namespace OpenUGD.ECS.Engine
{
    public interface IEngineCommandsProvider<TWorld> where TWorld : World
    {
        InputCommand<TWorld>? GetCommand(Type inputType);
    }

    public interface IEngineSystemsProvider<TWorld> where TWorld : World
    {
        ISystem<TWorld>[] CreateSystems();
    }

    public interface IECSEngineBuilder<TWorld, TConfig> : IEngineSystemsProvider<TWorld>,
        IEngineCommandsProvider<TWorld>
        where TWorld : World
        where TConfig : EngineConfiguration
    {
        TWorld CreateWorld(Engine<TWorld, TConfig> engine, int seed);
        void OnStart(Engine<TWorld, TConfig> engine, TWorld world);
    }
}
