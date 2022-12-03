namespace OpenUGD.ECS.Engine.Outputs
{
    public abstract class Output
    {
        public int Tick;

        public abstract void Reset();
    }
}