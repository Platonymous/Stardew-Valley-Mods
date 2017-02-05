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
    class TeleportEvent : HarpEvents
    {

        private HarpOfYoba harp;
        private bool played_before;
        private String teleLoc;
        private Vector2 teleV;

        public TeleportEvent()
        {
            this.teleLoc = "";


        }

        public override void beforePlaying(bool p, HarpOfYoba h)
        {
            this.harp = h;
            this.played_before = p;

            this.harp.playNewMusic();

            DelayedAction delayedAction1 = new DelayedAction(2000);
            delayedAction1.behavior = new DelayedAction.delayedBehavior(startPlaying);
            Game1.delayedActions.Add(delayedAction1);


            DelayedAction delayedAction2 = new DelayedAction(5000);
            delayedAction2.behavior = new DelayedAction.delayedBehavior(whilePlaying);
            Game1.delayedActions.Add(delayedAction2);



        }

        public override void startPlaying()
        {
            if (!this.played_before || this.teleLoc =="") { 
            this.teleLoc = "Town";
            this.teleV = new Vector2((float)(53 * Game1.tileSize), (float)(24 * Game1.tileSize + Game1.tileSize / 2));
            }
            harp.animateHarp();

        }

        public override void whilePlaying()
        {
                      
            DelayedAction delayedAction2 = new DelayedAction(3500);
            delayedAction2.behavior = new DelayedAction.delayedBehavior(stopPlaying);
            Game1.delayedActions.Add(delayedAction2);

            DelayedAction delayedAction = new DelayedAction(4000);
            delayedAction.behavior = new DelayedAction.delayedBehavior(afterPlaying);
            Game1.delayedActions.Add(delayedAction);
        }

        public override void stopPlaying()
        {

            harp.stopHarp();
            Game1.nextMusicTrack = "none";
            Game1.player.canMove = false;
            GameLocation location = Game1.currentLocation;
            Farmer who = Game1.player;

            for (int index = 0; index < 12; ++index)
                who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354, (float)Game1.random.Next(25, 75), 6, 1, new Vector2((float)Game1.random.Next((int)who.position.X - Game1.tileSize * 4, (int)who.position.X + Game1.tileSize * 3), (float)Game1.random.Next((int)who.position.Y - Game1.tileSize * 4, (int)who.position.Y + Game1.tileSize * 3)), false, Game1.random.NextDouble() < 0.5));
            Game1.playSound("wand");
            Game1.displayFarmer = false;
            Game1.player.Halt();
            Game1.player.faceDirection(2);
            Game1.player.freezePause = 1000;
            Game1.flashAlpha = 1f;

            new System.Drawing.Rectangle(who.GetBoundingBox().X, who.GetBoundingBox().Y, Game1.tileSize, Game1.tileSize).Inflate(Game1.tileSize * 3, Game1.tileSize * 3);
            int num1 = 0;
            for (int index = who.getTileX() + 8; index >= who.getTileX() - 8; --index)
            {
                List<TemporaryAnimatedSprite> temporarySprites = who.currentLocation.temporarySprites;
                TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite(6, new Vector2((float)index, (float)who.getTileY()) * (float)Game1.tileSize, Microsoft.Xna.Framework.Color.White, 8, false, 50f, 0, -1, -1f, -1, 0);
                temporaryAnimatedSprite.layerDepth = 1f;
                int num2 = num1 * 25;
                temporaryAnimatedSprite.delayBeforeAnimationStart = num2;
                Vector2 vector2 = new Vector2(-0.25f, 0.0f);
                temporaryAnimatedSprite.motion = vector2;
                temporarySprites.Add(temporaryAnimatedSprite);
                ++num1;
            }


        }

        public override void afterPlaying()
        {
            
            String tempLoc = this.teleLoc;
            Vector2 tempV = this.teleV;
            this.teleLoc = Game1.currentLocation.name;
            this.teleV = new Vector2(Game1.player.position.X, Game1.player.position.Y);
            Game1.warpFarmer(tempLoc, (int)tempV.X / Game1.tileSize, (int)tempV.Y / Game1.tileSize, false);

            Game1.changeMusicTrack("none");
            Game1.fadeToBlackAlpha = 0.99f;
            Game1.screenGlow = false;
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;

            Game1.player.canMove = true;

        }
    }
}
