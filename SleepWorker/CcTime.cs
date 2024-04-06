using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Linq;
using System.Reflection;

namespace SleepWorker
{
    public static class CcTime
    {

        private static MethodInfo update = Game1.game1.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).ToList().Find(m => m.Name == "Update");

        internal static bool skippingTime = false;
        internal static int targetTime = -1;
        internal static int cycles = 0;
        internal static Action Callback = null;
        internal static int lastTime = 0;

        private static IModHelper Helper;

        public static void TimeSkip(IModHelper helper, int time, Action callback)
        {
            Helper = helper;
            targetTime = time;
            cycles = 0;
            Callback = callback;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        public static void TimeSkip(IModHelper helper, string p, bool showTextInConsole = false)
        {
            Helper = helper;
            targetTime = Math.Min(Math.Max(int.Parse(p), Game1.timeOfDay), 2400);
            cycles = 0;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private static void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.Escape && cycles != 0)
            {
                cycles = 2001;
                Helper.Events.Input.ButtonPressed -= Input_ButtonPressed;
            }

        }

        private static void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (skippingTime)
                return;

            for (int i = 0; i < 60; i++)
            {

                try
                {
                    if (Game1.timeOfDay >= targetTime || cycles > 2000)
                    {
                        skippingTime = false;
                        Program.gamePtr.IsFixedTimeStep = true;
                        cycles = 0;
                        Callback?.Invoke();
                        Callback = null;
                        Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
                        return;
                    }
                    if (Game1.timeOfDay != lastTime)
                    {
                        lastTime = Game1.timeOfDay;
                        Game1.playSound("smallSelect");
                    }
                    skippingTime = true;
                    Game1.player.freezePause = 100;
                    Game1.player.forceTimePass = true;
                    Program.gamePtr.IsFixedTimeStep = false;
                    update.Invoke(Game1.game1, new[] { new AltGameTime(Game1.currentGameTime.TotalGameTime, Game1.currentGameTime.ElapsedGameTime) });
                    Program.gamePtr.IsFixedTimeStep = true;
                    skippingTime = false;
                }
                catch
                {
                    skippingTime = false;
                    Program.gamePtr.IsFixedTimeStep = true;
                }
            }
            cycles++;

        }
    }

    internal class AltGameTime : GameTime
    {
        public AltGameTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime)
            : base(totalGameTime, elapsedGameTime)
        {

        }
    }
}
