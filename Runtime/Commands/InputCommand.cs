using System;
using OpenUGD.ECS.Engine.Inputs;

namespace OpenUGD.ECS.Engine.Commands
{
    public abstract class InputCommand<TWorld> where TWorld : OpenUGD.ECS.World
    {
        protected InputCommand(Type inputType)
        {
            Type = inputType;
        }

        public Type Type { get; }

        public abstract void ExecuteCommand(Input action, TWorld state);
    }

    public abstract class InputCommand<T, TWorld> : InputCommand<TWorld> where T : Input where TWorld : OpenUGD.ECS.World
    {
        protected InputCommand() : base(typeof(T))
        {
        }

        public override void ExecuteCommand(Input action, TWorld state)
        {
            Execute((T)action, state);
        }

        protected abstract void Execute(T action, TWorld state);
    }
}