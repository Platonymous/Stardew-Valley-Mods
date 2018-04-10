using Microsoft.Xna.Framework;
using PyTK.Extensions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.Reflection;
using SObject = StardewValley.Object;


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

        public static string getFullObjectType(object o)
        {
            return o.GetType().AssemblyQualifiedName;
        }

        public static void registerTypeFromObject(object obj, bool showErrors = true, bool registerAssembly = false, Func<Type, bool> predicate = null)
        {
            PyLua.registerTypeFromObject(obj, showErrors, registerAssembly, predicate);
        }

        public static void registerTypeFromString(string fullTypeName, bool showErrors = true, bool registerAssembly = false, Func<Type, bool> predicate = null)
        {
            PyLua.registerTypeFromString(fullTypeName, showErrors, registerAssembly, predicate);
        }

        public static void loadGlobals()
        {
            PyLua.loadGlobals();
        }

        public static void addGlobal(string name, object obj)
        {
            PyLua.addGlobal(name, obj);
        }

        public static object getObjectByName(string name, bool bigCraftable = false)
        {
            int index = Game1.objectInformation.getIndexByName(name);
            return getObjectByIndex(index, bigCraftable);
        }

        public static object getObjectByIndex(int index, bool bigCraftable = false)
        {
            if (bigCraftable)
                return new SObject(Vector2.Zero, index);
            return new SObject(index, 1);
        }
    }
}
