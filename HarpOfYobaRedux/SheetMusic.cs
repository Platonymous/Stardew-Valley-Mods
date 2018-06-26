using System;
using System.Collections.Generic;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using PyTK.Extensions;

namespace HarpOfYobaRedux
{
    internal class SheetMusic : StardewValley.Object, ISaveElement
    {
        internal static Dictionary<string, SheetMusic> allSheets;
        public string sheetMusicID;
        private string sheetDescription;
        private bool owned;
        private Texture2D texture;
        private Color color;
        private string music;
        public int lenght;
        public IMagic magic;
        public bool playedToday;
 

        public SheetMusic()
        {

        }

        public SheetMusic(string id, Texture2D texture, string name, string description, Color color, string music, int lenght, IMagic magic)
        {
            if (allSheets == null)
                allSheets = new Dictionary<string, SheetMusic>();

            this.magic = magic;
            this.lenght = lenght;
            this.texture = texture;
            this.color = color;
            this.music = music;
            this.name = name;
            displayName = name;
            sheetDescription = description;
            sheetMusicID = id;

            allSheets.AddOrReplace(id, this);
            owned = false;
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
            lenght = allSheets[id].lenght;
            magic = allSheets[id].magic;
            allSheets[id].owned = true;
            owned = true;
            sheetMusicID = id;
            playedToday = false;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            build(additionalSaveData["id"]);
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

        public static bool hasSheet(string id)
        {
           return allSheets[id].owned;
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

        public static void beforeRebuilding()
        {
            foreach (SheetMusic sheet in allSheets.Values)
                sheet.owned = false;
        }

        public override Item getOne()
        {
            return new SheetMusic(sheetMusicID);
        }

        public void doMagic()
        {
            if(HarpOfYobaReduxMod.config.magic)
                magic.doMagic(playedToday);

            playedToday = true;
        }


        public void play()
        {
            Game1.changeMusicTrack(music);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            Rectangle sourceRectangle = new Rectangle(0, 0, 16, 16);
            spriteBatch.Draw(texture, location + new Vector2((Game1.tileSize / 2), (Game1.tileSize / 2)), new Rectangle?(sourceRectangle), Color.White * transparency, 0.0f, new Vector2(8, 8), Game1.pixelZoom * scaleSize * 0.8f, SpriteEffects.None, layerDepth);
            Rectangle sourceRectangleNote = new Rectangle(16, 0, 16, 16);
            spriteBatch.Draw(texture, location + new Vector2((Game1.tileSize / 2), (Game1.tileSize / 2)), new Rectangle?(sourceRectangleNote), this.color * transparency, 0.0f, new Vector2(8, 8), Game1.pixelZoom * scaleSize * 0.8f, SpriteEffects.None, layerDepth);
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, StardewValley.Farmer f)
        {
            Rectangle sourceRectangle = new Rectangle(0, 0, 16, 16);
            spriteBatch.Draw(texture, objectPosition, new Rectangle?(sourceRectangle), Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f));
            Rectangle sourceRectangleNote = new Rectangle(16, 0, 16, 16);
            spriteBatch.Draw(texture, objectPosition, new Rectangle?(sourceRectangleNote), color, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 3) / 10000f));
        }

    }
}
