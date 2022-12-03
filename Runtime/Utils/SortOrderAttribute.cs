using System;

namespace OpenUGD.ECS.Engine.Utils
{
    public class SortOrderAttribute : Attribute
    {
        public readonly int Order;

        public SortOrderAttribute(int order)
        {
            Order = order;
        }
    }
}