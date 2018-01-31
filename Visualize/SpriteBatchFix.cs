using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
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
                return AccessTools.Method(typeof(Visualize.FakeSpriteBatch), "DrawInternal");
        }
        
        internal static bool Prefix(ref SpriteBatch __instance, Texture2D __state, ref Texture2D texture, ref Vector4 destinationRectangle, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effect, ref float depth)
        {
            if (!VisualizeMod.active)
                return true;

            if (texture.Format != SurfaceFormat.Color)
                return true;

            __state = texture;

            bool scaleDestination = false;

            if (!VisualizeMod.callDrawHandlers(ref __instance, ref texture, ref destinationRectangle, ref scaleDestination, ref sourceRectangle, ref color, ref rotation, ref origin, ref effect, ref depth))
                return false;

            if (VisualizeMod._activeProfile.id == "Platonymous.Original" && VisualizeMod._config.saturation == 100 && VisualizeMod.palette.Count == 0)
                return true;

            return VisualizeMod._handler.Draw(ref __instance, ref texture, ref destinationRectangle, ref scaleDestination, ref sourceRectangle, ref color, ref rotation, ref origin, ref effect, ref depth);
        }

        internal static void Postfix(ref Texture2D texture, Texture2D __state)
        {
            texture = __state;
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
                return AccessTools.Method(typeof(Visualize.FakeSpriteBatch), "InternalDraw");
        }

        internal static bool Prefix(ref SpriteBatch __instance, Texture2D __state, ref Texture2D texture, ref Vector4 destination, ref bool scaleDestination, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effects, ref float depth)
        {
            if (!VisualizeMod.active) 
                return true;

            if (texture.Format != SurfaceFormat.Color)
                return true;
            
            __state = texture;

            if (!VisualizeMod.callDrawHandlers(ref __instance, ref texture, ref destination, ref scaleDestination, ref sourceRectangle, ref color, ref rotation, ref origin, ref effects, ref depth))
                return false;

            if (VisualizeMod._activeProfile.id == "Platonymous.Original" && VisualizeMod._config.saturation == 100 && VisualizeMod.palette.Count == 0)
                return true;

            return VisualizeMod._handler.Draw(ref __instance, ref texture, ref destination, ref scaleDestination, ref sourceRectangle, ref color, ref rotation, ref origin, ref effects, ref depth);
        }

        internal static void Postfix(ref Texture2D texture, Texture2D __state)
        {
            texture = __state;
        }

    }

    [HarmonyPatch]
    internal class GameLocationFix
    {
        internal static MethodInfo TargetMethod()
        {
            if (Type.GetType("StardewValley.GameLocation, Stardew Valley") != null)
                return AccessTools.Method(Type.GetType("StardewValley.GameLocation, Stardew Valley"), "drawLightGlows");
            else
                return AccessTools.Method(Type.GetType("StardewValley.GameLocation, StardewValley"), "drawLightGlows");
        }

        internal static bool Prefix(ref GameLocation __instance)
        {
            if (!VisualizeMod.active)
                return true;

            if (VisualizeMod._activeProfile.noLightsources)
                return false;

            return true;
        }
    }
 
 [HarmonyPatch]
 internal class SpriteBatchFix2Mono
 {
     internal static MethodInfo TargetMethod()
     {
         if (Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, MonoGame.Framework") != null)
             return AccessTools.Method(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, MonoGame.Framework"), "Begin", new[] { typeof(SpriteSortMode), typeof(BlendState), typeof(SamplerState), typeof(DepthStencilState), typeof(RasterizerState), typeof(Effect), typeof(Matrix?) });
         else
             return AccessTools.Method(typeof(Visualize.FakeSpriteBatch), "Begin", new[] { typeof(SpriteSortMode), typeof(BlendState), typeof(SamplerState), typeof(DepthStencilState), typeof(RasterizerState), typeof(Effect), typeof(Matrix?) });
     }

     internal static bool Prefix(ref SpriteBatch __instance, ref SpriteSortMode sortMode, ref BlendState blendState, ref SamplerState samplerState, ref DepthStencilState depthStencilState, ref RasterizerState rasterizerState, ref Effect effect, ref Matrix? transformMatrix)
     {
         if (!VisualizeMod.active)
             return true;

         VisualizeMod._handler.Begin(ref __instance, ref sortMode, ref blendState, ref samplerState, ref depthStencilState, ref rasterizerState, ref effect, ref transformMatrix);
         return true;
     }
 }

 [HarmonyPatch]
 internal class SpriteBatchFix2
 {
     internal static MethodInfo TargetMethod()
     {
         if (Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, Microsoft.Xna.Framework.Graphics") != null)
             return AccessTools.Method(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, Microsoft.Xna.Framework.Graphics"), "Begin", new[] { typeof(SpriteSortMode), typeof(BlendState), typeof(SamplerState), typeof(DepthStencilState), typeof(RasterizerState), typeof(Effect), typeof(Matrix) });
         else
             return AccessTools.Method(typeof(Visualize.FakeSpriteBatch), "Begin", new[] { typeof(SpriteSortMode), typeof(BlendState), typeof(SamplerState), typeof(DepthStencilState), typeof(RasterizerState), typeof(Effect), typeof(Matrix) });
     }

     internal static bool Prefix(ref SpriteBatch __instance, ref SpriteSortMode sortMode, ref BlendState blendState, ref SamplerState samplerState, ref DepthStencilState depthStencilState, ref RasterizerState rasterizerState, ref Effect effect, ref Matrix transformMatrix)
     {
         if (!VisualizeMod.active)
             return true;

         VisualizeMod._handler.Begin(ref __instance, ref sortMode, ref blendState, ref samplerState, ref depthStencilState, ref rasterizerState, ref effect, ref transformMatrix);
         return true;
     }
 }


}
