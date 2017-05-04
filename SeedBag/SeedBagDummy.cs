

using StardewValley;


using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SeedBag
{
    class SeedBagDummy : Tool
    {

        private Texture2D texture;

        public SeedBagDummy()

        {
            build();
        }

        public override string Name
        {
            get
            {

                return this.name;

            }
        }


        private void build()
        {
            name = "Seed Bag";
            description = "";

            numAttachmentSlots = 0;


            indexOfMenuItemView = 0;

            texture = SeedBagTool.texture;

        }

      

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            
            spriteBatch.Draw(texture, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), new Rectangle?(Game1.getSquareSourceRectForNonStandardTileSheet(texture, Game1.tileSize / 4, Game1.tileSize / 4, this.indexOfMenuItemView)), Color.White * transparency, 0f, new Vector2((float)(Game1.tileSize / 4 / 2), (float)(Game1.tileSize / 4 / 2)), (float)Game1.pixelZoom * scaleSize, SpriteEffects.None, layerDepth);

            int index = Game1.player.items.FindIndex(x => x is SeedBagDummy);
            if (index != -1)
            {
                Game1.player.items[index] = new SeedBagTool();
            }

        }

      

        public override string getDescription()
        {


                return "";


        }

        protected override string loadDisplayName()
        {
            return name;
        }

        protected override string loadDescription()
        {
            return getDescription();

        }






    }
}
