using System;
using System.Collections.Generic;
using OpenUGD.ECS.Engine.Commands;

namespace OpenUGD.ECS.Engine.Providers
{
    public class EngineCommandsProvider<TWorld> : IEngineCommandsProvider<TWorld> where TWorld : OpenUGD.ECS.World
    {
        private readonly Dictionary<Type, InputCommand<TWorld>> _mapCommands =
            new Dictionary<Type, InputCommand<TWorld>>();

        public InputCommand<TWorld>? GetCommand(Type inputType)
        {
            InputCommand<TWorld>? result;
            _mapCommands.TryGetValue(inputType, out result);
            return result;
        }

        public void Map<TCommand>()
            where TCommand : InputCommand<TWorld>, new()
        {
            var command = new TCommand();
            _mapCommands[command.Type] = command;
        }
    }
}