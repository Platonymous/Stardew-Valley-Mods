using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace Visualize
{
    [HarmonyPatch(typeof(SpriteBatch), "InternalDraw")]
    internal class SpriteBatchFix
    {
        internal static bool Prefix(ref SpriteBatch __instance, ref Texture2D texture, ref Vector4 destination, ref bool scaleDestination, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effects, ref float depth)
        {
            if (!VisualizeMod.callDrawHandlers(ref __instance, ref texture, ref destination, ref scaleDestination, ref sourceRectangle, ref color, ref rotation, ref origin, ref effects, ref depth))
                return false;

            return VisualizeMod._handler.Draw(ref __instance, ref texture, ref destination, ref scaleDestination, ref sourceRectangle, ref color, ref rotation, ref origin, ref effects, ref depth);
        }
    }
    
    [HarmonyPatch(typeof(GameLocation), "drawLightGlows")]
    internal class GameLocationFix
    {
        internal static bool Prefix(ref GameLocation __instance)
        {
            if (VisualizeMod._activeProfile.noLightsources)
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(SpriteBatch), "Begin", new[] { typeof(SpriteSortMode), typeof(BlendState), typeof(SamplerState), typeof(DepthStencilState), typeof(RasterizerState), typeof(Effect), typeof(Matrix) })]
    internal class SpriteBatchFix2
    {
        internal static bool Prefix(ref SpriteBatch __instance, ref SpriteSortMode sortMode, ref BlendState blendState, ref SamplerState samplerState, ref DepthStencilState depthStencilState, ref RasterizerState rasterizerState, ref Effect effect, ref Matrix transformMatrix)
        {
            if (!VisualizeMod.callBeginHandlers(ref __instance, ref sortMode, ref blendState, ref samplerState, ref depthStencilState, ref rasterizerState, ref effect, ref transformMatrix))
                return true;

            VisualizeMod._handler.Begin(ref __instance, ref sortMode, ref blendState, ref samplerState, ref depthStencilState, ref rasterizerState, ref effect, ref transformMatrix);
            return true;
        }
    }

}
