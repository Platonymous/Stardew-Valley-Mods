using PyTK.CustomElementHandler;
using PyTK.Types;

namespace PyTK.ConsoleCommands
{
    public static class CcSaveHandler
    {
        public static ConsoleCommand cleanup()
        {
            return new ConsoleCommand("pytk_cleanup", "Removes all custom element leftovers", (s, p) => SaveHandler.Cleanup());
        }
    }
}
