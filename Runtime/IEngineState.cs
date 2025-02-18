namespace OpenUGD.ECS.Engine
{
    public interface IEngineState
    {
        /// <summary>
        /// current tick
        /// </summary>
        int Tick { get; }

        /// <summary>
        /// whether or not the engine finished
        /// </summary>
        bool IsExited { get; }

        /// <summary>
        /// finish status
        /// </summary>
        ExitReason ExitState { get; }

        /// <summary>
        /// environment
        /// </summary>
        EngineEnvironment Environment { get; }

        /// <summary>
        /// to continue, after the finish of the engine
        /// </summary>
        void Continue();

        /// <summary>
        /// shut down the engine
        /// </summary>
        public void Exit(ExitReason reason);
    }
}
