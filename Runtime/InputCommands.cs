// © 2025 OpenUGD

using System;
using System.Collections.Generic;
using OpenUGD.ECS.Engine.Commands;
using OpenUGD.ECS.Engine.Inputs;

namespace OpenUGD.ECS.Engine
{
    public interface IAddCommand<TWorld>
        where TWorld : World
    {
        IAddCommand<TWorld> AddCommand<TInput, TCommand>(Func<TCommand> factory)
            where TInput : Input
            where TCommand : InputCommand<TWorld>;
    }

    public class InputCommands<TWorld> : IAddCommand<TWorld>
        where TWorld : World
    {
        private readonly Dictionary<Type, Func<InputCommand<TWorld>>> _commands = new();

        public IAddCommand<TWorld> AddCommand<TInput, TCommand>(
            Func<TCommand> factory
        )
            where TInput : Input
            where TCommand : InputCommand<TWorld>
        {
            _commands[typeof(TInput)] = factory;
            return this;
        }

        public IInputCommands<TWorld> Build() => new Builder(_commands);

        class Builder : IInputCommands<TWorld>
        {
            private readonly Dictionary<Type, Func<InputCommand<TWorld>>> _commandsFactory;
            private readonly Dictionary<Type, InputCommand<TWorld>> _commands;

            public Builder(Dictionary<Type, Func<InputCommand<TWorld>>> commandsFactory)
            {
                _commandsFactory = commandsFactory;
                _commands = new Dictionary<Type, InputCommand<TWorld>>();
            }

            public InputCommand<TWorld> GetCommand(Type inputType)
            {
                if (_commands.TryGetValue(inputType, out var command))
                {
                    return command;
                }

                if (_commandsFactory.TryGetValue(inputType, out var factory))
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
