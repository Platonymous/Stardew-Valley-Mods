using StardewValley;
using StardewValley.Tools;
using SObject = StardewValley.Object;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;

namespace HarpOfYobaRedux
{

    [XmlType("Mods_platonymous_HarpOfYoba_Instrument")]
    public class Instrument : Tool
    {
        internal static Dictionary<string,Instrument> allInstruments = new Dictionary<string, Instrument>();
        public string instrumentID { get; set; }
        private Texture2D texture;
        private bool readyToPlay;
        private int timeWhenReady;
        private int cooldownTime;
        internal IInstrumentAnimation animation;
        private string priorMusic;
        private GameLocation priorLocation;
        public static Dictionary<string, string> allAdditionalSaveData { get; set; } = new Dictionary<string, string>();
        public Instrument()
        {
            build("harp");
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
                allInstruments = new Dictionary<string, Instrument>();

            this.animation = animation;
            this.Name = name;
            displayName = name;
            this.description = description;
            this.texture = texture;
            instrumentID = id;
           

            if (allInstruments.ContainsKey(id))
                allInstruments.Remove(id);

            allInstruments.Add(id,this);
        }

        public Instrument(string id)
        {
            build(id);
        }

        private void build(string id)
        {
            Name = allInstruments[id].Name;
            description = allInstruments[id].description;
            texture = allInstruments[id].texture;
            animation = allInstruments[id].animation;
            readyToPlay = true;
            cooldownTime = 60;
            numAttachmentSlots.Value = 1;
            attachments.SetCount(numAttachmentSlots);
            InstantUse = true;
            instrumentID = id;

            if (allAdditionalSaveData == null)
                allAdditionalSaveData = new Dictionary<string, string>();
        }

        public override int attachmentSlots()
        {
            return numAttachmentSlots;
        }

        public override bool canThisBeAttached(SObject o)
        {
            if (o is SheetMusic || o == null) { return true; } else { return false; }
        }

        public override SObject attach(SObject o)
        {
            SObject priorAttachement = null;

            if (attachments.Length > 0 && attachments[0] != null)
            {
                priorAttachement = (SObject) attachments[0].getOne();
            }

            if (o is SheetMusic)
            {
                attachments[0] = o;
                Game1.playSound("button1");
                return priorAttachement;
            }
            else if (o == null)
            {
                attachments[0] = null;
                Game1.playSound("dwop");
                return priorAttachement;
            }

            return null;
        }


        protected override Item GetOneNew()
        {
            var one = new Instrument(instrumentID);
            foreach (var att in attachments)
            {
                if (att is SheetMusic sm)
                    one.attachments.Add(new SheetMusic(sm.sheetMusicID));
            }
            return one;
        }

        protected override void GetOneCopyFrom(Item source)
        {
        }


        public override string getDescription()
        {
            string text = description;
            if (attachments.Length > 0 && attachments[0] is SheetMusic)
            {
                text = (attachments[0] as SheetMusic).name;
            }
            SpriteFont smallFont = Game1.smallFont;
            int width = Game1.tileSize * 4 + Game1.tileSize / 4;
            return Game1.parseText(text, smallFont, width);
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

        private void doMagic()
        {
            SheetMusic sheet = (SheetMusic)attachments[0];
            sheet.doMagic();
        }

        private void play()
        {
            if (attachments[0] == null)
                return;

            priorMusic = Game1.currentSong.Name;
            priorLocation = Game1.currentLocation;

            SheetMusic sheet = (SheetMusic)attachments[0];

            Game1.delayedActions.Add(new DelayedAction(1000, animation.animate));
            Game1.delayedActions.Add(new DelayedAction(sheet.length / 2, doMagic));
            Game1.delayedActions.Add(new DelayedAction(sheet.length, resetMusic));
            Game1.delayedActions.Add(new DelayedAction(sheet.length + 1000, stop));

            animation.preAnimation();
            sheet.play();
        }

        private void stop()
        {
            if(priorLocation == null)
                priorLocation = Game1.currentLocation;
            
            animation.stop();
            Delivery.checkForProgress(priorLocation, (SheetMusic)attachments[0]);
        }
        
        private void resetMusic()
        {
            if (Game1.currentLocation == priorLocation)
                Game1.changeMusicTrack(priorMusic);
        }

        public new bool beginUsing(GameLocation location, int x, int y, StardewValley.Farmer who)
        {
            return false;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
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
                    alpha = 0.6f;
                else
                    alpha = 0.4f;

                if (minutesTillReady <= 0)
                {
                    readyToPlay = true;
                    minutesTillReady = 0;
                    alpha = 1f;
                }
            }

            Rectangle sourceRectangle = new Rectangle(32, 0, 16, 16);
            spriteBatch.Draw(texture, location + new Vector2((Game1.tileSize / 2), (Game1.tileSize / 2)), new Rectangle?(sourceRectangle), Color.White * (alpha * transparency), 0.0f, new Vector2(8, 8), Game1.pixelZoom * (scaleSize < 0.2 ? scaleSize : scaleSize), SpriteEffects.None, layerDepth);

            if (!readyToPlay && Game1.activeClickableMenu == null)
                Utility.drawTinyDigits(minutesTillReady/10, spriteBatch, location + new Vector2((24 * scaleSize), (40 * scaleSize / 2)), 4f * scaleSize, 1f, Color.White * alpha);

        }

        public override void drawAttachments(SpriteBatch b, int x, int y)
        {
            Rectangle attachementSourceRectangle = new Rectangle(64, 0, 64, 64);
            b.Draw(texture, new Vector2(x, y), new Rectangle?(attachementSourceRectangle), Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);

            if (attachments.Length > 0 && attachments[0] != null)
                attachments[0].drawInMenu(b, new Vector2(x, y), 1f);
        }

        protected override string loadDisplayName()
        {
            return Name;
        }

        protected override string loadDescription()
        {
            return description;
        }

    }
}
