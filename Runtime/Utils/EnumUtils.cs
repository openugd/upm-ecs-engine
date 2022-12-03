using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenUGD.ECS.Engine.Utils
{
    public static class EnumUtils
    {
        public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
        }

        public static Attribute[] GetAttributes(this Enum value)
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name).GetCustomAttributes(false).Cast<Attribute>().ToArray();
        }

        public static TAttribute[] GetAttributes<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().ToArray();
        }

        public static void GetAttributes(this Enum value, List<Attribute> attributes)
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            attributes.AddRange(enumType.GetField(name).GetCustomAttributes(false).Cast<Attribute>());
        }
    }
}