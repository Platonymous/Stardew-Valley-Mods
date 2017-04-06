using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarpOfYobaRedux
{
    class FisherMagic : IMagic
    {

        public FisherMagic()
        {

        }

        public void doMagic(bool playedToday)
        {
            Buff LuckFisher = new Buff(0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, "", "");
            LuckFisher.description = "Fisherman";
            LuckFisher.millisecondsDuration = 35000 + Game1.random.Next(30000);
            LuckFisher.sheetIndex = 1;
            LuckFisher.which = 999;

            if (!playedToday)
            {
                LuckFisher = new Buff(0, 5, 0, 0, 5000, 0, 0, 0, 0, 0, -3, 0, 2, "", "");
                LuckFisher.description = "Fisher King";
                LuckFisher.millisecondsDuration = 65000 + Game1.random.Next(60000);
            }

            LuckFisher.glow = Color.Azure;

            if (!Game1.buffsDisplay.hasBuff(999))
            {
                Game1.buffsDisplay.addOtherBuff(LuckFisher);
            }

        }
    }
}
