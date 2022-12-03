using System;

namespace OpenUGD.ECS.Engine.Utils
{
    public class DescriptionAttribute : Attribute
    {
        public DescriptionAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; }
    }
}