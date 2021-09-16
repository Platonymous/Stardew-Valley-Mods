using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Reflection;
using StardewValley;
using PyTK.Types;
using StardewValley.Menus;

namespace PyTK.Overrides
{
    class OvSpritebatch
    {/*
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;
        internal static Dictionary<object, CustomObjectData> dataChache = new Dictionary<object, CustomObjectData>();
        internal static MethodInfo drawMethodMono = AccessTools.Method(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, MonoGame.Framework"), "DrawInternal");
        internal static MethodInfo drawMethod = AccessTools.Method(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, Microsoft.Xna.Framework.Graphics"), "InternalDraw");
        internal static Dictionary<Rectangle, CustomObjectData> recCache = new Dictionary<Rectangle, CustomObjectData>();

        internal static CustomObjectData getDataFromSourceRectangle(Rectangle source)
        {
            CustomObjectData data = null;
            if (recCache.ContainsKey(source))
                return recCache[source];

            data = CustomObjectData.collection.Find(o => o.Value.sdvSourceRectangle == source) is KeyValuePair<string, CustomObjectData> d ? d.Value : null;
            recCache.Add(source, data);

            return data;
        }

        [HarmonyPatch]
        internal class SpriteBatchFixMono
        {
            internal static MethodInfo TargetMethod()
            {
                if (Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, MonoGame.Framework") != null)
                {
                    if (AccessTools.Method(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, MonoGame.Framework"), "DrawInternal") is MethodInfo mi)
                        return mi;
                    else
                        return AccessTools.Method(typeof(FakeSpriteBatch), "DrawInternal");
                }
                else
                    return AccessTools.Method(typeof(FakeSpriteBatch), "DrawInternal");
            }

            static bool skip1 = false;
            static bool skip2 = false;

            internal static bool Prefix(ref SpriteBatch __instance, ref Texture2D texture, ref Vector4 destinationRectangle, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effect, ref float depth, ref bool autoFlush)
            {
                if (skip1 || !sourceRectangle.HasValue || Game1.activeClickableMenu is TitleMenu)
                    return true;

                if (texture is MappedTexture2D mapped && mapped.Get(sourceRectangle.Value.X, sourceRectangle.Value.Y) is Texture2D t)
                {
                    drawMethodMono.Invoke(__instance, new object[] { t, destinationRectangle, new Rectangle(0, 0, t.Width, t.Height), color, rotation, origin, effect, depth, autoFlush });
                    return false;
                }

                if (texture is ScaledTexture2D s && sourceRectangle != null && sourceRectangle.Value is Rectangle r)
                {
                    var newDestination = new Vector4(destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Z, destinationRectangle.W);
                    var newSR = new Rectangle?(new Rectangle((int)(r.X * s.Scale), (int)(r.Y * s.Scale), (int)(r.Width * s.Scale), (int)(r.Height * s.Scale)));
                    var newOrigin = new Vector2(origin.X * s.Scale, origin.Y * s.Scale);

                    if (s.ForcedSourceRectangle.HasValue)
                        newSR = s.ForcedSourceRectangle.Value;

                    skip1 = true;
                    drawMethodMono.Invoke(__instance, new object[] { s.STexture, newDestination, newSR, color, rotation, newOrigin, effect, depth, autoFlush });
                    skip1 = false;
                    return false;
                }
                else if (!skip2 && texture.Tag is String tag)
                {
                    CustomObjectData data = tag != "cod_objects" && CustomObjectData.collection.ContainsKey(tag) ? CustomObjectData.collection[tag] : getDataFromSourceRectangle(sourceRectangle.Value);

                    texture.Tag = "cod_object";
                    Game1.bigCraftableSpriteSheet.Tag = "cod_object";
                    Game1.objectSpriteSheet.Tag = "cod_object";

                    if (data != null)
                    {
                        skip2 = true;
                        drawMethodMono.Invoke(__instance, new object[] { data.texture, destinationRectangle, data.sourceRectangle, data.color != Color.White ? data.color : color, rotation, origin, effect, depth, autoFlush });
                        skip2 = false;
                        return false;
                    }
                }
               
                return true;
            } 
        }


        [HarmonyPatch]
        internal class SpriteBatchFix
        {
            internal static MethodInfo TargetMethod()
            {
                if (Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, Microsoft.Xna.Framework.Graphics") != null)
                    return AccessTools.Method(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, Microsoft.Xna.Framework.Graphics"), "InternalDraw");
                else
                    return AccessTools.Method(typeof(FakeSpriteBatch), "InternalDraw");
            }

            static bool skip1 = false;
            static bool skip2 = false;

            internal static bool Prefix(ref SpriteBatch __instance, ref Texture2D texture, ref Vector4 destination, ref bool scaleDestination, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effects, ref float depth)
            {
                if (skip1 || !sourceRectangle.HasValue || Game1.activeClickableMenu is TitleMenu)
                    return true;

                if (texture is MappedTexture2D mapped && mapped.Get(sourceRectangle.Value.X, sourceRectangle.Value.Y) is Texture2D t)
                {
                    drawMethod.Invoke(__instance, new object[] { t, destination, scaleDestination, new Rectangle(0, 0, t.Width, t.Height), color, rotation, origin, effects, depth });
                    return false;
                }

                if (texture is ScaledTexture2D s && sourceRectangle != null && sourceRectangle.Value is Rectangle r)
                {
                    var newDestination = new Vector4(destination.X, destination.Y, destination.Z / s.Scale, destination.W / s.Scale);
                    var newSR = new Rectangle?(new Rectangle((int) (r.X * s.Scale), (int)(r.Y * s.Scale), (int)(r.Width * s.Scale), (int)(r.Height * s.Scale)));
                    var newOrigin = new Vector2(origin.X * s.Scale, origin.Y * s.Scale);

                    if (s.ForcedSourceRectangle.HasValue)
                        newSR = s.ForcedSourceRectangle.Value;

                    skip1 = true;
                    drawMethod.Invoke(__instance, new object[] { s.STexture, newDestination, scaleDestination, newSR, color, rotation, newOrigin, effects, depth });
                    skip1 = false;
                    return false;
                }else if (!skip2 && texture.Tag is String tag)
                {
                    CustomObjectData data = CustomObjectData.collection.ContainsKey(tag) ? CustomObjectData.collection[tag] : getDataFromSourceRectangle(sourceRectangle.Value);
                    texture.Tag = "cod_object";
                    Game1.bigCraftableSpriteSheet.Tag = "cod_object";
                    Game1.objectSpriteSheet.Tag = "cod_object";

                    if (data != null)
                    {
                        skip2 = true;
                        drawMethod.Invoke(__instance, new object[] { data.texture, destination, scaleDestination, data.sourceRectangle, data.color != Color.White ? data.color : color, rotation, origin, effects, depth });
                        skip2 = false;
                        return false;
                    }
                }
              
                return true;
            }



        }

        internal class FakeSpriteBatch
        {
            internal void DrawInternal(Texture2D texture, Vector4 destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effect, float depth, bool autoFlush)
            {
                return;
            }

            internal void InternalDraw(Texture2D texture, ref Vector4 destination, bool scaleDestination, ref Rectangle? sourceRectangle, Color color, float rotation, ref Vector2 origin, SpriteEffects effects, float depth)
            {
                return;
            }
        }
*/
    }
}
