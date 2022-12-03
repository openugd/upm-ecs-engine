namespace OpenUGD.ECS.Engine
{
    public interface IEngineState
    {
        /// <summary>
        ///     текущий тик
        /// </summary>
        int Tick { get; }

        /// <summary>
        ///     финишировал ли бой
        /// </summary>
        bool IsExited { get; }

        /// <summary>
        ///     финиш статус
        /// </summary>
        ExitReason ExitState { get; }

        /// <summary>
        ///     окружение
        /// </summary>
        EngineEnvironment Environment { get; }

        /// <summary>
        ///     продолжить, после финиша
        /// </summary>
        void Continue();

        /// <summary>
        ///     завершить работу движка
        /// </summary>
        public void Exit(ExitReason reason);
    }
}