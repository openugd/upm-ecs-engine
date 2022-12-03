using System;

namespace OpenUGD.ECS.Engine.Utils
{
    public class HeaderAttribute : Attribute
    {
        public HeaderAttribute(string label)
        {
            Label = label;
        }

        public string Label { get; set; }
    }
}