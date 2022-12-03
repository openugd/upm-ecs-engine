using System;
using System.Collections.Generic;

namespace OpenUGD.ECS.Engine.Utils
{
    public class PoolFactory<T>
    {
        private readonly Func<T> _factory;
        private readonly Action<T>? _onRelease;
        private readonly Action<T>? _onRetain;
        private readonly Stack<T> _stack;

        public PoolFactory(Func<T> factory, Action<T>? onRelease = null, Action<T>? onRetain = null)
        {
            _factory = factory;
            _onRelease = onRelease;
            _onRetain = onRetain;
            _stack = new Stack<T>();
        }

        public virtual T Pop()
        {
            var result = _stack.Count != 0 ? _stack.Pop() : _factory();

            _onRetain?.Invoke(result);

            return result;
        }

        public virtual void Return(T value)
        {
            if (_stack.Contains(value))
            {
#if DEBUG
                throw new ArgumentException();
#else
        return;
#endif
            }

            _onRelease?.Invoke(value);

            _stack.Push(value);
        }

        public void Prune(Action<T> action)
        {
            while (_stack.Count != 0)
            {
                action(_stack.Pop());
            }
        }

        public void Clear()
        {
            _stack.Clear();
        }
    }
}