using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using PyTK.Extensions;
using PyTK.Types;
using PyTK;

namespace HarpOfYobaRedux
{
    class RainMagic : IMagic
    {

        public RainMagic()
        {

        }

        public void doMagic(bool playedToday)
        {
            if (Game1.isRaining || !Game1.currentLocation.isOutdoors)
                return;

            Game1.playSound("thunder_small");

            PyUtils.setDelayedAction(500, () => Game1.isRaining = true);
            PyUtils.setDelayedAction(2000, () => new TerrainSelector<HoeDirt>(h => h.state < 1).keysIn(Game1.currentLocation).useAll(k => (Game1.currentLocation.terrainFeatures[k] as HoeDirt).state = 1));
            PyUtils.setDelayedAction(6000, () => Game1.isRaining = false);
        }


    }
}
