
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.TerrainFeatures;


namespace TheHarpOfYoba
{
    class RainEvent : HarpEvents
    {

        private bool was_raining;
        private bool played_before;
        private HarpOfYoba harp;

        public RainEvent()
        {

       

        }

        public override void beforePlaying(bool p, HarpOfYoba h)
        {
            this.harp = h;
            this.played_before = p;

            this.was_raining = Game1.isRaining;

            this.harp.playNewMusic();

            DelayedAction delayedAction1 = new DelayedAction(1000);
            delayedAction1.behavior = new DelayedAction.delayedBehavior(startPlaying);
            Game1.delayedActions.Add(delayedAction1);

            DelayedAction.playSoundAfterDelay("thunder_small", 4000);

            DelayedAction delayedAction2 = new DelayedAction(5000);
            delayedAction2.behavior = new DelayedAction.delayedBehavior(whilePlaying);
            Game1.delayedActions.Add(delayedAction2);

          

        }

        public override void startPlaying()
        {

            harp.animateHarp();

        }

        public override void whilePlaying()
        {


            Game1.isRaining = true;

            HoeDirt hd;

            List<Vector2> hdtiles = new List<Vector2>();


            GameLocation gls = Game1.getLocationFromName("Farm");

            foreach (var keyV in gls.terrainFeatures.Keys)
            {
                if (gls.terrainFeatures[keyV] is HoeDirt)
                {
                    hdtiles.Add(keyV);
                }
            }

            for (int i = 0; i < hdtiles.Count(); i++)
            {
                hd = (HoeDirt)gls.terrainFeatures[hdtiles[i]];
                hd.state = 1;
                gls.terrainFeatures[hdtiles[i]] = hd;
            }

            DelayedAction delayedAction2 = new DelayedAction(4500);
            delayedAction2.behavior = new DelayedAction.delayedBehavior(stopPlaying);
            Game1.delayedActions.Add(delayedAction2);

            DelayedAction delayedAction = new DelayedAction(6000);
            delayedAction.behavior = new DelayedAction.delayedBehavior(afterPlaying);
            Game1.delayedActions.Add(delayedAction);
        }

        public override void stopPlaying()
        {

            harp.stopHarp();
            Game1.nextMusicTrack = "none";
            DelayedAction.playMusicAfterDelay(harp.oldMusic, 10000);
        }

        public override void afterPlaying()
        {

            Game1.isRaining = was_raining;

        }
    }
}
