using System;
using System.Collections;
using System.Collections.Generic;
using OpenUGD.ECS.Engine.Inputs;
using OpenUGD.ECS.Engine.Utils;

namespace OpenUGD.ECS.Engine
{
    public interface IEngineExecutedInputs : IEnumerable<Input>
    {
        int Count { get; }
        new Queue<Input>.Enumerator GetEnumerator();
    }

    public interface IEngineInputs : IEnumerable<Input>
    {
        int Count { get; }
        void AddInput(Input input);
        new List<Input>.Enumerator GetEnumerator();
        IEngineExecutedInputs Executed { get; }
    }

    public class EngineInputs<TWorld> : IEngineInputs where TWorld : World
    {
        class EngineExecutedInputs : IEngineExecutedInputs
        {
            private readonly Queue<Input> _executedInputs;

            public EngineExecutedInputs(Queue<Input> executedInputs) => _executedInputs = executedInputs;

            public int Count => _executedInputs.Count;

            public Queue<Input>.Enumerator GetEnumerator() => _executedInputs.GetEnumerator();

            IEnumerator<Input> IEnumerable<Input>.GetEnumerator() => _executedInputs.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _executedInputs.GetEnumerator();
        }

        private readonly Queue<Input> _executedInputs;
        private readonly PriorityQueueComparable<Input> _actionQueue;
        private readonly Engine<TWorld> _engine;
        private readonly IInputCommands<TWorld> _inputCommands;
        private readonly Serializer _serializer;
        private readonly EngineExecutedInputs _engineExecuted;
        private int _idIncrement;

        public EngineInputs(Engine<TWorld> engine, IInputCommands<TWorld> inputCommands, Serializer serializer)
        {
            _engine = engine;
            _inputCommands = inputCommands;
            _serializer = serializer;
            _executedInputs = new Queue<Input>(4096);
            _actionQueue = new PriorityQueueComparable<Input>(4096);
            _engineExecuted = new EngineExecutedInputs(_executedInputs);
        }

        public int Count => _actionQueue.Count;

        public IEngineExecutedInputs Executed => _engineExecuted;

        public void AddInput(Input input)
        {
            if (input.Tick <= _engine.Tick)
            {
                throw new ArgumentException(
                    $"{nameof(AddInput)} a new action cannot be less than or equal to an already completed game tick. Action tick:{input.Tick}. Game Tick:{_engine.Tick}, Type:{input.GetType().FullName}"
                );
            }

            Input.Internal.SetId(input, ++_idIncrement);
            Enqueue(input);
        }

        public void ClearExecuted() => _executedInputs.Clear();

        public Input Peek() => _actionQueue.Peek();

        public Input Dequeue() => _actionQueue.Dequeue();

        public void Enqueue(Input input) => _actionQueue.Enqueue(input);

        public int CopyTo(List<Input> toList)
        {
            toList.AddRange(_actionQueue.Source);
            return _actionQueue.Count;
        }

        public void CloneTo(List<Input> toList)
        {
            foreach (var input in _actionQueue)
            {
                toList.Add(_serializer.Clone(input));
            }
        }

        public void Execute(Input input)
        {
            _executedInputs.Enqueue(input);

            var command = _inputCommands.GetCommand(input.GetType());

            if (_engine.Environment.IsDebug())
            {
                if (command == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(Execute)}, for an input {input.GetType()} cannot find command");
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

        public List<Input>.Enumerator GetEnumerator() => _actionQueue.GetEnumerator();

        List<Input>.Enumerator IEngineInputs.GetEnumerator() => GetEnumerator();

        IEnumerator<Input> IEnumerable<Input>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


    public static class EngineInputsExtensions
    {
        public static bool Contains(this IEngineInputs inputs, Type type, int tick)
        {
            foreach (var input in inputs)
            {
                if (input.Tick == tick && input.GetType() == type)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Contains(this IEngineInputs inputs, Type type)
        {
            foreach (var input in inputs)
            {
                if (input.GetType() == type)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Contains<T>(this IEngineInputs inputs, int tick) where T : Input
        {
            return inputs.Contains(typeof(T), tick);
        }

        public static bool Contains<T>(this IEngineInputs inputs) where T : Input
        {
            return inputs.Contains(typeof(T));
        }

        public static int CopyTo(this IEngineInputs inputs, List<Input> toList)
        {
            toList.AddRange(inputs);
            return inputs.Count;
        }
    }
}