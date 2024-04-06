using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using StardewValley.Menus;

namespace CustomMovies
{
    class OvSpritebatchNew
    {
        internal static Dictionary<string, Dictionary<Rectangle?, Texture2D>> repTextures = new Dictionary<string, Dictionary<Rectangle?, Texture2D>>();

        internal static bool skip = false;

        internal static void initializePatch(Harmony instance)
        {
            foreach (MethodInfo method in typeof(OvSpritebatchNew).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => m.Name == "Draw"))
                instance.Patch(typeof(SpriteBatch).GetMethod("Draw", method.GetParameters().Select(p => p.ParameterType).Where(t => !t.Name.Contains("SpriteBatch")).ToArray()), new HarmonyMethod(method), null, null);
        }
        public static bool DrawFix(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, Vector2 origin, float rotation = 0f, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            if (!(texture is ScaledTexture2D))
                return true;

            if (skip)
            {
                skip = false;
                return true;
            }

            if (texture == null)
                return false;

            sourceRectangle = sourceRectangle.HasValue ? sourceRectangle.Value : new Rectangle(0, 0, texture.Width, texture.Height);


            if (sourceRectangle.HasValue && texture.Name != null && texture.Name != "" && repTextures.ContainsKey(texture.Name) && repTextures[texture.Name].Keys.FirstOrDefault(k => k.HasValue && k.Value.Contains(sourceRectangle.Value)) is Rectangle srr)
            {
                texture = repTextures[texture.Name][srr];
                sourceRectangle = new Rectangle(sourceRectangle.Value.X - srr.X, sourceRectangle.Value.Y - srr.Y, sourceRectangle.Value.Width, sourceRectangle.Value.Height);
            }

            if (texture is AnimatedTexture2D animTex)
                animTex.Tick();

            if (texture is ScaledTexture2D s && sourceRectangle.Value is Rectangle r)
            {
                if (s.AsOverlay)
                {
                    skip = true;
                    __instance.Draw(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, Math.Max(layerDepth - 0.00001f, 0f));
                }
                var newDestination = new Rectangle(destinationRectangle.X, destinationRectangle.Y, (int)(destinationRectangle.Width), (int)(destinationRectangle.Height));
                var newSR = new Rectangle?(new Rectangle((int)(r.X * s.Scale), (int)(r.Y * s.Scale), (int)(r.Width * s.Scale), (int)(r.Height * s.Scale)));
                var newOrigin = new Vector2(origin.X * s.Scale, origin.Y * s.Scale);

                if (s.ForcedSourceRectangle.HasValue)
                    newSR = s.ForcedSourceRectangle.Value;

                skip = true;
                __instance.Draw(s.STexture, newDestination, newSR, color, rotation, newOrigin, effects, layerDepth);
                return false;
            }

            return true;
        }

        public static bool Draw(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            return DrawFix(__instance, texture, destinationRectangle, sourceRectangle, color, origin, rotation, effects, layerDepth);
        }
        public static bool Draw(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
        {
            return DrawFix(__instance, texture, destinationRectangle, sourceRectangle, color, Vector2.Zero);
        }
        public static bool Draw(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            Rectangle sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            return DrawFix(__instance, texture, destinationRectangle, sourceRectangle, color, Vector2.Zero);
        }
        public static bool Draw(SpriteBatch __instance, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            sourceRectangle = sourceRectangle.HasValue ? sourceRectangle.Value : new Rectangle(0, 0, texture.Width, texture.Height);
            return DrawFix(__instance, texture, new Rectangle((int)(position.X), (int)(position.Y), (int)(sourceRectangle.Value.Width * scale.X), (int)(sourceRectangle.Value.Height * scale.Y)), sourceRectangle, color, origin, rotation, effects, layerDepth);
        }
        public static bool Draw(SpriteBatch __instance, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            sourceRectangle = sourceRectangle.HasValue ? sourceRectangle.Value : new Rectangle(0, 0, texture.Width, texture.Height);

            return DrawFix(__instance, texture, new Rectangle((int)(position.X), (int)(position.Y), (int)(sourceRectangle.Value.Width * scale), (int)(sourceRectangle.Value.Height * scale)), sourceRectangle, color, origin, rotation, effects, layerDepth);
        }
        public static bool Draw(SpriteBatch __instance, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
        {
            sourceRectangle = sourceRectangle.HasValue ? sourceRectangle.Value : new Rectangle(0, 0, texture.Width, texture.Height);
            return DrawFix(__instance, texture, new Rectangle((int)(position.X), (int)(position.Y), (int)(sourceRectangle.Value.Width), (int)(sourceRectangle.Value.Height)), sourceRectangle, color, Vector2.Zero);
        }
        public static bool Draw(SpriteBatch __instance, Texture2D texture, Vector2 position, Color color)
        {
            Rectangle sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            return DrawFix(__instance, texture, new Rectangle((int)(position.X), (int)(position.Y), (int)(texture.Width), (int)(texture.Height)), sourceRectangle, color, Vector2.Zero);

        }
    }
}