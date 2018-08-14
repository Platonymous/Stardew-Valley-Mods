using PyTK.ConsoleCommands;
using StardewValley;
using System;
using System.Threading.Tasks;

namespace HarpOfYobaRedux
{
    class TimeMagic : IMagic
    {
        public TimeMagic()
        {

        }

        public void doMagic(bool playedToday)
        {
            Game1.player.forceTimePass = true;
            Game1.playSound("stardrop");
            if (Game1.timeOfDay < 2400)
                Task.Run(() => CcTime.TimeSkip((Game1.timeOfDay).ToString(), false));
        }
    }
}
