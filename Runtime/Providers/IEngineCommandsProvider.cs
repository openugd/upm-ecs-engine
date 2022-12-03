using System;
using OpenUGD.ECS.Engine.Commands;

namespace OpenUGD.ECS.Engine.Providers
{
    public interface IEngineCommandsProvider<TWorld> where TWorld : OpenUGD.ECS.World
    {
        InputCommand<TWorld>? GetCommand(Type inputType);
    }
}