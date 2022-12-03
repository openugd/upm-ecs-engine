using OpenUGD.ECS.Engine.Outputs;

namespace OpenUGD.ECS.Engine
{
    public interface IEngineContext
    {
        int Seed { get; }
        T Enqueue<T>() where T : Output, new();
    }
}