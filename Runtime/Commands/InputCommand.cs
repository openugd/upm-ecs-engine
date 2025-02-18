using OpenUGD.ECS.Engine.Inputs;

namespace OpenUGD.ECS.Engine.Commands
{
    public abstract class InputCommand<TWorld> where TWorld : World
    {
        public abstract void ExecuteCommand(Input action, TWorld state);
    }
}
