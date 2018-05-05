using PyTK.Types;
using System;
using System.Collections.Generic;
using PyTK.Lua;
using StardewModdingAPI;
using System.Net;

namespace PyTK.ConsoleCommands
{
    public static class CcLua
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        public static ConsoleCommand runScript()
        {
        Action<string,string[]> action = delegate (string s, string[] p)
            {
                if (p.Length < 1)
                    return;

                List<string> args = new List<string>(p);

                string fn = "";
                if (p[0] == "--f")
                {
                    args.Remove(p[0]);
                    fn = String.Join(" ", args);
                    PyLua.loadScriptFromFile(fn, PyLua.consoleCacheID);
                    return;
                }

                if(args[0] == "--reload")
                {
                    Monitor.Log("Reloading..", LogLevel.Trace);
                    PyLua.scripts.Remove(PyLua.consoleCacheID);
                    PyLua.loadScriptFromFile(PyLua.consoleChache, PyLua.consoleCacheID);
                    Monitor.Log("OK", LogLevel.Trace);
                    return;
                }

                if (args[0] == "--clear")
                {
                    Monitor.Log("Clearing..", LogLevel.Trace);
                    PyLua.scripts.Remove(PyLua.consoleCacheID);
                    Monitor.Log("OK", LogLevel.Trace);
                    return;
                }

                try
                {
                    PyLua.loadScriptFromString(String.Join(" ", args), "consoleScript");

                    if (args[0] == "--save")
                    {
                        PyLua.saveScriptToFile(PyLua.consoleCacheID, PyLua.consoleChache);
                        Monitor.Log("Saving..", LogLevel.Trace);
                    }

                    Monitor.Log("OK", LogLevel.Trace);
                }catch(Exception e)
                {
                    string em = "ERROR: LUA Script crashed. ";
                    Monitor.Log(em + e.Message, LogLevel.Alert);
                }
            };

            return new ConsoleCommand("lua", "Runs lua code, just write code or use lua -f YOUR_PATH to load a file", (s, p) => action.Invoke(s,p));
        }


        internal static string downloadString(string url)
        {
            string s = "";
            using (WebClient client = new WebClient())
            {
                s = client.DownloadString(url);
            }
            return s;
        }
    }
}
