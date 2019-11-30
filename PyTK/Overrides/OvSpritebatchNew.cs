using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using PyTK.Types;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PyTK.Overrides
{
    class OvSpritebatchNew
    {
        internal static Dictionary<Rectangle, CustomObjectData> recCache = new Dictionary<Rectangle, CustomObjectData>();


        internal static bool skip = false;

        internal static void initializePatch(Harmony.HarmonyInstance instance)
        {
            foreach (MethodInfo method in typeof(OvSpritebatchNew).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => m.Name == "Draw"))
                instance.Patch(typeof(SpriteBatch).GetMethod("Draw", method.GetParameters().Select(p => p.ParameterType).Where(t => !t.Name.Contains("SpriteBatch")).ToArray()), new Harmony.HarmonyMethod(method), null, null);
        }

        internal static CustomObjectData getDataFromSourceRectangle(Rectangle source)
        {
            CustomObjectData data = null;
            if (recCache.ContainsKey(source))
                return recCache[source];

            data = CustomObjectData.collection.Find(o => o.Value.sdvSourceRectangle == source) is KeyValuePair<string, CustomObjectData> d ? d.Value : null;
            recCache.Add(source, data);

            return data;
        }

        public static bool DrawFix(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, Vector2 origin, float rotation = 0f, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            if (skip)
            {
                skip = false;
                return true;
            }

            sourceRectangle = sourceRectangle.HasValue ? sourceRectangle.Value : new Rectangle(0, 0, texture.Width, texture.Height);

            if (texture is MappedTexture2D mapped && mapped.Get(sourceRectangle.Value.X, sourceRectangle.Value.Y) is Texture2D t)
            {
                skip = true;
                __instance.Draw(t, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);
                return false;
            }

            if (texture is ScaledTexture2D s && sourceRectangle.Value is Rectangle r)
            {
                var newDestination = new Rectangle(destinationRectangle.X, destinationRectangle.Y, (int)(destinationRectangle.Width), (int)(destinationRectangle.Height));
                var newSR = new Rectangle?(new Rectangle((int)(r.X * s.Scale), (int)(r.Y * s.Scale), (int)(r.Width * s.Scale), (int)(r.Height * s.Scale)));
                var newOrigin = new Vector2(origin.X * s.Scale, origin.Y * s.Scale);

                if (s.ForcedSourceRectangle.HasValue)
                    newSR = s.ForcedSourceRectangle.Value;

                skip = true;
                __instance.Draw(s.STexture, newDestination, newSR, color, rotation, newOrigin, effects, layerDepth);
                return false;
            }

            if (texture != null && texture.Tag != null && texture.Tag is String tag)
            {
                CustomObjectData data = CustomObjectData.collection.ContainsKey(tag) ? CustomObjectData.collection[tag] : getDataFromSourceRectangle(sourceRectangle.Value);
                texture.Tag = "cod_object";
                Game1.bigCraftableSpriteSheet.Tag = "cod_object";
                Game1.objectSpriteSheet.Tag = "cod_object";

                if (data != null && data.texture != null)
                {
                    skip = true;
                    __instance.Draw(data.texture, destinationRectangle, data.sourceRectangle, data.color, rotation, origin, effects, layerDepth);
                    return false;
                }
            }

            return true;
        }

        public static bool Draw(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            return DrawFix(__instance, texture, destinationRectangle, sourceRectangle, color,origin, rotation, effects, layerDepth);
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
