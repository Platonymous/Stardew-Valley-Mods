using StardewValley;
using StardewValley.Tools;

using CustomElementHandler;

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace HarpOfYobaRedux
{
    internal class Instrument : Tool, ISaveElement
    {

        private static Dictionary<string,Instrument> allInstruments;
        public string instrumentID;
        public bool owned;
        private Texture2D texture;
        private bool readyToPlay;
        private int timeWhenReady;
        private int cooldownTime;
        public IInstrumentAnimation animation;
        private string priorMusic;
        private GameLocation priorLocation;
        public static Dictionary<string, string> allAdditionalSaveData;


        public Instrument()
        {
            
        }

        public static bool hasInstument(string id)
        {
            return allInstruments[id].owned;
        }

        public static void beforeRebuilding()
        {
            foreach(Instrument instrument in allInstruments.Values)
            {
                instrument.owned = false;
            }
        }

        public override string Name
        {
            get
            {
                return getDisplayName();
            }
        }

        public override bool canBeDropped()
        {
            return false;
        }

        public override bool canBeGivenAsGift()
        {
            return false;
        }

        public override bool canBeTrashed()
        {
            return false;
        }

        public Instrument(string id, Texture2D texture, string name, string description, IInstrumentAnimation animation)
        {
            if (allInstruments == null)
            {
                allInstruments = new Dictionary<string, Instrument>();
            }

            this.animation = animation;
            this.name = name;
            this.description = description;
            this.texture = texture;
            instrumentID = id;

            if (allInstruments.ContainsKey(id))
            {
                allInstruments.Remove(id);
            }
            owned = false;

            allInstruments.Add(id,this);
            
        }

        public Instrument(string id)
        {
            build(id);
        }

        private void build(string id)
        {
            name = allInstruments[id].name;
            description = allInstruments[id].description;
            texture = allInstruments[id].texture;
            animation = allInstruments[id].animation;
            readyToPlay = true;
            cooldownTime = 60;
            numAttachmentSlots = 1;
            attachments = new StardewValley.Object[numAttachmentSlots];
            owned = true;
            allInstruments[id].owned = true;
            instantUse = true;
            instrumentID = id;

            if (allAdditionalSaveData == null)
            {
                allAdditionalSaveData = new Dictionary<string, string>();
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
            StardewValley.Object priorAttachement = (StardewValley.Object) null;

            if (attachments.Length > 0 && attachments[0] != null)
            {
                priorAttachement = (SheetMusic) attachments[0].getOne();
            }
        

            if (o is SheetMusic)
            {
                    attachments[0] = o;
                    Game1.playSound("button1");

            return priorAttachement;
            }
            else if(o == null)
            {
                attachments[0] = (StardewValley.Object)null;
                Game1.playSound("dwop");

                return priorAttachement;
            }

           
            return (StardewValley.Object)null;
        }


        public override Item getOne()
        {
            return this;
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            Dictionary<string, string> additionalSaveData = new Dictionary<string, string>();
            
            additionalSaveData.Add("id", instrumentID);

            foreach (string key in allAdditionalSaveData.Keys)
            {
                additionalSaveData.Add(key, allAdditionalSaveData[key]);
            }

            return additionalSaveData;
        }

        public dynamic getReplacement()
        {
            FishingRod replacement = new FishingRod(1);
            replacement.upgradeLevel = -1;
            replacement.attachments = this.attachments;
            return replacement;
        }


        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            build(additionalSaveData["id"]);
            this.attachments = (replacement as Tool).attachments;
            allInstruments[instrumentID].owned = true;

            foreach (string key in additionalSaveData.Keys)
            {
                if (key != "id" && !allAdditionalSaveData.ContainsKey(key))
                {
                    allAdditionalSaveData.Add(key, additionalSaveData[key]);
                }

            }

        }

        public override string getDescription()
        {
            return loadDescription();
        }

        protected override string loadDescription()
        {
            string text = this.description;
            if(attachments.Length > 0 && attachments[0] is SheetMusic)
            {
                text = (attachments[0] as SheetMusic).name;
            }
            SpriteFont smallFont = Game1.smallFont;
            int width = Game1.tileSize * 4 + Game1.tileSize / 4;
            return Game1.parseText(text, smallFont, width);
        }


        protected override string loadDisplayName()
        {
            return name;
        }
       

        private string getDisplayName()
        {
            return name;
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, StardewValley.Farmer who)
        {
            if (!readyToPlay)
            {
                Game1.player.canMove = true;
                return;
            }

                if (attachments.Length > 0 && attachments[0] is SheetMusic)
            {
                int timeOfDay = Game1.timeOfDay;
                int hours = (int)Math.Floor((double)timeOfDay / 100);
                int minutes = timeOfDay - (hours * 100);
                int totalminutes = hours * 60 + (minutes);
                timeWhenReady = totalminutes + cooldownTime;
                readyToPlay = false;
                Game1.player.canMove = true;

                play();

                
            }
            else
            {
                Game1.showRedMessage("Attach Sheet Music to play.");
                Game1.player.canMove = true;
            }

        }

        private void animatePlay()
        {
            animation.animate();
        }

        private void doMagic()
        {
            
            SheetMusic sheet = (SheetMusic)attachments[0];
            sheet.doMagic();
        }

        private void play()
        {
            if (attachments[0] == null)
            {
                return;
            }

            priorMusic = Game1.currentSong.Name;
            priorLocation = Game1.currentLocation;

            SheetMusic sheet = (SheetMusic)attachments[0];

            DelayedAction startAction = new DelayedAction(1000);
            startAction.behavior = new DelayedAction.delayedBehavior(animatePlay);

            DelayedAction magicAction = new DelayedAction(sheet.lenght/2);
            magicAction.behavior = new DelayedAction.delayedBehavior(doMagic);

            DelayedAction stopMusicAction = new DelayedAction(sheet.lenght);
            stopMusicAction.behavior = new DelayedAction.delayedBehavior(resetMusic);

            DelayedAction stopAction = new DelayedAction(sheet.lenght + 1000);
            stopAction.behavior = new DelayedAction.delayedBehavior(stop);

            Game1.delayedActions.Add(startAction);
            Game1.delayedActions.Add(stopMusicAction);
            Game1.delayedActions.Add(stopAction);
            Game1.delayedActions.Add(magicAction);

            

            animation.preAnimation();
            
            sheet.play();

        }

        private void stop()
        {
            if(priorLocation == null)
            {
                priorLocation = Game1.currentLocation;
            }
            
            animation.stop();
            Delivery.checkForProgress(priorLocation, (SheetMusic)attachments[0]);
        }
        
        private void resetMusic()
        {
            if (Game1.currentLocation == priorLocation)
            {
                Game1.changeMusicTrack(priorMusic);
            }
        }

        public new bool beginUsing(GameLocation location, int x, int y, StardewValley.Farmer who)
        {
            return false;
        }
        

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            float alpha = 1.0f;
            int minutesTillReady = 0;
            if (!readyToPlay)
            {
                int timeOfDay = Game1.timeOfDay;
                int hours = (int) Math.Floor((double) timeOfDay / 100);
                int minutes = timeOfDay - (hours * 100);
                int totalminutes = hours * 60 + minutes;
                minutesTillReady = timeWhenReady - totalminutes;
                

                int milliseconds = Game1.currentGameTime.TotalGameTime.Milliseconds;

                if ( (milliseconds % 1000 >= 500) || readyToPlay)
                {
                    alpha = 0.6f;
                }
                else
                {
                    alpha = 0.4f;
                }

                if (minutesTillReady <= 0)
                {
                    readyToPlay = true;
                    minutesTillReady = 0;
                    alpha = 1f;
                }

            }

            Rectangle sourceRectangle = new Microsoft.Xna.Framework.Rectangle(32, 0, 16, 16);
            spriteBatch.Draw(texture, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangle), Color.White * (alpha * transparency), 0.0f, new Vector2(8, 8), (float)Game1.pixelZoom * ((double)scaleSize < 0.2 ? scaleSize : scaleSize), SpriteEffects.None, layerDepth);

            if (!readyToPlay && Game1.activeClickableMenu == null)
            {
                Utility.drawTinyDigits(minutesTillReady/10, spriteBatch, location + new Vector2((float)(24 * scaleSize), (float)(40 * scaleSize / 2)), 4f * scaleSize, 1f, Microsoft.Xna.Framework.Color.White * alpha);
            }

        }

        public override void drawAttachments(SpriteBatch b, int x, int y)
        {
            Rectangle attachementSourceRectangle = new Rectangle(64, 0, 64, 64);
            b.Draw(texture, new Vector2((float)x, (float)y), new Microsoft.Xna.Framework.Rectangle?(attachementSourceRectangle), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);

            if (attachments.Length > 0 && attachments[0] != null)
            {
                attachments[0].drawInMenu(b, new Vector2((float)x, (float)y), 1f);
            }
        }

    }
}
