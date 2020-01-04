using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Visualize
{


    internal class Effects : IVisualizeHandler
    {
        public static Dictionary<Texture2D, Texture2D> textureCache = new Dictionary<Texture2D, Texture2D>();
        public static Dictionary<Color, Color> colorCache = new Dictionary<Color, Color>();
        public static Dictionary<Color, Color> tintColorCache = new Dictionary<Color, Color>();
        public static Dictionary<Color, Color> shortColorCache = new Dictionary<Color, Color>();
        public static Dictionary<Color[], Color[]> lineCache = new Dictionary<Color[], Color[]>();
        public static Color[] lastData;
        public static Color[] lastNewData;
        public static bool running = false;
        public static Dictionary<Texture2D, List<Color>> paletteCache = new Dictionary<Texture2D, List<Color>>();
        internal static int[] whitecolor = new int[] { 255, 255, 255, 255 };
        internal static bool active = false;

        public Texture2D ProcessTexture(ref Texture2D texture)
        {
           
            Profile useProfile = VisualizeMod._activeProfile;

            texture = changeColor(texture, useProfile.saturation, useProfile.palette);

            return texture;
        }

        public bool Draw(ref SpriteBatch __instance, ref Texture2D texture, ref Vector4 destination, ref bool scaleDestination, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effects, ref float depth)
        {
            if (active || texture.Height == Game1.viewport.Height && texture.Width == Game1.viewport.Width)
            {
                active = false;
                return true;
            }
                
            Profile useProfile = VisualizeMod._activeProfile;
            MethodInfo drawMethod;

            if (Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, Microsoft.Xna.Framework.Graphics") != null)
                drawMethod = AccessTools.Method(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, Microsoft.Xna.Framework.Graphics"), "InternalDraw");
            else
                drawMethod = AccessTools.Method(Type.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch, MonoGame.Framework"), "DrawInternal");

            active = true;

            Color newColor = changeColor(color, useProfile.saturation, useProfile.palette);

            if (useProfile.tint != whitecolor)
            {
                if (tintColorCache.ContainsKey(newColor))
                    newColor = tintColorCache[newColor];
                else
                {
                    Color tintedColor = multiply(newColor, new Color(useProfile.tint[0], useProfile.tint[1], useProfile.tint[2], useProfile.tint[3]));
                    tintColorCache.Add(newColor, tintedColor);
                    newColor = tintedColor;
                }
            }

            drawMethod.Invoke(__instance, new object[] { changeColor(texture, useProfile.saturation, useProfile.palette), destination, scaleDestination, sourceRectangle, newColor, rotation, origin, effects, depth });

            return false;
        }

        public Color multiply(Color color1, Color color2)
        {
            Color result = new Color();

            result.R = (byte)MathHelper.Min(((color1.R * color2.R) / 255), 255);
            result.G = (byte)MathHelper.Min(((color1.G * color2.G) / 255), 255);
            result.B = (byte)MathHelper.Min(((color1.B * color2.B) / 255), 255);
            result.A = color1.A;

            return result;
        }

        private Texture2D changeColor(Texture2D texture, float saturation, string palletteFile)
        {
            if (textureCache.ContainsKey(texture))
                    return textureCache[texture];

            if (!running)
            {
                running = true;
                Task.Run(() =>
               {
                   Texture2D newTexture = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);

                   Color[] colorData;

                   colorData = new Color[texture.Width * texture.Height];
                   int width = texture.Width;
                   int height = texture.Height;
                   texture.GetData<Color>(colorData);
                   int length = colorData.Length;
                   Color[] lastColorData = (Color[])colorData.Clone();
                   Dictionary<Color, Color> newShortColorCache = new Dictionary<Color, Color>();

                   for (int x = 0; x < texture.Width; x++)
                       for (int y = 0; y < texture.Height; y++)
                       {
                           Color color = colorData[x * texture.Height + y];
                           if (shortColorCache.ContainsKey(color))
                               colorData[x * texture.Height + y] = shortColorCache[color];
                           else
                           {
                               Color newColor = changeColor(color, saturation, palletteFile);
                               shortColorCache.Add(color, newColor);
                               colorData[x * texture.Height + y] = newColor;
                           }
                       }

                   shortColorCache = newShortColorCache;
                   newTexture.SetData<Color>(colorData);

                   lastData = lastColorData;
                   lastNewData = colorData;

                   if (textureCache.ContainsKey(texture))
                       textureCache[texture] = newTexture;
                   else
                       textureCache.Add(texture, newTexture);

                   running = false;
               });
            }

            return texture;
        }

        private Color changeColor(Color color, float saturation, string palletteFile)
        {
            if (color.A == 0)
                return color;

            if (colorCache.ContainsKey(color))
                return colorCache[color];

            saturation *= VisualizeMod._config.saturation / 100;

            Color newColor = new Color(color.R, color.G, color.B, color.A);

            float newR = color.R;
            float newG = color.G;
            float newB = color.B;

            float l = 0.2125f * newR + 0.7154f * newG + 0.0721f * newB;

            float s = 1f - (saturation / 100);

            if (s != 0)
            {
                newR = newR + s * (l - newR);
                newG = newG + s * (l - newG);
                newB = newB + s * (l - newB);
            }

            newColor.R = (byte)MathHelper.Min(newR, 255);
            newColor.G = (byte)MathHelper.Min(newG, 255);
            newColor.B = (byte)MathHelper.Min(newB, 255);

            if (palletteFile != "none" && VisualizeMod.palette.Count > 0)
                newColor = FindNearestColor(VisualizeMod.palette.ToArray(), newColor);

            newColor.A = color.A;
            if (colorCache.ContainsKey(color))
                colorCache[color] = newColor;
            else
                colorCache.Add(color, newColor);

            return newColor;
        }

        internal List<Color> loadPalette(Texture2D texture)
        {
            Color[] colorData = new Color[texture.Width * texture.Height];
            List<Color> palette = new List<Color>();

            texture.GetData(colorData);

            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    if (!palette.Contains(colorData[x * texture.Height + y]))
                        palette.Add(colorData[x * texture.Height + y]);
                }
            }

            return palette;

        }

        private int GetDistance(Color current, Color match)
        {
            int redDifference;
            int greenDifference;
            int blueDifference;

            redDifference = current.R - match.R;
            greenDifference = current.G - match.G;
            blueDifference = current.B - match.B;

            return redDifference * redDifference + greenDifference * greenDifference + blueDifference * blueDifference;
        }

        private Color FindNearestColor(Color[] map, Color current)
        {
            int shortestDistance;
            int index;

            index = -1;
            shortestDistance = int.MaxValue;

            for (int i = 0; i < map.Length; i++)
            {
                Color match;
                int distance;

                match = map[i];
                distance = GetDistance(current, match);

                if (distance < shortestDistance)
                {
                    index = i;
                    shortestDistance = distance;
                }
            }

            return map[index];
        }
    }
}
