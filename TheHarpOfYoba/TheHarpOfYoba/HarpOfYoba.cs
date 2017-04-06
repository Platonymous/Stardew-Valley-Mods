using System;

using System.Collections.Generic;

using StardewValley;
using StardewValley.Locations;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheHarpOfYoba
{
    class HarpOfYoba : Tool
    {
        public Texture2D harpTex;
        public Texture2D harpTex2;

        public Microsoft.Xna.Framework.Rectangle textureBounds = new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16);
        public Microsoft.Xna.Framework.Rectangle textureBounds2 = new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64);

        public SheetMusic sheet;

        public bool charger;
        private int lastCheck;
        private int lastDay;
        private bool isPlaying;
        private int aniTick;
        private int cframe;

        public String oldMusic;

        public List<SheetMusic> playedToday;

        public static bool owned = false;



        public HarpOfYoba(Texture2D ht, Texture2D ht2)
          : base("Harp of Yoba", 0, -1, -1, false, 1)
        {
            this.harpTex = ht;
            this.harpTex2 = ht2;

            this.isPlaying = false;

            this.numAttachmentSlots = 1;

            this.attachments = new StardewValley.Object[this.numAttachmentSlots];

            this.charger = true;

            this.lastCheck = 0;

            this.upgradeLevel = 0;

            this.CurrentParentTileIndex = this.indexOfMenuItemView;

            this.instantUse = true;

            this.sheet = (SheetMusic)null;

            this.stackable = false;

            this.description = "Add Sheet Music to play.";

            this.cframe = 0;

            this.playedToday = new List<SheetMusic>();

            this.lastDay = Game1.dayOfMonth;
            
        }

        public override string getDescription()
        {
            if (this.sheet == null)
            {
                return this.description;

            }
            else
            {
                return this.sheet.name;
            }
        }

        public override int attachmentSlots()
        {
            return numAttachmentSlots;
        }


        public override bool canThisBeAttached(StardewValley.Object o)
        {
            if (o is SheetMusic || o == null) { return true; } else { return false; }
        }

        public override StardewValley.Object attach(StardewValley.Object o)
        {
            if (o != null && o is SheetMusic)
            {
                SheetMusic smo = (SheetMusic)o;
                return attachSheet(smo);
            }


            if (o == null)
            {
                if (this.sheet != null)
                {
                    SheetMusic attachment = this.sheet;
                    this.sheet = (SheetMusic)null;
                    Game1.playSound("dwop");

                    return attachment;

                }

            }
            return (StardewValley.Object)null;
        }

        public SheetMusic attachSheet(SheetMusic o)
        {

            SheetMusic @object = this.sheet;
            this.sheet = o;
            Game1.playSound("button1");

            return @object;

        }


        public override void tickUpdate(GameTime time, StardewValley.Farmer who)
        {


            int tp = Game1.timeOfDay - this.lastCheck;
            if (!this.isPlaying && !this.charger && (tp > 100 || tp < 0))
            {
                this.charger = true;
                this.lastCheck = Game1.timeOfDay;
            }

            if (this.lastDay != Game1.dayOfMonth)
            {
                this.playedToday = new List<SheetMusic>();
            }

            updateGFX();


        }

        public void updateGFX()
        {

            aniTick++;
            if (aniTick >= 10)
            {

                if (cframe >= 4)
                {

                    cframe = 0;
                }
                this.textureBounds = new Microsoft.Xna.Framework.Rectangle(this.cframe * 16, 0, 16, 16);
                this.cframe++;


                if (this.charger)
                {
                    this.textureBounds = new Microsoft.Xna.Framework.Rectangle(4 * 16, 0, 16, 16);

                }
                aniTick = 0;
            }
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {

            spriteBatch.Draw(harpTex, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize - 4)), new Microsoft.Xna.Framework.Rectangle?(this.textureBounds), Microsoft.Xna.Framework.Color.White * transparency, 0f, new Vector2(8f, 16f), (float)Game1.pixelZoom * (((double)scaleSize < 0.2) ? scaleSize : (scaleSize)), SpriteEffects.None, layerDepth);

        }

        public override void drawAttachments(SpriteBatch b, int x, int y)
        {
            if (this.sheet == (SheetMusic)null)
            {
                b.Draw(harpTex2, new Vector2((float)x, (float)y), new Microsoft.Xna.Framework.Rectangle?(textureBounds2), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
            }
            else
            {
                b.Draw(harpTex2, new Vector2((float)x, (float)y), new Microsoft.Xna.Framework.Rectangle?(textureBounds2), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
                this.sheet.drawInMenu(b, new Vector2((float)x, (float)y), 1f);
            }

        }

        public override void DoFunction(GameLocation location, int x, int y, int power, StardewValley.Farmer who)
        {
            Game1.player.canMove = true;
            if (this.sheet != (SheetMusic)null && this.charger && !isPlaying && this.sheet.music != "none")
            {
                Game1.player.canMove = false;
                this.isPlaying = true;
                Game1.player.forceTimePass = true;
                this.lastCheck = Game1.timeOfDay;
                this.charger = false;
                this.playHarp();
            }

        }

        public void stopHarp()
        {

            Game1.player.completelyStopAnimatingOrDoingAction();
            Game1.player.canMove = true;
            this.isPlaying = false;
            this.lastCheck = Game1.timeOfDay;
            Game1.player.forceTimePass = false;

        }


      public void animateHarp()
        {

            String anim = "308 99 98 98 99 100 100";
            string[] split = anim.Split(' ');

            int int32 = Convert.ToInt32(split[0]);
            bool flip = false;
            bool flag = true;
            List<FarmerSprite.AnimationFrame> animation = new List<FarmerSprite.AnimationFrame>();
            for (int index = 1; index < split.Length; ++index)
                animation.Add(new FarmerSprite.AnimationFrame(Convert.ToInt32(split[index]), int32, false, flip, (AnimatedSprite.endOfAnimationBehavior)null, false));

            Game1.player.FarmerSprite.setCurrentAnimation(animation.ToArray());
            Game1.player.FarmerSprite.loopThisAnimation = flag;
            Game1.player.FarmerSprite.PauseForSingleAnimation = true;

        }

        public void playNewMusic()
        {

            Game1.nextMusicTrack = this.sheet.music;

        }
        

        public void playHarp()
        {
           
             if(Game1.currentLocation.name == "Beach")
            {
                Beach bl = (Beach)Game1.currentLocation;

                if (Game1.isRaining && Game1.timeOfDay < 1900 && !Game1.currentSeason.Equals("winter") && bl.bridgeFixed)
                {
                    HarpOfYobaMod.processIndicators[this.sheet.pos] = true;
                }



            }

            Game1.playSound("dwop");
            Game1.player.faceDirection(2);
            Game1.player.showFrame(98);

            bool playedBefore = true;

            oldMusic = Game1.currentSong.Name;
           
            if (!this.playedToday.Contains(this.sheet))
            {
                playedBefore = false;
                this.playedToday.Add(sheet);
            }
                     
            this.sheet.harpEvents.beforePlaying(playedBefore, this);
            
        }

        protected override string loadDisplayName()
        {
            return "Harp of Yoba";
        }

        protected override string loadDescription()
        {
            return "Add Sheet Music to play.";
        }
    }
}
