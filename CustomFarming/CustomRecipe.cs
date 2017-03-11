using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;

namespace CustomFarming
{
    public class CustomRecipe
    {

        public ICustomFarmingObject item;
        public string materials;
        public int numberProducedPerCraft = 1;

        public CustomRecipe(ICustomFarmingObject item)
        {
            this.item = item;

        }

        public void consumeIngredients()
        {

        }

        public bool doesFarmerHaveIngredientsInInventory(List<Item> items)
        {

            return true;
        }

        public void drawMenuView(SpriteBatch b, int x, int y, float layerDepth = 0.88f, bool shadow = true)
        {
           
           Utility.drawWithShadow(b, item.Texture, new Vector2((float)x, (float)y), item.SourceRectangle, Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, false, layerDepth, -1, -1, 0.35f);
            
        }
    }
}
