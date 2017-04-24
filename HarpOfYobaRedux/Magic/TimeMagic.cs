
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Timers;

namespace HarpOfYobaRedux
{
    class TimeMagic : IMagic
    {
        private TimeSpan oldTS;
        int targetTime;
        int currentTime;
        Game1 gamePtr;


        public TimeMagic()
        {

        }

        private void moveTimeForward()
        {

            Game1.playSound("parry");
            Game1.performTenMinuteClockUpdate();
            currentTime = Game1.timeOfDay;

            if (currentTime == targetTime)
            {
                gamePtr.TargetElapsedTime = oldTS;
            }

        }

        public void doMagic(bool playedToday)
        {
            Type type = typeof(Game1).Assembly.GetType("StardewValley.Program", true);
            gamePtr = (Game1) type.GetField("gamePtr").GetValue(null);

            Game1.player.forceTimePass = true;
            Game1.playSound("stardrop");
            targetTime = Game1.timeOfDay + 200;
            currentTime = Game1.timeOfDay;
            oldTS = new TimeSpan(gamePtr.TargetElapsedTime.Ticks);

            gamePtr.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 1);

            for (int i = 0; i < 12; i++)
            {
                DelayedAction timeAction = new DelayedAction((i + 1) * 1000 / 2 - i * 20);
                timeAction.behavior = new DelayedAction.delayedBehavior(moveTimeForward);

                Game1.delayedActions.Add(timeAction);
            }


        }
    }
}
