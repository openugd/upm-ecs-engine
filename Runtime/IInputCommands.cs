using System;
using OpenUGD.ECS.Engine.Commands;

namespace OpenUGD.ECS.Engine
{
    public interface IInputCommands<TWorld>
        where TWorld : World
    {
        InputCommand<TWorld>? GetCommand(Type inputType);
    }
}
