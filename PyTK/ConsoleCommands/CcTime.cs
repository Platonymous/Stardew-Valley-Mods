using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;
using System.Reflection;

namespace PyTK.ConsoleCommands
{
    public static class CcTime
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        private static MethodInfo update = Game1.game1.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).ToList().Find(m => m.Name == "Update");
        private static MethodInfo draw = Game1.game1.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).ToList().Find(m => m.Name == "Draw");


        public static ConsoleCommand skip()
        {
            return new ConsoleCommand("pytk_skip", "Fast forwards to 2200 or specified time of day.", (s,p) => TimeSkip(p.Length > 0 ? p[0] : "2200", true));
        }

        public static void TimeSkip(string p, bool showTextInConsole = false)
        {
            try
            {
                int t = Math.Min(Math.Max(int.Parse(p), Game1.timeOfDay), 2400);

                if (showTextInConsole)
                    Monitor.Log("Return to the main window.", LogLevel.Info);

                while (Game1.timeOfDay < t)
                {
                    update.Invoke(Game1.game1, new[] { Game1.currentGameTime });
                    if (Game1.CurrentEvent != null)
                        Game1.CurrentEvent.skipEvent();
                    if (Game1.activeClickableMenu is DialogueBox db)
                        db.closeDialogue();
                    Game1.player.forceTimePass = true;
                    Game1.player.freezePause = 1000;
                }
            }
            catch
            {

            }

        }
    }
}
