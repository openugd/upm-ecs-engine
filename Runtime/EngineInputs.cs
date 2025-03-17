using System;
using System.Linq;
using System.Reflection;
using OpenUGD.ECS.Engine.Inputs;
using OpenUGD.ECS.Engine.Utils;

namespace OpenUGD.ECS.Engine
{
    public interface IEngineInputs
    {
        int Count { get; }
        void AddInput(Input input);
        Input[] Copy();
    }

    public class EngineInputs<TWorld> : IEngineInputs where TWorld : OpenUGD.ECS.World
    {
        private readonly PriorityQueueComparable<Input> _actionQueue;
        private readonly Engine<TWorld> _engine;
        private readonly IInputCommands<TWorld> _inputCommands;
        private readonly Serializer _serializer;
        private int _idIncrement;

        public EngineInputs(Engine<TWorld> engine, IInputCommands<TWorld> inputCommands, Serializer serializer)
        {
            _engine = engine;
            _inputCommands = inputCommands;
            _serializer = serializer;
            _actionQueue = new PriorityQueueComparable<Input>(4096);
        }

        public int Count => _actionQueue.Count;

        public void AddInput(Input input)
        {
            if (input.Tick <= _engine.Tick)
                throw new ArgumentException(string.Format(
                    "{2} a new action cannot be less than or equal to an already completed game tick. Action tick:{0}. Game Tick:{1}, Type:{3}",
                    input.Tick, _engine.Tick, MethodBase.GetCurrentMethod()!.Name, input.GetType().FullName));

            Input.Internal.SetId(input, ++_idIncrement);
            Enqueue(input);
        }

        public Input[] Copy()
        {
            return _serializer.Clone(_actionQueue.ToArray());
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
                        MethodBase.GetCurrentMethod()!.Name + ": command not found: {0}", input.GetType()));
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
