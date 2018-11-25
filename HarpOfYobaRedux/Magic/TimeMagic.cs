using PyTK.ConsoleCommands;
using PyTK.Types;
using StardewValley;
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
            STime time = STime.CURRENT + STime.HOUR;
            int timeInt = (time.hour * 100 + time.minute * 10);
            if (timeInt < 2600) 
                Task.Run(() => {
                    try
                    {
                        CcTime.TimeSkip(timeInt.ToString(), false);
                        }
                    catch { }

                    });
        }
    }
}
