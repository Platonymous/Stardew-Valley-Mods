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

        public static ConsoleCommand savecheck()
        {
            return new ConsoleCommand("pytk_savecheck", "Checks all savefiles for XML errors", (s, p) => PyUtils.checkAllSaves());
        }
    }
}
