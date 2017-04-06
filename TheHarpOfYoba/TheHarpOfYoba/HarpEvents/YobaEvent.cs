using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Tools;
using System.Xml.Serialization;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using StardewValley.Monsters;
using Microsoft.Xna.Framework.Audio;
using xTile.Dimensions;
using StardewValley.Locations;


namespace TheHarpOfYoba
{
    class YobaEvent : HarpEvents
    {

        private HarpOfYoba harp;
        private bool played_before;
        private int oldRadius;

        public YobaEvent()
        {



        }

        public override void beforePlaying(bool p, HarpOfYoba h)
        {
            this.harp = h;
            this.played_before = p;
            this.oldRadius = Game1.player.magneticRadius;

            this.harp.playNewMusic();

            DelayedAction delayedAction1 = new DelayedAction(1000);
            delayedAction1.behavior = new DelayedAction.delayedBehavior(startPlaying);
            Game1.delayedActions.Add(delayedAction1);


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

            if (Game1.currentLocation.isOutdoors)
            {

                List<Vector2> treetiles = new List<Vector2>();


                GameLocation gls = Game1.currentLocation;

                foreach (var keyV in gls.terrainFeatures.Keys)
                {
                    if (gls.terrainFeatures[keyV] is Tree || gls.terrainFeatures[keyV] is FruitTree || gls.terrainFeatures[keyV] is Grass)
                    {
                        treetiles.Add(keyV);

                    }
                }


                for (int i = 0; i < treetiles.Count(); i++)
                {
                    bool treegrow = false;
                    if (!this.played_before)
                    {
                        treegrow = true;
                        Game1.player.doEmote(28);
                    }




                    if (gls.terrainFeatures[treetiles[i]] is Tree)
                    {

                        Tree cT = (Tree)gls.terrainFeatures[treetiles[i]];
                        if (cT.growthStage < 4 && treegrow)
                        {
                            cT.growthStage++;
                        }
                        cT.performUseAction(treetiles[i]);


                        gls.terrainFeatures[treetiles[i]] = cT;
                    }

                    if (gls.terrainFeatures[treetiles[i]] is FruitTree)
                    {

                        FruitTree cT = (FruitTree)gls.terrainFeatures[treetiles[i]];
                        if (cT.growthStage < 4 && treegrow)
                        {
                            cT.growthStage++;
                        }
                        cT.performUseAction(treetiles[i]);


                        gls.terrainFeatures[treetiles[i]] = cT;
                    }

                    if (gls.terrainFeatures[treetiles[i]] is Grass)
                    {

                        Grass gT = (Grass)gls.terrainFeatures[treetiles[i]];
                        gT.numberOfWeeds = gT.numberOfWeeds + Game1.random.Next(1, 4);
                        gT.numberOfWeeds = Math.Min(gT.numberOfWeeds, 4);

                        gT.doCollisionAction(gT.getBoundingBox(treetiles[i]), 3, treetiles[i], Game1.player, Game1.currentLocation);



                        gls.terrainFeatures[treetiles[i]] = gT;
                    }

                }
                Game1.player.magneticRadius += 2000;

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
            Game1.player.magneticRadius = this.oldRadius;


        }
    }
}
