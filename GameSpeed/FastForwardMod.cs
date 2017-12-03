using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using System;

namespace FastForward
{
    public class FastForwardMod : Mod
    {
        private static TimeSpan reset;
        private bool speedup = false;
        private int seconds = 0;

        public override void Entry(IModHelper helper)
        {
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            reset = new TimeSpan(Program.gamePtr.TargetElapsedTime.Ticks);
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            ControlEvents.KeyReleased += ControlEvents_KeyReleased;
            GameEvents.OneSecondTick += GameEvents_OneSecondTick;
        }

        private void ControlEvents_KeyReleased(object sender, EventArgsKeyPressed e)
        {
            if (e.KeyPressed == Keys.PageUp)
            {
                speedup = false;
                seconds = 20;
                Game1.player.addedSpeed = 0;
            }
        }

        private void GameEvents_OneSecondTick(object sender, EventArgs e)
        {
            seconds = seconds == 0 ? 10 : seconds - 1;

            if (speedup && seconds % 2 == 0)
                Game1.performTenMinuteClockUpdate();

            if(seconds == 11)
            {
                Program.gamePtr.TargetElapsedTime = reset;
            }
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (e.KeyPressed == Keys.PageUp)
            {
                speedup = true;
                Program.gamePtr.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 1);
            }
        }
    }
}
