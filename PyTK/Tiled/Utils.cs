using System;
using System.Globalization;

namespace PyTK.Tiled
{
    internal static class Utils
    {
        public static int FromString(string value)
        {
            if (value == null)
                throw new FormatException();
            return value.StartsWith("0x") ? int.Parse(value.Substring(2), NumberStyles.HexNumber) : int.Parse(value);
        }
    }
}
