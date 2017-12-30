using System;
using System.Collections.Generic;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;

namespace HarpOfYobaRedux
{
    internal class SheetMusic : StardewValley.Object, ISaveElement
    {
        private static Dictionary<string, SheetMusic> allSheets;
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
            {
                allSheets = new Dictionary<string, SheetMusic>();
            }

            this.magic = magic;
            this.lenght = lenght;
            this.texture = texture;
            this.color = color;
            this.music = music;
            this.name = name;
            displayName = name;
            sheetDescription = description;
            sheetMusicID = id;

            if (allSheets.ContainsKey(id))
            {
                allSheets.Remove(id);
            }

            owned = false;
            allSheets.Add(id, this);

        }

        public override string Name
        {
            get
            {
                return name;
            }
        }

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
            additionalSaveData.Add("id", this.sheetMusicID.ToString());
            return additionalSaveData;
        }

        public dynamic getReplacement()
        {
    
            return new StardewValley.Object(685,1); 
        }

        public static void beforeRebuilding()
        {
            foreach (SheetMusic sheet in allSheets.Values)
            {
                sheet.owned = false;
            }
        }

        public override Item getOne()
        {
            return new SheetMusic(sheetMusicID);
        }

        public void doMagic()
        {

            magic.doMagic(playedToday);

            playedToday = true;
        }


        public void play()
        {
            Game1.changeMusicTrack(music);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
 
            Rectangle sourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16);
            spriteBatch.Draw(texture, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangle), Color.White * transparency, 0.0f, new Vector2(8, 8), (float)Game1.pixelZoom * scaleSize * 0.8f, SpriteEffects.None, layerDepth);

            Rectangle sourceRectangleNote = new Microsoft.Xna.Framework.Rectangle(16, 0, 16, 16);
            spriteBatch.Draw(texture, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleNote), color * transparency, 0.0f, new Vector2(8, 8), (float)Game1.pixelZoom * scaleSize * 0.8f, SpriteEffects.None, layerDepth);

        }

        public override void drawWhenHeld(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Vector2 objectPosition, StardewValley.Farmer f)
        {
            Rectangle sourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16);
            spriteBatch.Draw(texture, objectPosition, new Microsoft.Xna.Framework.Rectangle?(sourceRectangle), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f));

            Rectangle sourceRectangleNote = new Microsoft.Xna.Framework.Rectangle(16, 0, 16, 16);
            spriteBatch.Draw(texture, objectPosition, new Microsoft.Xna.Framework.Rectangle?(sourceRectangleNote), color, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 3) / 10000f));
        }

    }
}
