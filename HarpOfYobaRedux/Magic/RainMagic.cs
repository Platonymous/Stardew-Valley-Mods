using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using PyTK.Extensions;
using PyTK.Types;
using PyTK;
using System;

namespace HarpOfYobaRedux
{
    class RainMagic : IMagic
    {
        private int maxDist;
        private Random r = new Random();

        public RainMagic()
        {

        }

        public void doMagic(bool playedToday)
        {
            maxDist = playedToday ? 5 : 9;

            if (Game1.isRaining || !Game1.currentLocation.isOutdoors)
                return;

            Game1.playSound("thunder_small");

            PyUtils.setDelayedAction(500, () => Game1.isRaining = true);
            PyUtils.setDelayedAction(2000, () => new TerrainSelector<HoeDirt>(h => h.state < 1).keysIn(Game1.currentLocation).useAll(k => water(k)));
            PyUtils.setDelayedAction(6000, () => Game1.isRaining = false);
        }

        private double getDistance(Vector2 i, Vector2 j)
        {
            float distX = Math.Abs(j.X - i.X);
            float distY = Math.Abs(j.Y - i.Y);
            double dist = Math.Sqrt((distX * distX) + (distY * distY));
            return dist;
        }

        private void water(Vector2 k)
        {
            if(getDistance(Game1.player.getTileLocation(), k) < maxDist)
            (Game1.currentLocation.terrainFeatures[k] as HoeDirt).state = 1;
        }


    }
}
