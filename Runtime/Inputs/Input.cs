using System;
using System.Collections.Generic;

namespace OpenUGD.ECS.Engine.Inputs
{
    [Serializable]
    public abstract class Input : IComparable
    {
        public static readonly InputComparer Comparer = new InputComparer();

        private int _internalId;

        public int Tick;

        public int CompareTo(object? obj)
        {
            var other = obj as Input;
            if (other == null) throw new ArgumentException($"object is not derived from: {typeof(Input)}");
            return Comparer.Compare(this, other);
        }

        internal static class Internal
        {
            internal static void SetId(Input input, int id)
            {
                input._internalId = id;
            }

            internal static int GetId(Input input)
            {
                return input._internalId;
            }
        }

        public class InputComparer : Comparer<Input>
        {
            public override int Compare(Input? x, Input? y)
            {
                if (x == null) throw new ArgumentNullException(nameof(x));
                if (y == null) throw new ArgumentNullException(nameof(y));

                if (x.Tick > y.Tick) return 1;
                if (x.Tick < y.Tick) return -1;

                if (x._internalId > y._internalId) return 1;
                if (x._internalId < y._internalId) return -1;

                return 0;
            }
        }
    }
}