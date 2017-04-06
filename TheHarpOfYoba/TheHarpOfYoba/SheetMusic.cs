using System;

using StardewValley;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace TheHarpOfYoba
{
    class SheetMusic : StardewValley.Object
    {

        public Texture2D sheetTex;
        public Rectangle textureBounds;
        public String music;
        public String magic;
        public int pos;
        public HarpEvents harpEvents;

        public static bool[] owned = {false, false, false, false, false, false};

        public SheetMusic(Texture2D tex, int p, String n, String m, HarpEvents he)
        {

            this.name = n;
            this.sheetTex = tex;
            this.music = m;
            this.textureBounds = new Microsoft.Xna.Framework.Rectangle(p * 16, 0, 16, 16);
            this.magic = m;
            this.pos = p;
            this.harpEvents = he;


        }

        public override int Stack
        {
            get
            {
                return 1;
            }
            set
            {
            }
        }

        public override int maximumStackSize()
        {
            return 1;
        }

        public override int getStack()
        {
            return 1;
        }

        public override int addToStack(int amount)
        {
            return 1;
        }


        public override Item getOne()
        {
            return (Item)new SheetMusic(this.sheetTex, this.pos, this.Name, this.music, this.harpEvents);
        }

        public override bool canBeTrashed()
        {
            return false;
        }

        public override bool canBeGivenAsGift()
        {
            return false;
        }


        public override string getDescription()
        {
            return "Add this Sheet Music to your Harp to play.";
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            spriteBatch.Draw(this.sheetTex, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize * 0.75 + 8)), new Microsoft.Xna.Framework.Rectangle?(this.textureBounds), Microsoft.Xna.Framework.Color.White * transparency, 0f, new Vector2(8f, 16f), 0.8f * (float)Game1.pixelZoom * (((double)scaleSize < 0.2) ? scaleSize : (scaleSize)), SpriteEffects.None, layerDepth);
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, StardewValley.Farmer f)
        {
            spriteBatch.Draw(this.sheetTex, objectPosition, this.textureBounds, Microsoft.Xna.Framework.Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 2) / 10000f));
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            Vector2 value = this.getScale();
            value *= (float)Game1.pixelZoom;
            Vector2 value2 = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize), (float)(y * Game1.tileSize - Game1.tileSize)));
            Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle((int)(value2.X - value.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(value2.Y - value.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)((float)Game1.tileSize + value.X), (int)((float)(Game1.tileSize * 2) + value.Y / 2f));
            spriteBatch.Draw(this.sheetTex, destinationRectangle, this.textureBounds, Microsoft.Xna.Framework.Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + ((this.parentSheetIndex == 105) ? 0.0035f : 0f) + (float)x * 1E-08f);

        }

    }
}
