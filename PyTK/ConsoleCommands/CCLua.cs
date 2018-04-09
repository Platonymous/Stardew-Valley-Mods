using PyTK.Types;
using System;
using System.Collections.Generic;
using PyTK.Lua;

namespace PyTK.ConsoleCommands
{
    public static class CcLua
    {
        public static ConsoleCommand runScript()
        {
            Action<string,string[]> action = delegate (string s, string[] p)
            {
                if (p.Length < 1)
                    return;

                List<string> args = new List<string>(p);
   
                if(p[0] == "-f")
                {
                    args.Remove(p[0]);
                    string fn = String.Join(" ", args);
                    PyLua.loadScriptFromFile(fn);
                    return;
                }

                PyLua.loadScriptFromString(String.Join(" ", args));

            };

            return new ConsoleCommand("lua", "Runs lua code, just write code or use lua -f YOUR_PATH to load a file", (s, p) => action.Invoke(s,p));
        }
    }
}
