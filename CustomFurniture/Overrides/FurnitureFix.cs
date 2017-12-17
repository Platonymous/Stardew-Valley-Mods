using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;

namespace CustomFurniture.Overrides
{
    [HarmonyPatch(typeof(Furniture), "drawAtNonTileSpot")]
    public class FurnitureFix
    {
        internal static bool Prefix(Furniture __instance, SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha = 1f)
        {
 
            if (__instance is CustomFurniture ho)
            {
                CustomFurnitureMod.harmonyDraw(ho.texture, location, new Rectangle?(ho.sourceRect), Color.White * alpha, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, ho.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
                return false;
            }


            return true;
        }
    }
}
