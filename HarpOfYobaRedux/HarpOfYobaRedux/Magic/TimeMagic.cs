using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using Microsoft.Xna.Framework;

namespace HarpOfYobaRedux
{
    class TimeMagic : IMagic
    {

        public TimeMagic()
        {

        }

        private void moveTimeForward()
        {
            Game1.playSound("parry");
            Game1.performTenMinuteClockUpdate();
        } 

        private void slowDown()
        {
           
            foreach (GameLocation location in Game1.locations)
            {
                foreach (NPC ch in location.characters)
                {
                    if (ch.isVillager())
                    {
                        ch.addedSpeed = 0;
                    }

                }
            }
        }


        public void doMagic(bool playedToday)
        {
            Game1.player.forceTimePass = true;
            Game1.playSound("stardrop");
            foreach (GameLocation location in Game1.locations)
            {
                foreach (NPC ch in location.characters)
                {
                    if (ch.isVillager())
                    {
                        ch.addedSpeed = 10;
                    }

                }
            }
            
            for (int i = 0; i < 12; i++)
            {
                DelayedAction timeAction = new DelayedAction((i + 1) * 1000 / 2);
                timeAction.behavior = new DelayedAction.delayedBehavior(moveTimeForward);

                Game1.delayedActions.Add(timeAction);
            }

            DelayedAction stopAction = new DelayedAction(7000);
            stopAction.behavior = new DelayedAction.delayedBehavior(slowDown);

            Game1.delayedActions.Add(stopAction);

        }
    }
}
