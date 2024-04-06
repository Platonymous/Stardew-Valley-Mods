using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Reflection;
using StardewValley;

namespace Portraiture2
{    public class HarmonyPatches
    {
        public static void PatchAll(ScaleUpMod mod)
        {
            var harmonyInstance = new Harmony("Platonymous.ScaleUp");
            foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(HarmonyPatches)).Where(m => m.Name.Contains("Draw")))
            {
                var types = method.GetParameters().Select(p => p.ParameterType.IsByRef ? p.ParameterType.GetElementType() : p.ParameterType).Where(t => !t.Name.Contains("SpriteBatch")).ToArray();
                if (AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.Draw), types) is MethodInfo target)
                    harmonyInstance.Patch(target, new HarmonyMethod(method), null, null);
            }
            harmonyInstance.Patch(AccessTools.Method(typeof(Game1), nameof(Game1.getSourceRectForStandardTileSheet)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(GetSourceRectForStandardTileSheet)));

            harmonyInstance.Patch(AccessTools.Method(typeof(Game1), nameof(Game1.getSquareSourceRectForNonStandardTileSheet)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(GetSquareSourceRectForNonStandardTileSheet)));

            harmonyInstance.Patch(AccessTools.Method(typeof(Game1), nameof(Game1.getArbitrarySourceRect)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(GetArbitrarySourceRect)));
            
        }
        public static void GetSquareSourceRectForNonStandardTileSheet(ref Texture2D tileSheet, int tilePosition, ref int tileWidth, ref int tileHeight)
        {
            GetBounds(ref tileWidth, ref tileHeight, tilePosition, ref tileSheet);
        }


        public static void GetArbitrarySourceRect(ref Texture2D tileSheet, int tilePosition, ref int tileWidth, ref int tileHeight)
        {
            GetBounds(ref tileWidth, ref tileHeight, tilePosition, ref tileSheet);
        }

        public static void GetSourceRectForStandardTileSheet(ref Texture2D tileSheet, int tilePosition, ref int width, ref int height)
        {
            GetBounds(ref width, ref height, tilePosition, ref tileSheet);
        }

        public static void GetBounds(ref int width, ref int height, int tilePosition, ref Texture2D tileSheet)
        {
            ScaleUpMod.Scales = ScaleUpMod.Singleton.Helper.GameContent.Load<Dictionary<string, ScaleUpData>>(ScaleUpMod.ScaleUpdDataAsset);

            if (tileSheet?.Name is string name && ScaleUpMod.Scales.Values.FirstOrDefault(s => s.Asset == name) is ScaleUpData data)
            {

                tileSheet = new Texture2D(Game1.graphics.GraphicsDevice, data.OrgWidth, data.OrgHeight);
                return;
            }
        }

        public static bool Draw(SpriteBatch __instance, Texture2D texture, ref Rectangle destinationRectangle,ref Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            if (ScaleUpMod.Scales.Values.FirstOrDefault(s => s.Asset == texture.Name) is ScaleUpData data)
            {
                __instance.Draw(texture, new Vector2(destinationRectangle.X, destinationRectangle.Y), sourceRectangle, color, rotation, origin, destinationRectangle.Width / (sourceRectangle.HasValue ? sourceRectangle.Value.Width : texture.Width), effects, layerDepth);
                return false;
            }

            return true;
        }

        public static bool Draw(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            if (ScaleUpMod.Scales.Values.FirstOrDefault(s => s.Asset == texture.Name) is ScaleUpData data)
            {
                __instance.Draw(texture, new Vector2(destinationRectangle.X, destinationRectangle.Y), null, color, 0f, Vector2.Zero, destinationRectangle.Width / texture.Width, SpriteEffects.None, 0f);
                return false;
            }

            return true;
        }

        public static bool Draw(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
        {

            if (ScaleUpMod.Scales.Values.FirstOrDefault(s => s.Asset == texture.Name) is ScaleUpData data)
            {
                __instance.Draw(texture, new Vector2(destinationRectangle.X, destinationRectangle.Y),sourceRectangle, color, 0f, Vector2.Zero, destinationRectangle.Width / (sourceRectangle.HasValue ? sourceRectangle.Value.Width : texture.Width), SpriteEffects.None, 0f);
                return false;
            }

            return true;
        }

        public static bool Draw(SpriteBatch __instance, Texture2D texture, Vector2 position, Color color)
        {
            if (ScaleUpMod.Scales.Values.FirstOrDefault(s => s.Asset == texture.Name) is ScaleUpData data)
            {
                __instance.Draw(texture, position, null, color, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
                return false;
            }

            return true;
        }

        public static bool Draw(SpriteBatch __instance, Texture2D texture,  Vector2 position, Rectangle? sourceRectangle, Color color)
        {
            if (ScaleUpMod.Scales.Values.FirstOrDefault(s => s.Asset == texture.Name) is ScaleUpData data)
            {
                __instance.Draw(texture, position, sourceRectangle, color, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None,0f);
                return false;
            }

            return true;
        }

        public static bool Draw(SpriteBatch __instance, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, ref Vector2 origin, ref float scale, SpriteEffects effects, float layerDepth)
        {
            if (ScaleUpMod.Scales.Values.FirstOrDefault(s => s.Asset == texture.Name) is ScaleUpData data)
            {
                __instance.Draw(texture, position, sourceRectangle, color, rotation, origin, new Vector2(scale,scale), effects, layerDepth);
                return false;
            }

            return true;
        }

        public static void Draw(SpriteBatch __instance, Texture2D texture, ref Vector2 position, ref Rectangle? sourceRectangle, Color color, float rotation, ref Vector2 origin, ref Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            if (ScaleUpMod.Scales.Values.FirstOrDefault(s => s.Asset == texture.Name) is ScaleUpData data)
            {
                var ow = sourceRectangle.HasValue ? sourceRectangle.Value.Width : data.OrgWidth;
                var oh = sourceRectangle.HasValue ? sourceRectangle.Value.Height : data.OrgHeight;

                var dataScale = data.Scale;
                float percW = (sourceRectangle.HasValue ? (float)sourceRectangle.Value.Width : data.OrgWidth) / data.OrgWidth;
                float percH = (sourceRectangle.HasValue ? (float)sourceRectangle.Value.Height : data.OrgHeight) / data.OrgHeight;
                var originalHeight = (sourceRectangle.HasValue ? sourceRectangle.Value.Height : data.OrgHeight);
                var originalWidth = (sourceRectangle.HasValue ? sourceRectangle.Value.Width : data.OrgWidth);
                sourceRectangle = data.GetScaledSource(sourceRectangle, (int)ow,(int)oh, out int padx, out int pady, true);
                
                scale.X = scale.X / dataScale;
                scale.Y = scale.Y / dataScale;

                origin = origin * data.Scale;

                if (data.Padded)
                {
                    origin.X += padx / 2;
                    origin.Y += pady;
                }
            }
        }

    }

}
