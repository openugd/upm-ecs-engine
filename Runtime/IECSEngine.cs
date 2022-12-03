using System;

namespace OpenUGD.ECS.Engine
{
    public interface IECSEngine : IDisposable
    {
        /// <summary>
        ///     текущий тик
        /// </summary>
        int Tick { get; }

        /// <summary>
        ///     входные действия
        /// </summary>
        IEngineInputs Inputs { get; }

        /// <summary>
        ///     выходные действия (для клиента)
        /// </summary>
        IEngineOutputs Outputs { get; }

        /// <summary>
        ///     состояние
        /// </summary>
        IEngineState State { get; }

        /// <summary>
        ///     создать снепшот
        /// </summary>
        /// <returns>снепшот</returns>
        EngineSnapshot Snapshot();

        /// <summary>
        ///     перемотать на шаг
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        int FastForward(int tick);

        /// <summary>
        ///     вернуться на 1н шаг назад
        /// </summary>
        /// <returns></returns>
        bool Backward();

        /// <summary>
        ///     рестарт
        /// </summary>
        /// <param name="randomSeed"> -1 = use previous value</param>
        void StartOver(int randomSeed = -1);

        /// <summary>
        ///     перемотать на тик
        /// </summary>
        /// <param name="tick"></param>
        int GoTo(int tick);
    }
}