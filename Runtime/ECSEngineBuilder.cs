// © 2025 OpenUGD

using System;
using System.Collections.Generic;
using System.Linq;
using OpenUGD.ECS.Engine.Commands;
using OpenUGD.ECS.Engine.Inputs;
using OpenUGD.ECS.Engine.Systems;

namespace OpenUGD.ECS.Engine
{
    public class ECSEngineBuilder<TEngine, TWorld, TConfiguration>
        where TEngine : Engine<TWorld, TConfiguration>
        where TWorld : World
        where TConfiguration : EngineConfiguration
    {
        private readonly Dictionary<Type, Func<InputCommand<TWorld>>> _commands = new();
        private readonly List<Action<TEngine, TWorld>> _onStart = new();
        private readonly List<Func<ISystem<TWorld>>> _systems = new();

        public ECSEngineBuilder<TEngine, TWorld, TConfiguration> AddOnStart(Action<TEngine, TWorld> onStart)
        {
            _onStart.Add(onStart);
            return this;
        }

        public ECSEngineBuilder<TEngine, TWorld, TConfiguration> AddSystem(
            Func<ISystem<TWorld>> factory
        )
        {
            _systems.Add(factory);
            return this;
        }

        public ECSEngineBuilder<TEngine, TWorld, TConfiguration> AddCommand<TInput, TCommand>(
            Func<TCommand> factory
        )
            where TInput : Input
            where TCommand : InputCommand<TWorld>
        {
            _commands[typeof(TInput)] = factory;
            return this;
        }

        public TEngine Build(
            TConfiguration configuration,
            Func<TConfiguration, IECSEngineBuilder<TWorld, TConfiguration>, TWorld> worldFactory,
            Func<TConfiguration, IECSEngineBuilder<TWorld, TConfiguration>, TEngine> engineFactory
        )
        {
            return engineFactory(configuration, new Builder(this, worldFactory, configuration));
        }

        class Builder : IECSEngineBuilder<TWorld, TConfiguration>
        {
            private readonly Func<TConfiguration, IECSEngineBuilder<TWorld, TConfiguration>, TWorld> _worldFactory;
            private readonly TConfiguration _configuration;
            private Dictionary<Type, Func<InputCommand<TWorld>>> _builderCommands;
            private List<Func<ISystem<TWorld>>> _builderSystems;
            private List<Action<TEngine, TWorld>> _builderOnStart;
            private Dictionary<Type, InputCommand<TWorld>> _commands = new();

            public Builder(
                ECSEngineBuilder<TEngine, TWorld, TConfiguration> builder,
                Func<TConfiguration, IECSEngineBuilder<TWorld, TConfiguration>, TWorld> worldFactory,
                TConfiguration configuration
            )
            {
                _worldFactory = worldFactory;
                _configuration = configuration;
                _builderCommands = new Dictionary<Type, Func<InputCommand<TWorld>>>(builder._commands);
                _builderSystems = new List<Func<ISystem<TWorld>>>(builder._systems);
                _builderOnStart = new List<Action<TEngine, TWorld>>(builder._onStart);
            }

            ISystem<TWorld>[] IEngineSystemsProvider<TWorld>.CreateSystems()
            {
                return _builderSystems.Select(factory => factory()).ToArray();
            }

            void IECSEngineBuilder<TWorld, TConfiguration>.
                OnStart(
                    Engine<TWorld, TConfiguration> engine,
                    TWorld world
                )
            {
                foreach (var action in _builderOnStart)
                {
                    action((TEngine)engine, world);
                }
            }

            TWorld IECSEngineBuilder<TWorld, TConfiguration>.CreateWorld(
                Engine<TWorld, TConfiguration> engine,
                int seed
            )
            {
                return _worldFactory(_configuration, this);
            }

            public InputCommand<TWorld> GetCommand(Type inputType)
            {
                if (_commands.TryGetValue(inputType, out var command))
                {
                    return command;
                }

                if (_builderCommands.TryGetValue(inputType, out var factory))
                {
                    command = factory();
                    _commands[inputType] = command;
                    return command;
                }

                throw new InvalidOperationException("Command not found");
            }
        }
    }
}
