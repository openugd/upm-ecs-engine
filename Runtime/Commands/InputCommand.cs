﻿using OpenUGD.ECS.Engine.Inputs;

namespace OpenUGD.ECS.Engine.Commands
{
    public abstract class InputCommand<TWorld>
        where TWorld : World
    {
        public abstract void ExecuteCommand(Input input, TWorld world);
    }

    public abstract class InputCommand<TInput, TWorld> : InputCommand<TWorld>
        where TInput : Input
        where TWorld : World
    {
        public abstract void ExecuteCommand(TInput input, TWorld world);

        public override void ExecuteCommand(Input input, TWorld world) => ExecuteCommand((TInput)input, world);
    }
}
