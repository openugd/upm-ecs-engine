using System;

namespace OpenUGD.ECS.Engine.Utils
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExportAttribute : Attribute
    {
        public bool CanImport { get; set; } = true;
    }
}