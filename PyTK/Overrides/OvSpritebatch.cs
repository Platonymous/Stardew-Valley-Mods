using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using StardewModdingAPI;
using SObject = StardewValley.Object;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using StardewValley;
using PyTK.Types;

namespace PyTK.Overrides
{
    class OvSpritebatch
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;
        internal static bool replaceNext = false;
        internal static CustomObjectData nextData;
        internal static Dictionary<object, CustomObjectData> dataChache = new Dictionary<object, CustomObjectData>();

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

            static bool skip = false;

            internal static bool Prefix(ref SpriteBatch __instance, ref Texture2D texture, ref Vector4 destinationRectangle, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effect, ref float depth, ref bool autoFlush)
            {
                if (!skip && texture is ScaledTexture2D s && sourceRectangle.HasValue && sourceRectangle.Value is Rectangle r)
                {
                    var newDestination = new Vector4(destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Z / s.Scale, destinationRectangle.W / s.Scale);
                    var newSR = new Rectangle?(new Rectangle((int)(r.X * s.Scale), (int)(r.Y * s.Scale), (int)(r.Width * s.Scale), (int)(r.Height * s.Scale)));

                    skip = true;
                    MethodInfo drawMethod = AccessTools.Method(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, MonoGame.Framework"), "DrawInternal");
                    drawMethod.Invoke(__instance, new object[] { s.STexture, newDestination, newSR, color, rotation, origin, effect, depth, autoFlush });
                    skip = false;
                    return false;
                }

                if (!replaceNext)
                    return true;

                if (sourceRectangle.HasValue && sourceRectangle == nextData.sdvSourceRectangle)
                {
                    replaceNext = false;

                    if (nextData.texture == null)
                        return false;

                    MethodInfo drawMethod = AccessTools.Method(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, MonoGame.Framework"), "DrawInternal");
                    drawMethod.Invoke(__instance, new object[] { nextData.texture, destinationRectangle, nextData.sourceRectangle, nextData.color != Color.White ? nextData.color : color, rotation, origin, effect, depth, autoFlush });
                    return false;
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

            static bool skip = false;

            internal static bool Prefix(ref SpriteBatch __instance, ref Texture2D texture, ref Vector4 destination, ref bool scaleDestination, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effects, ref float depth)
            {
                if (!skip && texture is ScaledTexture2D s && sourceRectangle.HasValue && sourceRectangle.Value is Rectangle r)
                {
                    var newDestination = new Vector4(destination.X, destination.Y, destination.Z / s.Scale, destination.W / s.Scale);
                    var newSR = new Rectangle?(new Rectangle((int) (r.X * s.Scale), (int)(r.Y * s.Scale), (int)(r.Width * s.Scale), (int)(r.Height * s.Scale)));
                    skip = true;
                    MethodInfo drawMethod = AccessTools.Method(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, Microsoft.Xna.Framework.Graphics"), "InternalDraw");
                    drawMethod.Invoke(__instance, new object[] { s.STexture, newDestination, scaleDestination, newSR, color, rotation, origin, effects, depth });
                    skip = false;
                    return false;
                }
                
                if (!replaceNext)
                    return true;

                if (sourceRectangle.HasValue && sourceRectangle == nextData.sdvSourceRectangle)
                {
                    replaceNext = false;

                    if (nextData.texture == null)
                        return false;

                    MethodInfo drawMethod = AccessTools.Method(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, Microsoft.Xna.Framework.Graphics"), "InternalDraw");
                    drawMethod.Invoke(__instance, new object[] { nextData.texture, destination, scaleDestination, nextData.sourceRectangle, nextData.color != Color.White ? nextData.color : color, rotation, origin, effects, depth });
                    return false;
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
        
        internal class DrawFix1
        {
            public static void prefix(ref SObject __instance)
            {
                CustomObjectData c = null;

                if (dataChache.ContainsKey(__instance))
                    c = dataChache[__instance];
                else if (!SaveHandler.hasSaveType(__instance) || __instance is IDrawFromCustomObjectData)
                {
                    SObject obj = __instance;
                    c = __instance is IDrawFromCustomObjectData draw ? draw.data : CustomObjectData.collection.Find(o => o.Value.sdvId == obj.ParentSheetIndex && o.Value.bigCraftable == obj.bigCraftable.Value).Value;
                    dataChache.AddOrReplace(__instance, c);
                }
                else
                    dataChache.AddOrReplace(__instance, c);

                if (c != null)
                {
                    replaceNext = true;
                    nextData = c;
                }
                else
                    replaceNext = false;
            }

            public static void postfix(ref SObject __instance)
            {
                    replaceNext = false;
            }
        }

        internal class DrawFix2
        {
            public static void prefix(ref TemporaryAnimatedSprite __instance, string ___textureName)
            {
                CustomObjectData c = null;

                if (dataChache.ContainsKey(__instance))
                    c = dataChache[__instance];
                else if (___textureName == "Maps\\springobjects")
                {
                    TemporaryAnimatedSprite obj = __instance;
                    c = __instance is IDrawFromCustomObjectData draw ? draw.data : CustomObjectData.collection.Find(o => o.Value.sdvSourceRectangle == obj.sourceRect && o.Value.bigCraftable == false).Value;
                    dataChache.AddOrReplace(__instance, c);
                }
                else
                    dataChache.AddOrReplace(__instance, c);

                if (c != null)
                {
                    replaceNext = true;
                    nextData = c;
                }
                else
                    replaceNext = false;
            }

            public static void postfix(ref SObject __instance)
            {
                replaceNext = false;
            }
        }

    }
}
