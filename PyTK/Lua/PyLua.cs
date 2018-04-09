using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonSharp.Interpreter;
using PyTK.Extensions;
using StardewModdingAPI;
using StardewValley;

using StardewValley.Locations;

using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace PyTK.Lua
{
    public static class PyLua
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;
        internal static Dictionary<string, Script> scripts = new Dictionary<string, Script>();

        internal static void init()
        {
            registerTypes();
            scripts = new Dictionary<string, Script>();
        }

        public static void loadScriptFromString(string scriptCode, string uniqueID = null)
        {
            Script script = new Script();
            script.Globals["Game1"] = Game1.game1;
            script.Globals["Luau"] = new LuaUtils();
            script.DoString(scriptCode);

            if (uniqueID != null)
                scripts.AddOrReplace(uniqueID, script);
        }

        public static void loadScriptFromFile(string path, string uniqueID = null)
        {
            string contents = System.IO.File.ReadAllText(path);
            loadScriptFromString(contents, uniqueID);
        }

        public static void callFunction(string uniqueID, string callFunction, params object[] args)
        {
            if(scripts.ContainsKey(uniqueID))
                scripts[uniqueID].Call(scripts[uniqueID].Globals[callFunction], args);
        }


        private static void registerTypes()
        {
            UserData.RegisterType<LuaUtils>();

            /* XNA */
            UserData.RegisterType<Vector2>();
            UserData.RegisterType<Texture2D>();
            UserData.RegisterType<Point>();
            UserData.RegisterType<Rectangle>();

            /* SDV */
            var types = typeof(Game1).Assembly.DefinedTypes;
            foreach(var tp in types)
            {
                Type at = tp.AsType();
                if (at.ToString().Contains("SerializableDictionary"))
                    continue;
                try
                {
                    UserData.RegisterType(at);
                }
                catch { }
            }

        }

       
    }
}
