using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PyTK.Tiled
{
    internal class PathHelper
    {
        public static string GetRelativePath(string basePath, string absolutePath)
        {
            basePath = basePath.Trim();
            char directorySeparatorChar;
            int num;
            if (basePath.Length > 0)
            {
                string str1 = basePath;
                directorySeparatorChar = Path.DirectorySeparatorChar;
                string str2 = directorySeparatorChar.ToString() ?? "";
                num = !str1.EndsWith(str2) ? 1 : 0;
            }
            else
                num = 0;
            if (num != 0)
            {
                string str1 = basePath;
                directorySeparatorChar = Path.DirectorySeparatorChar;
                string str2 = directorySeparatorChar.ToString();
                basePath = str1 + str2;
            }
            absolutePath = absolutePath.Trim();
            if (!Path.IsPathRooted(basePath) || !Path.IsPathRooted(absolutePath))
                return absolutePath;
            if (absolutePath.StartsWith(basePath))
                return absolutePath.Remove(0, basePath.Length);
            for (; basePath.Length > 0 && absolutePath.Length > 0 && (int)char.ToLower(basePath[0]) == (int)char.ToLower(absolutePath[0]); absolutePath = absolutePath.Remove(0, 1))
                basePath = basePath.Remove(0, 1);
            int length = basePath.Split(new char[1]
            {
        Path.DirectorySeparatorChar
            }, StringSplitOptions.RemoveEmptyEntries).Length;
            while (length-- > 0)
            {
                string str1 = "..";
                directorySeparatorChar = Path.DirectorySeparatorChar;
                string str2 = directorySeparatorChar.ToString();
                string str3 = absolutePath;
                absolutePath = str1 + str2 + str3;
            }
            return absolutePath;
        }

        public static string GetAbsolutePath(string basePath, string relativePath)
        {
            basePath = basePath.Trim();
            relativePath = relativePath.Trim();
            if (!Path.IsPathRooted(basePath))
                throw new ArgumentException("Path must be rooted", "basePath");
            if (Path.IsPathRooted(relativePath))
                throw new ArgumentException("Path must be relative", "relativePath");
            string str1 = basePath;
            char directorySeparatorChar = Path.DirectorySeparatorChar;
            string str2 = directorySeparatorChar.ToString() ?? "";
            if (!str1.EndsWith(str2))
            {
                string str3 = basePath;
                directorySeparatorChar = Path.DirectorySeparatorChar;
                string str4 = directorySeparatorChar.ToString();
                basePath = str3 + str4;
            }
            string input = basePath + relativePath;
            Regex regex = new Regex("\\\\[^\\\\]+\\\\\\.\\.");
            while (input.Contains(".."))
                input = regex.Replace(input, "");
            return input;
        }
    }
}
