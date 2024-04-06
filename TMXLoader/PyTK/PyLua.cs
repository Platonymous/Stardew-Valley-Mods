using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using xTile;

namespace TMXLoader
{
    public static class PyLua
    {
        internal static IModHelper Helper { get; } = TMXLoaderMod.helper;
        internal static IMonitor Monitor { get; } = TMXLoaderMod.monitor;
        internal static Dictionary<string, Script> scripts = new Dictionary<string, Script>();
        internal static string scripthead = "--[[ Begin Script ]]";
        internal static string consoleChache = Path.Combine(Helper.DirectoryPath, "console.lua");
        internal static string consoleCacheID = "consoleScript";
        internal static Dictionary<string, object> luaGlobals = new Dictionary<string, object>();

        internal static void init()
        {
            UserData.DefaultAccessMode = InteropAccessMode.LazyOptimized;

            Task.Run(() => {

                registerTypes();

                if (!File.Exists(consoleChache))
                    File.WriteAllText(consoleChache, "");
                addGlobal("Game1", Game1.game1);
                addGlobal("Luau", new LuaUtils());
                addGlobal("Pyu", new PyUtils());
                addGlobal("Color", new Color());
                addGlobal("Vecor2", new Vector2());

                loadGlobals();
                loadScriptFromFile(consoleChache, consoleCacheID);
            });
            scripts = new Dictionary<string, Script>();
            
        }

        public static bool hasScript(string uniqueId)
        {
            return scripts.ContainsKey(uniqueId);
        }

        public static void addGlobal(string name, object obj)
        {
            luaGlobals.Remove(name);
            luaGlobals.Add(name, obj);
            foreach (Script s in scripts.Values)
                    s.Globals[name] = obj;
        }

        public static void loadScriptFromString(string scriptCode, string uniqueID = null)
        {
            Script script = scripts.ContainsKey(uniqueID) ? scripts[uniqueID] : getNewScript();

            script.DoString(scriptCode);

            if (uniqueID != null)
            {
                scripts.Remove(uniqueID);
                scripts.Add(uniqueID, script);
                script.Globals["SID"] = uniqueID;
            }

        }

        internal static Script getNewScript()
        {
            Script script = new Script();

            foreach (KeyValuePair<string, object> global in luaGlobals)
                script.Globals[global.Key] = global.Value;

            script.LoadString(scripthead);

            return script;
        }

        public static void loadScriptFromFile(string path, string uniqueID = null)
        {
            string contents = File.ReadAllText(path);

            loadScriptFromString(contents, uniqueID);
        }

        public static void callFunction(string uniqueID, string callFunction, params object[] args)
        {
            if (scripts.ContainsKey(uniqueID))
                scripts[uniqueID].Call(scripts[uniqueID].Globals[callFunction], args);
        }

        public static void loadGlobals()
        {
            foreach (Script s in scripts.Values)
                foreach (KeyValuePair<string, object> global in luaGlobals)
                    s.Globals[global.Key] = global.Value;
        }


        public static void registerType(Type type, bool showErrors = true, bool registerAssembly = false, Func<Type, bool> predicate = null)
        {
            if (!registerAssembly)
            {
                try
                {
                    UserData.RegisterType(type);
                }
                catch (Exception e)
                {
                    if (showErrors)
                        Monitor.Log("ERROR: " + e.Message, LogLevel.Alert);
                }
            }
            else
            {
                var types = type.Assembly.DefinedTypes;
                foreach (var tp in types)
                {
                    Type at = tp.AsType();

                    if (predicate != null && !predicate.Invoke(at))
                        continue;

                    try
                    {
                        UserData.RegisterType(at);
                    }
                    catch (Exception e)
                    {
                        if (showErrors)
                            Monitor.Log("ERROR: " + e.Message, LogLevel.Alert);
                    }
                }
            }
        }

        public static void registerTypeFromObject(object obj, bool showErrors = true, bool registerAssembly = false, Func<Type, bool> predicate = null)
        {
            registerType(obj.GetType(), showErrors, registerAssembly, predicate);
        }

        public static void registerTypeFromString(string fullTypeName, bool showErrors = true, bool registerAssembly = false, Func<Type, bool> predicate = null)
        {
            registerType(Type.GetType(fullTypeName), showErrors, registerAssembly, predicate);
        }

        private static void registerTypes()
        {
            

            registerType(typeof(LuaUtils),false,true);

            /* XNA */
            registerType(typeof(Vector2), false, true);

            /* SDV */
            registerType(typeof(Game1), false, true);
            registerType(typeof(NetInt), false, true);

            /*XTile*/
            registerType(typeof(Map), false, true);
        }

        public static void saveScriptToFile(string uniqueId, string path)
        {
            if (!scripts.ContainsKey(uniqueId))
                return;

            Script s = scripts[uniqueId];
            string source = "";
            int c = s.SourceCodeCount;
            bool rec = false;
            for (int i = 0; i < c; i++)
            {
                string next = s.GetSourceCode(i).Code;
               
                if (rec && next.Contains("function"))
                    source += next.Replace("--save", "") + Environment.NewLine;

                if (next.Contains(scripthead))
                    rec = true;
            }
      

            File.WriteAllText(path, source);
        }       
    }
}
