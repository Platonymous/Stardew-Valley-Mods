using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Visualize
{
    class SpritbatchFixNew
    {

        internal static void initializePatch(Harmony instance)
        {
            foreach (MethodInfo method in typeof(SpritbatchFixNew).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => m.Name == "Draw"))
            {
                try
                {
                    instance.Patch(typeof(SpriteBatch).GetMethod("Draw", method.GetParameters().Select(p => p.ParameterType.Name.Contains("Texture2D") ? typeof(Texture2D) : p.ParameterType.Name.Contains("Color") ? typeof(Color) : p.ParameterType).Where(t => !t.Name.Contains("SpriteBatch")).ToArray()), new HarmonyMethod(method), null, null);
                }
                catch (Exception e)
                {
                    VisualizeMod._monitor.Log(method.Name + "(" + string.Join(",", method.GetParameters().Select(p => p.ParameterType.ToString())) + ")",StardewModdingAPI.LogLevel.Error);
                    VisualizeMod._monitor.Log(e.Message + ":" + e.StackTrace, StardewModdingAPI.LogLevel.Error);
                }
            }

            foreach (MethodInfo method in typeof(SpritbatchFixNew).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => m.Name == "DrawString"))
            {
                try
                {
                    instance.Patch(typeof(SpriteBatch).GetMethod("DrawString", method.GetParameters().Select(p => p.ParameterType.Name.Contains("Color") ? typeof(Color) : p.ParameterType).Where(t => !t.Name.Contains("SpriteBatch")).ToArray()), new HarmonyMethod(method), null, null);
                }
                catch (Exception e)
                {
                    VisualizeMod._monitor.Log(method.Name + "(" + string.Join(",", method.GetParameters().Select(p => p.ParameterType.ToString())) + ")", StardewModdingAPI.LogLevel.Error);
                    VisualizeMod._monitor.Log(e.Message + ":" + e.StackTrace, StardewModdingAPI.LogLevel.Error);
                }
            }
        }

        public static bool DrawFix(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, Vector2 origin, float rotation, SpriteEffects effects, float layerDepth)
        {
            if (!VisualizeMod.active)
                return true;

            if (texture.Format != SurfaceFormat.Color)
                return true;
            
            if ((VisualizeMod._activeProfile.id == "Platonymous.Original" || VisualizeMod._activeProfile.id == "auto") && VisualizeMod._config.saturation == 100 && VisualizeMod.palette.Count == 0)
                return true;

            if (!VisualizeMod.callDrawHandlers(__instance, texture, destinationRectangle, sourceRectangle, color, origin, rotation, effects, layerDepth))
                return false;

            return VisualizeMod._handler.Draw(__instance, texture, destinationRectangle, sourceRectangle, color, origin, rotation, effects, layerDepth);
        }

        public static bool DrawStringFix(SpriteBatch __instance, SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation = 0f, Vector2? origin = null, float scale = 1f, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            if (!VisualizeMod.active)
                return true;

            if ((VisualizeMod._activeProfile.id == "Platonymous.Original" || VisualizeMod._activeProfile.id == "auto") && VisualizeMod._config.saturation == 100 && VisualizeMod.palette.Count == 0)
                return true;

            if (!VisualizeMod.callDrawHandlers(__instance, spriteFont, text, position, color, rotation, origin, scale, effects, layerDepth))
                return false;

            return VisualizeMod._handler.Draw(__instance, spriteFont, text, position,color, rotation, origin, scale, effects, layerDepth);
        }

        public static bool DrawString(SpriteBatch __instance, SpriteFont spriteFont, string text, Vector2 position, Color color)
        {

            return DrawStringFix(__instance,spriteFont,text,position,color);
        }

        public static bool DrawString(SpriteBatch __instance, SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
        {
     
            return DrawStringFix(__instance, spriteFont, text.ToString(), position, color);
        }

        public static bool DrawString(SpriteBatch __instance, SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
  
            return DrawStringFix(__instance, spriteFont, text, position, color,rotation,origin,scale,effects,layerDepth);
        }

        public static bool DrawString(SpriteBatch __instance, SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {

            return DrawStringFix(__instance, spriteFont, text.ToString(), position, color, rotation, origin, scale, effects, layerDepth);
        }

        public static bool DrawString(SpriteBatch __instance, SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {

            return DrawStringFix(__instance, spriteFont, text, position, color, rotation, origin,1f, effects, layerDepth);
        }

        public static bool DrawString(SpriteBatch __instance, SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {

            return DrawStringFix(__instance, spriteFont, text.ToString(), position, color, rotation, origin, scale.X, effects, layerDepth);
        }


        public static bool Draw(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            return DrawFix(__instance, texture, destinationRectangle, sourceRectangle, color, origin, rotation, effects, layerDepth);
        }
        public static bool Draw(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
        {
            var origin = Vector2.Zero;
            var rotation = 0f;
            var layerDepth = 0f;
            SpriteEffects effects = SpriteEffects.None;
            return DrawFix(__instance, texture, destinationRectangle, sourceRectangle, color, origin,rotation,effects, layerDepth);
        }
        public static bool Draw(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            var origin = Vector2.Zero;
            var rotation = 0f;
            var layerDepth = 0f;
            SpriteEffects effects = SpriteEffects.None;
            Rectangle? sourceRectangle = new Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height));
            return DrawFix(__instance, texture, destinationRectangle, sourceRectangle, color, origin, rotation, effects, layerDepth);
        }
        public static bool Draw(SpriteBatch __instance, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            sourceRectangle = sourceRectangle.HasValue ? sourceRectangle.Value : new Rectangle(0, 0, texture.Width, texture.Height);
            var destinationRectangle = new Rectangle((int)(position.X), (int)(position.Y), (int)(sourceRectangle.Value.Width * scale.X), (int)(sourceRectangle.Value.Height * scale.Y));

            return DrawFix(__instance, texture, destinationRectangle, sourceRectangle, color, origin, rotation, effects, layerDepth);
        }
        public static bool Draw(SpriteBatch __instance, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            sourceRectangle = sourceRectangle.HasValue ? sourceRectangle.Value : new Rectangle(0, 0, texture.Width, texture.Height);
            var destinationRectangle = new Rectangle((int)(position.X), (int)(position.Y), (int)(sourceRectangle.Value.Width * scale), (int)(sourceRectangle.Value.Height * scale));
            return DrawFix(__instance, texture, destinationRectangle, sourceRectangle, color, origin, rotation, effects, layerDepth);
        }
        public static bool Draw(SpriteBatch __instance, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
        {
            var origin = Vector2.Zero;
            var rotation = 0f;
            var layerDepth = 0f;
            SpriteEffects effects = SpriteEffects.None;
            sourceRectangle = sourceRectangle.HasValue ? sourceRectangle.Value : new Rectangle(0, 0, texture.Width, texture.Height);
            var destinationRectangle = new Rectangle((int)(position.X), (int)(position.Y), (int)(sourceRectangle.Value.Width), (int)(sourceRectangle.Value.Height));
            return DrawFix(__instance, texture, destinationRectangle, sourceRectangle, color, origin, rotation, effects, layerDepth);
        }
        public static bool Draw(SpriteBatch __instance, Texture2D texture, Vector2 position, Color color)
        {
            var origin = Vector2.Zero;
            var rotation = 0f;
            var layerDepth = 0f;
            SpriteEffects effects = SpriteEffects.None;
            Rectangle? sourceRectangle = new Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height));
            var destinationRectangle = new Rectangle((int)(position.X), (int)(position.Y), (int)(texture.Width), (int)(texture.Height));
            return DrawFix(__instance, texture, destinationRectangle, sourceRectangle, color, origin, rotation, effects, layerDepth);

        }
    }
}
