using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Reflection;

namespace CustomFurniture.Overrides
{
    [HarmonyPatch]
    public class FurnitureFix
    {
        internal static MethodInfo TargetMethod()
        {
            if (Type.GetType("StardewValley.Objects.Furniture, Stardew Valley") != null)
                return AccessTools.Method(Type.GetType("StardewValley.Objects.Furniture, Stardew Valley"), "drawAtNonTileSpot");
            else
                return AccessTools.Method(Type.GetType("StardewValley.Objects.Furniture, StardewValley"), "drawAtNonTileSpot");
        }

        internal static bool Prefix(Furniture __instance, SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha = 1f)
        {
 
            if (__instance is CustomFurniture ho)
            {
                CustomFurnitureMod.harmonyDraw(ho.texture, location, new Rectangle?(ho.sourceRect.Value), Color.White * alpha, 0.0f, Vector2.Zero, Game1.pixelZoom, ho.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    public class FurnitureFix2
    {
        internal static MethodInfo TargetMethod()
        {
            if (Type.GetType("StardewValley.Objects.Furniture, Stardew Valley") != null)
                return AccessTools.Method(Type.GetType("StardewValley.Objects.Furniture, Stardew Valley"), "rotate");
            else
                return AccessTools.Method(Type.GetType("StardewValley.Objects.Furniture, StardewValley"), "rotate");
        }

        internal static bool Prefix(Furniture __instance)
        {
            if (__instance is CustomFurniture cf)
                cf.customRotate();
            else
                return true;

            return false;
        }
    }
}
