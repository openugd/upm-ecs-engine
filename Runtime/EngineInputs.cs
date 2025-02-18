using System;
using System.Reflection;
using OpenUGD.ECS.Engine.Inputs;
using OpenUGD.ECS.Engine.Utils;

namespace OpenUGD.ECS.Engine
{
    public interface IEngineInputs
    {
        int Count { get; }
        void AddInput(Input input);
    }

    public class EngineInputs<TWorld> : IEngineInputs where TWorld : OpenUGD.ECS.World
    {
        private readonly PriorityQueueComparable<Input> _actionQueue;
        private readonly Engine<TWorld> _engine;
        private readonly IInputCommands<TWorld> _inputCommands;
        private int _idIncrement;

        public EngineInputs(Engine<TWorld> engine, IInputCommands<TWorld> inputCommands)
        {
            _engine = engine;
            _inputCommands = inputCommands;
            _actionQueue = new PriorityQueueComparable<Input>(4096);
        }

        public int Count => _actionQueue.Count;

        public void AddInput(Input input)
        {
            if (input.Tick <= _engine.Tick)
                throw new ArgumentException(string.Format(
                    "{2} новое действие не можеть быть меньше или равно чем уже пройденый тик игры. Тик действия:{0}. Тик игры:{1}, тип:{3}",
                    input.Tick, _engine.Tick, MethodBase.GetCurrentMethod()!.Name, input.GetType().FullName));

            Input.Internal.SetId(input, ++_idIncrement);
            Enqueue(input);
        }

        public Input Peek()
        {
            return _actionQueue.Peek();
        }

        public bool Contains<T>(int tick) where T : Input
        {
            return Contains(typeof(T), tick);
        }

        public bool Contains(Type type, int tick)
        {
            foreach (var input in _actionQueue)
                if (input.Tick == tick && input.GetType() == type)
                    return true;

            return false;
        }

        public Input Dequeue()
        {
            return _actionQueue.Dequeue();
        }

        public void Enqueue(Input input)
        {
            _actionQueue.Enqueue(input);
        }

        public void Execute(Input input)
        {
            var command = _inputCommands.GetCommand(input.GetType());

            if (_engine.Environment.IsDebug())
            {
                if (command == null)
                {
                    throw new InvalidOperationException(string.Format(
                        MethodBase.GetCurrentMethod()!.Name + ": комманда не найдена: {0}", input.GetType()));
                }
            }

            if (command != null)
            {
                command.ExecuteCommand(input, _engine.World!);
            }
        }

        public void Clear()
        {
            _actionQueue.Clear();
            _idIncrement = 0;
        }
    }
}
