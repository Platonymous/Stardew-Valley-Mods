
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace HarpOfYobaRedux
{
    class RainMagic : IMagic
    {

        public RainMagic()
        {

        }

        private void waterCrops()
        {

            List<Vector2> hdtiles = new List<Vector2>();

            GameLocation gl = Game1.currentLocation;

            if(gl.terrainFeatures != null && gl.isOutdoors) { 

            foreach (var keyV in gl.terrainFeatures.Keys)
            {
                if (gl.terrainFeatures[keyV] is HoeDirt)
                {
                    hdtiles.Add(keyV);
                }
            }
            
            for (int i = 0; i < hdtiles.Count; i++)
            {
                (gl.terrainFeatures[hdtiles[i]] as HoeDirt).state = 1;
            }

            }
        }

        public void startRain()
        {
            Game1.isRaining = true;
        }

        public void doMagic(bool playedToday)
        {
            
            if (Game1.isRaining || !Game1.currentLocation.isOutdoors)
            {
                return;
            }
            Game1.playSound("thunder_small");

            DelayedAction startAction = new DelayedAction(500);
            startAction.behavior = new DelayedAction.delayedBehavior(startRain);

            DelayedAction rainAction = new DelayedAction(2000);
            rainAction.behavior = new DelayedAction.delayedBehavior(waterCrops);

            DelayedAction stopAction = new DelayedAction(6000);
            stopAction.behavior = new DelayedAction.delayedBehavior(stopRaining);

            Game1.delayedActions.Add(startAction);
            Game1.delayedActions.Add(rainAction);
            Game1.delayedActions.Add(stopAction);

        }

        private void stopRaining()
        {
            Game1.isRaining = false;
        }

    }
}
