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
            Type[] types = new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(string) };
            ConstructorInfo buffInfo = typeof(Buff).GetConstructor(types);
            
            object[] argsPlayed = new object[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, "" };
            object[] argsUnplayed = new object[] { 0, 5, 0, 0, 5000, 0, 0, 0, 0, 0, -3, 0, 2, "" };
            if (buffInfo == null)
            {
                argsPlayed = new object[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, "", "" };
                argsUnplayed = new object[] { 0, 5, 0, 0, 5000, 0, 0, 0, 0, 0, -3, 0, 2, "", "" };
            }
            
            Buff LuckFisher = (Buff) Activator.CreateInstance(typeof(Buff), argsPlayed);

            LuckFisher.description = "Fisherman";
            LuckFisher.millisecondsDuration = 35000 + Game1.random.Next(30000);
            LuckFisher.sheetIndex = 1;
            LuckFisher.which = 999;

            if (!playedToday)
            {
                LuckFisher = (Buff) Activator.CreateInstance(typeof(Buff), argsUnplayed);
                LuckFisher.description = "Fisher King";
                LuckFisher.millisecondsDuration = 65000 + Game1.random.Next(60000);
            }

            LuckFisher.glow = Color.Azure;

            if (!Game1.buffsDisplay.hasBuff(999))
                Game1.buffsDisplay.addOtherBuff(LuckFisher);
        }
    }
}
