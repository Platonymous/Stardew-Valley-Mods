using Microsoft.Xna.Framework;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Events;

namespace PyTK.ConsoleCommands
{
    public static class CcTime
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        private static MethodInfo update = Game1.game1.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).ToList().Find(m => m.Name == "Update");

        public static ConsoleCommand skip()
        {
            Action<string> action = delegate (string p)
            {
                int t = Math.Min(Math.Max(int.Parse(p), Game1.timeOfDay), 2400);

                Monitor.Log("Return to the main window.", LogLevel.Info);
                while (Game1.timeOfDay < t)
                {
                        update.Invoke(Game1.game1, new[] { Game1.currentGameTime });
                        if (Game1.CurrentEvent != null)
                            Game1.CurrentEvent.skipEvent();
                        if (Game1.activeClickableMenu is DialogueBox db)
                            db.closeDialogue();
                }
            };

            return new ConsoleCommand("pytk_skip", "Fast forwards to 2200 or specified time of day.", (s,p) => action(p.Length > 0 ? p[0] : "2200"));
        }
    }
}
