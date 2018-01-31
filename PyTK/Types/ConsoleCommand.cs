using System;

namespace PyTK.Types
{
    public class ConsoleCommand
    {
        public string name;
        public string documentation;
        public Action<string, string[]> callback;

        public ConsoleCommand(string name, string documentation, Action<string,string[]> callback)
        {
            this.name = name;
            this.documentation = documentation;
            this.callback = callback;
        }

        public void trigger()
        {
            callback.Invoke(name, new string[] { });
        }

        public void register()
        {
            PyTKMod._helper.ConsoleCommands.Add(name, documentation, callback);
        }
    }
}
