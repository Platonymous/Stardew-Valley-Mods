using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace PyTK.Tiled
{
    internal static class EnumHelper
    {
        public static string GetEnumName(this Enum value)
        {
            DescriptionAttribute[] customAttributes = (DescriptionAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return customAttributes.Length != 0 ? customAttributes[0].Description : value.ToString();
        }

        public static T GetEnumByName<T>(this string name)
        {
            Type type1 = Nullable.GetUnderlyingType(typeof(T));
            if ((object)type1 == null)
                type1 = typeof(T);
            Type type2 = type1;
            if (!type2.IsEnum)
                throw new InvalidOperationException();
            foreach (FieldInfo field in type2.GetFields())
            {
                DescriptionAttribute customAttribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (customAttribute != null)
                {
                    if (customAttribute.Description == name)
                        return (T)field.GetValue(null);
                }
                else if (field.Name == name)
                    return (T)field.GetValue(null);
            }
            return default(T);
        }

        public static IEnumerable<T> EnumToList<T>()
        {
            Type enumType = typeof(T);
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");
            Array values = Enum.GetValues(enumType);
            List<T> objList = new List<T>(values.Length);
            foreach (int num in values)
                objList.Add((T)Enum.Parse(enumType, num.ToString()));
            return objList;
        }
    }
}
