using System;

namespace OpenUGD.ECS.Engine.Utils
{
    [AttributeUsage(AttributeTargets.Field)]
    public class GroupAttribute : Attribute
    {
        public GroupAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}