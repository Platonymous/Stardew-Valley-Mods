using System;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
namespace CustomTV
{
    public class CustomTVMod : Mod
    {

        internal static IModHelper Modhelper;
        internal static IMonitor monitor;

        public override void Entry(IModHelper helper)
        {
            Modhelper = Helper;
            monitor = Monitor;
        }

        public static void changeAction(string id, Action<TV, TemporaryAnimatedSprite, StardewValley.Farmer, string> action)
        {
            PyTK.CustomTV.CustomTVMod.changeAction(id, action);
        }

        public static void removeChannel(string key)
        {
            PyTK.CustomTV.CustomTVMod.removeKey(key);
        }

        public static void addChannel(string id, string name, Action<TV, TemporaryAnimatedSprite, StardewValley.Farmer, string> action = null)
        {
            PyTK.CustomTV.CustomTVMod.addChannel(id, name, action);
        }

        public static void endProgram()
        {
            PyTK.CustomTV.CustomTVMod.endProgram();
        }

        public static void showProgram(TemporaryAnimatedSprite sprite, string text, Action afterDialogues = null, TemporaryAnimatedSprite overlay = null)
        {
            PyTK.CustomTV.CustomTVMod.showProgram(sprite, text, afterDialogues, overlay);
        }

        public static void showProgram(TemporaryAnimatedSprite sprite, string text, Action afterDialogues = null)
        {
            PyTK.CustomTV.CustomTVMod.showProgram(sprite, text, afterDialogues, null);
        }
    }
}
