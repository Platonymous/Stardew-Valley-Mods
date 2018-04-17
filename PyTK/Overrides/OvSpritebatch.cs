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

namespace PyTK.Overrides
{
    class OvSpritebatch
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;
        internal static bool replaceNext = false;
        internal static Item nextItem;
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

            internal static bool Prefix(ref SpriteBatch __instance, ref Texture2D texture, ref Vector4 destinationRectangle, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effect, ref float depth)
            {
                if (!replaceNext )
                    return true;

                if (sourceRectangle.HasValue && sourceRectangle == nextData.sdvSourceRectangle && texture == nextData.sdvTexture)
                {
                    replaceNext = false;

                    if (nextData.texture == null)
                        return false;

                    MethodInfo drawMethod = AccessTools.Method(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, MonoGame.Framework"), "DrawInternal");
                    drawMethod.Invoke(__instance, new object[] { nextData.texture, destinationRectangle, true, nextData.sourceRectangle, nextData.color != Color.White ? nextData.color : color, rotation, origin, effect, depth });
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

            internal static bool Prefix(ref SpriteBatch __instance, ref Texture2D texture, ref Vector4 destination, ref bool scaleDestination, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effects, ref float depth)
            {
                if (!replaceNext)
                    return true;
                
                if (sourceRectangle.HasValue && sourceRectangle == nextData.sdvSourceRectangle && texture == nextData.sdvTexture)
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
                    c = __instance is IDrawFromCustomObjectData draw ? draw.data : CustomObjectData.collection.Find(o => o.Value.sdvId == obj.parentSheetIndex && o.Value.bigCraftable == obj.bigCraftable).Value;
                    dataChache.AddOrReplace(__instance, c);
                }
                else
                    dataChache.AddOrReplace(__instance, c);

                if (c != null)
                {
                    replaceNext = true;
                    nextItem = __instance;
                    nextData = c;
                }
                else
                    replaceNext = false;
            }

            public static void postfix(ref SObject __instance)
            {
                    replaceNext = false;
            }

            public static void init(string name, Type type, List<string> toPatch)
            {
                HarmonyInstance harmony = HarmonyInstance.Create("Platonymous.PyTK.Draw." + name);
                List<MethodInfo> replacer = typeof(DrawFix1).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList();
                MethodInfo prefix = typeof(DrawFix1).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "prefix");
                MethodInfo postfix = typeof(DrawFix1).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "postfix");
                List<MethodInfo> originals = type.GetMethods().ToList();

                foreach(MethodInfo method in originals)
                    if(toPatch.Contains(method.Name))
                        harmony.Patch(method, new HarmonyMethod(prefix), new HarmonyMethod(postfix));                        
            }

            

        }

    }
}
