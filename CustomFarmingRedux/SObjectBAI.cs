using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using SObject = StardewValley.Object;

namespace CustomFarmingRedux
{
    class SObjectBAI
    {
        public static void Postfix_drawInMenu(SObject __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth)
        {
            if(__instance.preservedParentSheetIndex.Value < 0 && System.Type.GetType("BetterArtisanGoodIcons.ArtisanGoodsManager, BetterArtisanGoodIcons") != null)
                spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(10f * scaleSize, 10f * scaleSize), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (__instance.preservedParentSheetIndex.Value * -1), 16, 16)), Color.White * transparency, 0.0f, new Vector2(4f, 4f), 1.5f * scaleSize, SpriteEffects.None, layerDepth);
        }
    }
}
