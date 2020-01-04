using System;
using System.Collections.Generic;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using PyTK.Extensions;

namespace HarpOfYobaRedux
{
    internal class SheetMusic : StardewValley.Object, ISaveElement, ICustomObject
    {
        internal static Dictionary<string, SheetMusic> allSheets;
        public string sheetMusicID;
        private string sheetDescription;
        private Texture2D texture;
        private Color color;
        private string music;
        public int length;
        public IMagic magic;
        public bool playedToday = false;

        public SheetMusic()
        {
            
        }

        public SheetMusic(CustomObjectData data)
            :this(data.id.Split('.')[2])
        {



        }

        public SheetMusic(string id, Texture2D texture, string name, string description, Color color, string music, int length, IMagic magic)
        {
            if (allSheets == null)
                allSheets = new Dictionary<string, SheetMusic>();

            this.magic = magic;
            this.length = length;
            this.texture = texture;
            this.color = color;
            this.music = music;
            this.name = name;
            displayName = name;
            sheetDescription = description;
            sheetMusicID = id;
            allSheets.AddOrReplace(id, this);
        }

        public override string Name { get => name; }

        public override string DisplayName { get => name; set => name = value; }

        public SheetMusic(string id)
        {
            build(id);
        }

        private void build(string id)
        {
            name = allSheets[id].name;
            sheetDescription = allSheets[id].sheetDescription;
            color = allSheets[id].color;
            texture = allSheets[id].texture;
            music = allSheets[id].music;
            length = allSheets[id].length;
            magic = allSheets[id].magic;
            sheetMusicID = id;
            playedToday = false;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            build(additionalSaveData["id"]);
        }

        public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
        {
            restore();
        }

        public override bool canBeDropped()
        {
            return false;
        }

        public void restore()
        {
            if (texture == null)
                foreach (var s in allSheets)
                    if (s.Value.Name == Name)
                    {
                        build(s.Value.sheetMusicID);
                        break;
                    }
        }

        public override bool canBeGivenAsGift()
        {
            return false;
        }

        public override bool canBeTrashed()
        {
            return false;
        }

        public override string getDescription()
        {
            string text = this.sheetDescription;
            SpriteFont smallFont = Game1.smallFont;
            int width = Game1.tileSize * 4 + Game1.tileSize / 4;
            return Game1.parseText(text, smallFont, width);
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            Dictionary<string, string> additionalSaveData = new Dictionary<string, string>();
            additionalSaveData.Add("id", sheetMusicID.ToString());
            return additionalSaveData;
        }

        public object getReplacement()
        {
            return new StardewValley.Object(685,1); 
        }

        public override Item getOne()
        {
            return new SheetMusic(sheetMusicID);
        }

        public void doMagic()
        {
            if(HarpOfYobaReduxMod.config.magic)
                magic?.doMagic(playedToday);

            playedToday = true;
        }


        public void play()
        {
            Game1.changeMusicTrack(music);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            restore();
            if (texture == null)
                return;
            Rectangle sourceRectangle = new Rectangle(0, 0, 16, 16);
            spriteBatch.Draw(texture, location + new Vector2((Game1.tileSize / 2), (Game1.tileSize / 2)), new Rectangle?(sourceRectangle), Color.White * transparency, 0.0f, new Vector2(8, 8), Game1.pixelZoom * scaleSize * 0.8f, SpriteEffects.None, layerDepth);
            Rectangle sourceRectangleNote = new Rectangle(16, 0, 16, 16);
            spriteBatch.Draw(texture, location + new Vector2((Game1.tileSize / 2), (Game1.tileSize / 2)), new Rectangle?(sourceRectangleNote), this.color * transparency, 0.0f, new Vector2(8, 8), Game1.pixelZoom * scaleSize * 0.8f, SpriteEffects.None, layerDepth);
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, StardewValley.Farmer f)
        {
            restore();
            if (texture == null)
                return;
            Rectangle sourceRectangle = new Rectangle(0, 0, 16, 16);
            spriteBatch.Draw(texture, objectPosition, new Rectangle?(sourceRectangle), Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f));
            Rectangle sourceRectangleNote = new Rectangle(16, 0, 16, 16);
            spriteBatch.Draw(texture, objectPosition, new Rectangle?(sourceRectangleNote), color, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 3) / 10000f));
        }

        public ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            return new SheetMusic(additionalSaveData["id"]);
        }
    }
}
