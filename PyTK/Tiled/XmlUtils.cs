using System;
using System.Xml.Linq;

namespace PyTK.Tiled
{
    internal static class XmlUtils
    {
        public static XElement CreateNullElement(string name)
        {
            return new XElement(name);
        }

        public static XObject If(bool condition, XObject elem)
        {
            if (!condition)
                return null;
            return elem;
        }

        public static XObject If(bool condition, XmlUtils.XObjectCreationDelegate objectCreation)
        {
            if (!condition)
                return null;
            return objectCreation();
        }

        public static bool HasOwnProperty(this XElement elem, string name)
        {
            if (!name.StartsWith("@"))
                return elem.Element(name) != null;
            return elem.Attribute(name.Substring(1)) != null;
        }

        public static T Value<T>(this XElement elem)
        {
            return (T)Convert.ChangeType(elem.Value, typeof(T));
        }

        public static T Value<T>(this XElement elem, string elementName)
        {
            if (elem == null)
                return default(T);
            Type type1 = typeof(T);
            string str1;
            if (!elementName.StartsWith("@"))
            {
                XElement xelement = elem.Element(elementName);
                str1 = xelement != null ? xelement.Value : null;
            }
            else
            {
                XAttribute xattribute = elem.Attribute(elementName.Substring(1));
                str1 = xattribute != null ? xattribute.Value : null;
            }
            string str2 = str1;
            if (str2 == null)
                return default(T);
            if (type1 == typeof(int))
                return (T)Convert.ChangeType(Utils.FromString(str2), type1);
            Type type2 = Nullable.GetUnderlyingType(type1);
            if ((object)type2 == null)
                type2 = type1;
            Type conversionType = type2;
            if (!conversionType.IsEnum)
                return (T)Convert.ChangeType(str2, conversionType);
            long result;
            if (long.TryParse(str2, out result))
                return (T)(object)result;
            return str2.GetEnumByName<T>();
        }

        public delegate XObject XObjectCreationDelegate();
    }
}
