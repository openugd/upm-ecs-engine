namespace OpenUGD.ECS.Engine
{
    public class EngineFactory
    {
        public IECSEngine Create<TWorld, TConfiguration>(TConfiguration config,
            IExternalProvider<TWorld, TConfiguration> externalProvider)
            where TWorld : OpenUGD.ECS.World
            where TConfiguration : EngineConfiguration
        {
            var engine = new Engine<TWorld, TConfiguration>(config, externalProvider);
            return engine;
        }
    }
}