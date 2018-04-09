using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Reflection;


namespace PyTK.Lua
{
    public class LuaUtils
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        public static bool log(string text)
        {
            Monitor.Log(text, LogLevel.Trace);
            return true;
        }

        public static bool setGameValue(string field, object value, int delay = 0, object root = null)
        {
            List<string> tree = new List<string>(field.Split('.'));
            FieldInfo fieldInfo = null;

            object currentBranch = root == null ? Game1.game1 : root;

            fieldInfo = typeof(Game1).GetField(tree[0], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            tree.Remove(tree[0]);

            if (tree.Count > 0)
                foreach (string branch in tree)
                {
                    currentBranch = fieldInfo.GetValue(currentBranch);
                    fieldInfo = currentBranch.GetType().GetField(branch, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }

            if (delay > 0)
                PyUtils.setDelayedAction(delay, () => fieldInfo.SetValue(fieldInfo.IsStatic ? null : currentBranch, value));
            else
                fieldInfo.SetValue(fieldInfo.IsStatic ? null : currentBranch, value);

            return true;
        }

        public static string getObjectType(object o)
        {
            return o.GetType().ToString().Split(',')[0];
        }


    }
}
