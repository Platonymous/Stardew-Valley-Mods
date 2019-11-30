using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Reflection;

namespace Visualize
{
    [HarmonyPatch]
    internal class SpriteBatchFixMono
    {
        internal static MethodInfo TargetMethod()
        {
            if (Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, MonoGame.Framework") != null)
                return AccessTools.Method(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, MonoGame.Framework"), "DrawInternal");
            else
                return AccessTools.Method(typeof(FakeSpriteBatch), "DrawInternal");
        }
        
        internal static bool Prefix(ref SpriteBatch __instance, Texture2D __state, ref Texture2D texture, ref Vector4 destinationRectangle, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effect, ref float depth)
        {
            if (!VisualizeMod.active)
                return true;

            if (texture.Format != SurfaceFormat.Color)
                return true;

            bool scaleDestination = false;

            if (!VisualizeMod.callDrawHandlers(ref __instance, ref texture, ref destinationRectangle, ref scaleDestination, ref sourceRectangle, ref color, ref rotation, ref origin, ref effect, ref depth))
                return false;

            if ((VisualizeMod._activeProfile.id == "Platonymous.Original" || VisualizeMod._activeProfile.id == "auto") && VisualizeMod._config.saturation == 100 && VisualizeMod.palette.Count == 0)
                return true;

            return VisualizeMod._handler.Draw(ref __instance, ref texture, ref destinationRectangle, ref scaleDestination, ref sourceRectangle, ref color, ref rotation, ref origin, ref effect, ref depth);
        }


    }

    [HarmonyPatch]
    internal class SpriteBatchFix
    {
        internal static MethodInfo TargetMethod()
        {
            if(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, Microsoft.Xna.Framework.Graphics") != null)
                return AccessTools.Method(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, Microsoft.Xna.Framework.Graphics"), "InternalDraw");
            else
                return AccessTools.Method(typeof(FakeSpriteBatch), "InternalDraw");
        }

        internal static bool Prefix(ref SpriteBatch __instance, Texture2D __state, ref Texture2D texture, ref Vector4 destination, ref bool scaleDestination, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effects, ref float depth)
        {
            if (!VisualizeMod.active) 
                return true;

            if (texture.Format != SurfaceFormat.Color)
                return true;

            if (!VisualizeMod.callDrawHandlers(ref __instance, ref texture, ref destination, ref scaleDestination, ref sourceRectangle, ref color, ref rotation, ref origin, ref effects, ref depth))
                return false;

            if ((VisualizeMod._activeProfile.id == "Platonymous.Original" || VisualizeMod._activeProfile.id == "auto") && VisualizeMod._config.saturation == 100 && VisualizeMod.palette.Count == 0)
                return true;

            return VisualizeMod._handler.Draw(ref __instance, ref texture, ref destination, ref scaleDestination, ref sourceRectangle, ref color, ref rotation, ref origin, ref effects, ref depth);
        }


    }

}
