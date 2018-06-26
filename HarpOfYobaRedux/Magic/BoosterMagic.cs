using Microsoft.Xna.Framework;
using StardewValley;

namespace HarpOfYobaRedux
{
    class BoosterMagic : IMagic
    {
        public BoosterMagic()
        {

        }

        public void doMagic(bool playedToday)
        {
            Buff buff = new Buff(22);
            buff.glow = Color.Orange;
            buff.description = "Adventure!";
            buff.millisecondsDuration = 15000 + Game1.random.Next(15000);

            if (!playedToday)
            {
                Game1.player.stamina = Game1.player.maxStamina.Value;
                Game1.player.health = Game1.player.maxHealth;
                buff.millisecondsDuration = 35000 + Game1.random.Next(30000);
            }

            if (!Game1.buffsDisplay.hasBuff(22))
                Game1.buffsDisplay.addOtherBuff(buff);
        }
    }
}
