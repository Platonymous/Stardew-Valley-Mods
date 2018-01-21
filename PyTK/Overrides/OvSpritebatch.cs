using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PyTK.Overrides
{
    class OvSpritebatch
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        
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

            internal static void Prefix(ref SpriteBatch __instance, KeyValuePair<Texture2D, Rectangle?> __state, ref Texture2D texture, ref Vector4 destinationRectangle, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effect, ref float depth)
            {
                __state = new KeyValuePair<Texture2D, Rectangle?>(texture, sourceRectangle);

                if (!sourceRectangle.HasValue || texture == null)
                    return;

                Rectangle sr = sourceRectangle.Value;

                if (!CustomObjectData.collection.Exists(a => a.Value.sdvSourceRectangle == sr))
                    return;

                CustomObjectData data = CustomObjectData.collection.Find(a => a.Value.sdvSourceRectangle == sr).Value;

                if (data.color != Color.White)
                    color = data.color.multiplyWith(color);

                texture = data.texture;
                sourceRectangle = data.sourceRectangle;
            }

            internal static void Postfix(ref Texture2D texture, ref Rectangle? sourceRectangle, KeyValuePair<Texture2D, Rectangle?> __state)
            {
                texture = __state.Key;
                sourceRectangle = __state.Value;
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

            internal static void Prefix(ref SpriteBatch __instance, KeyValuePair<Texture2D,Rectangle?> __state, ref Texture2D texture, ref Vector4 destination, ref bool scaleDestination, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effects, ref float depth)
            {
                __state = new KeyValuePair<Texture2D, Rectangle?>(texture, sourceRectangle);

                if (!sourceRectangle.HasValue || texture == null)
                    return;

                Rectangle sr = sourceRectangle.Value;

                if (!CustomObjectData.collection.Exists(a => a.Value.sdvSourceRectangle == sr))
                    return;

                CustomObjectData data = CustomObjectData.collection.Find(a => a.Value.sdvSourceRectangle == sr).Value;

                if (data.color != Color.White)
                    color = data.color.multiplyWith(color);

                texture = data.texture;
                sourceRectangle = data.sourceRectangle;
            }

            internal static void Postfix(ref Texture2D texture, ref Rectangle? sourceRectangle, KeyValuePair<Texture2D, Rectangle?> __state)
            {
                if (__state.Key == null || !__state.Value.HasValue)
                    return;

                texture = __state.Key;
                sourceRectangle = __state.Value;
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
        
    }
}
