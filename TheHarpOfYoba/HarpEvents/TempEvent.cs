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
    class TempEvent : HarpEvents
    {

        private HarpOfYoba harp;
        private bool played_before;

        public TempEvent()
        {



        }

        public override void beforePlaying(bool p, HarpOfYoba h)
        {
            this.harp = h;
            this.played_before = p;


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



        }
    }
}
