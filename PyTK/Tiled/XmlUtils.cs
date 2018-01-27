using System;
using System.Xml.Linq;

namespace PyTK.Tiled
{
    internal static class XmlUtils
    {
        public static XElement CreateNullElement(string name)
        {
            return new XElement((XName)name);
        }

        public static XObject If(bool condition, XObject elem)
        {
            if (!condition)
                return (XObject)null;
            return elem;
        }

        public static XObject If(bool condition, XmlUtils.XObjectCreationDelegate objectCreation)
        {
            if (!condition)
                return (XObject)null;
            return objectCreation();
        }

        public static bool HasOwnProperty(this XElement elem, string name)
        {
            if (!name.StartsWith("@"))
                return elem.Element((XName)name) != null;
            return elem.Attribute((XName)name.Substring(1)) != null;
        }

        public static T Value<T>(this XElement elem)
        {
            return (T)Convert.ChangeType((object)elem.Value, typeof(T));
        }

        public static T Value<T>(this XElement elem, string elementName)
        {
            if (elem == null)
                return default(T);
            Type type1 = typeof(T);
            string str1;
            if (!elementName.StartsWith("@"))
            {
                XElement xelement = elem.Element((XName)elementName);
                str1 = xelement != null ? xelement.Value : (string)null;
            }
            else
            {
                XAttribute xattribute = elem.Attribute((XName)elementName.Substring(1));
                str1 = xattribute != null ? xattribute.Value : (string)null;
            }
            string str2 = str1;
            if (str2 == null)
                return default(T);
            if (type1 == typeof(int))
                return (T)Convert.ChangeType((object)Utils.FromString(str2), type1);
            Type type2 = Nullable.GetUnderlyingType(type1);
            if ((object)type2 == null)
                type2 = type1;
            Type conversionType = type2;
            if (!conversionType.IsEnum)
                return (T)Convert.ChangeType((object)str2, conversionType);
            long result;
            if (long.TryParse(str2, out result))
                return (T)(object)(ValueType)result;
            return str2.GetEnumByName<T>();
        }

        public delegate XObject XObjectCreationDelegate();
    }
}
