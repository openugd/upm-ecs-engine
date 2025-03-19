using System;
using System.Collections.Generic;
using System.Reflection;
using OpenUGD.ECS.Engine.Outputs;

namespace OpenUGD.ECS.Engine
{
    public interface IEngineOutputs
    {
        bool IsEmpty { get; }
        int Count { get; }
        Output Dequeue();
        void ReleaseToPool(Output evt);

        void Enqueue(Output output);
        T Enqueue<T>(int tick) where T : Output, new();
    }

    public class EngineOutputs : IEngineOutputs
    {
        private readonly HashSet<Output> _debugInPool;
        private readonly Dictionary<Type, Stack<Output>> _pool = new Dictionary<Type, Stack<Output>>();
        private readonly Queue<Output> _queue = new Queue<Output>();

        public EngineOutputs(bool isDebug)
        {
            if (isDebug)
            {
                _debugInPool = new HashSet<Output>();
            }
        }

        public bool IsEmpty => _queue.Count == 0;
        public int Count => _queue.Count;

        public Output Dequeue()
        {
            if (_queue.Count == 0)
            {
                throw new InvalidOperationException($"{nameof(Dequeue)}: queue is empty");
            }

            return _queue.Dequeue();
        }

        public void ReleaseToPool(Output evt)
        {
            Stack<Output>? stack;
            if (!_pool.TryGetValue(evt.GetType(), out stack))
            {
                _pool[evt.GetType()] = stack = new Stack<Output>();
            }

            if (_debugInPool != null)
            {
                if (!_debugInPool.Add(evt))
                {
                    throw new ArgumentException("this event is already in the pool");
                }
            }
            
            stack.Push(evt);
        }

        public void ReturnToPoolAll()
        {
            while (_queue.Count != 0)
            {
                ReleaseToPool(_queue.Dequeue());
            }
        }

        public void Enqueue(Output output)
        {
            if (_debugInPool != null)
            {
                _debugInPool.Remove(output);
            }

            _queue.Enqueue(output);
        }

        public T Enqueue<T>(int tick) where T : Output, new()
        {
            Stack<Output>? stack;
            if (_pool.TryGetValue(typeof(T), out stack) && stack.Count != 0)
            {
                var result = (T)stack.Pop();
                result.Tick = tick;
                result.Reset();
                Enqueue(result);
                return result;
            }

            var instance = new T();
            instance.Tick = tick;
            Enqueue(instance);
            return instance;
        }
    }
}