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
    }

    public class EngineOutputs : IEngineOutputs
    {
        private readonly HashSet<Output> _debugInPool = new HashSet<Output>();
        private readonly Dictionary<Type, Stack<Output>> _pool = new Dictionary<Type, Stack<Output>>();
        private readonly Queue<Output> _queue = new Queue<Output>();

        public bool IsEmpty => _queue.Count == 0;
        public int Count => _queue.Count;

        public Output Dequeue()
        {
            if (_queue.Count == 0)
            {
                throw new InvalidOperationException($"{MethodBase.GetCurrentMethod()!.Name}: очередь пуста");
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

            if (!_debugInPool.Add(evt))
            {
                throw new ArgumentException("данный ивент уже находится в пуле");
            }

            stack.Push(evt);
        }

        public void Enqueue(Output output)
        {
            _debugInPool.Remove(output);
            _queue.Enqueue(output);
        }

        public void ReturnToPoolAll()
        {
            while (_queue.Count != 0)
            {
                ReleaseToPool(_queue.Dequeue());
            }
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