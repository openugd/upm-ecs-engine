using System;

namespace OpenUGD.ECS.Engine
{
    [Serializable]
    [Flags]
    public enum EngineEnvironment
    {
        Debug = 1 << 0,
        Release = 1 << 1,
        Client = 1 << 2,
        Server = 1 << 3,
        Snapshot = 1 << 4
    }

    public static class EngineEnvironmentExtension
    {
        public static bool IsDebug(this EngineEnvironment environment)
        {
            return (environment & EngineEnvironment.Debug) == EngineEnvironment.Debug;
        }

        public static bool IsRelease(this EngineEnvironment environment)
        {
            return (environment & EngineEnvironment.Release) == EngineEnvironment.Release;
        }

        public static bool IsGenerateOutputs(this EngineEnvironment environment)
        {
            return (environment & EngineEnvironment.Client) == EngineEnvironment.Client;
        }

        public static bool IsSnapshot(this EngineEnvironment environment)
        {
            return (environment & EngineEnvironment.Snapshot) == EngineEnvironment.Snapshot;
        }
    }
}