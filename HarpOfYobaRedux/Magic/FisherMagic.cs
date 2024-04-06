using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Reflection;

namespace HarpOfYobaRedux
{
    class FisherMagic : IMagic
    {

        public FisherMagic()
        {

        }

        public void doMagic(bool playedToday)
        {
            Buff LuckFisher = new Buff("hoy.fisherman", displayName: "Fisherman", duration: 35000 + Game1.random.Next(30000), description: "");

            LuckFisher.millisecondsDuration = 35000 + Game1.random.Next(30000);
            LuckFisher.iconSheetIndex = 1;
            LuckFisher.iconTexture = Game1.buffsIcons;

            LuckFisher.effects.FishingLevel.Value = 1;

            if (!playedToday)
            {
                LuckFisher.description = "";
                LuckFisher.displayName = "Fisher King";
                LuckFisher.millisecondsDuration = 65000 + Game1.random.Next(60000);
                LuckFisher.effects.FishingLevel.Value = 5;
                LuckFisher.effects.LuckLevel.Value = 5000;
                LuckFisher.effects.MaxStamina.Value = 100;
            }

            LuckFisher.glow = Color.Azure;
            if (!Game1.player.hasBuff("hoy.fisherman"))
                Game1.player.applyBuff(LuckFisher);
        }
    }
}
