using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;

namespace Visualize
{
    public class Effects
    {
        public static Dictionary<Texture2D, Texture2D> textureCache = new Dictionary<Texture2D, Texture2D>();
        public static Dictionary<Color, Color> colorCache = new Dictionary<Color, Color>();
        public static Dictionary<Texture2D, List<Color>> paletteCache = new Dictionary<Texture2D, List<Color>>();
        public static BlendState lightingBlend = new BlendState() { ColorBlendFunction = BlendFunction.ReverseSubtract, ColorDestinationBlend = Blend.One, ColorSourceBlend = Blend.SourceColor };

        public static Texture2D processTexture(Texture2D texture)
        {
            return changeColor(ref texture, VisualizeMod._activeProfile.light, VisualizeMod._activeProfile.red, VisualizeMod._activeProfile.green, VisualizeMod._activeProfile.blue, VisualizeMod._activeProfile.saturation);
        }

        public static bool appyEffects(ref SpriteBatch spritebatch, ref Color color, ref Texture2D texture)
        {
            if (VisualizeMod._activeProfile.noShadow && (texture == Game1.shadowTexture))
                return false;

            if (VisualizeMod._activeProfile.noTransparancy && color != Color.White && color.R == color.G && color.G == color.B && color.B == color.A)
                color = Color.White;
            else
                color = changeColor(ref color, VisualizeMod._activeProfile.light, VisualizeMod._activeProfile.red, VisualizeMod._activeProfile.green, VisualizeMod._activeProfile.blue, VisualizeMod._activeProfile.saturation);

            texture = changeColor(ref texture, VisualizeMod._activeProfile.light, VisualizeMod._activeProfile.red, VisualizeMod._activeProfile.green, VisualizeMod._activeProfile.blue, VisualizeMod._activeProfile.saturation);

            if (VisualizeMod._activeProfile.tint != Color.White)
                color = multiply(color, VisualizeMod._activeProfile.tint);

            return true;
        }

        public static Texture2D changeColor(ref Texture2D texture, float light, float r, float g, float b, float saturation)
        {
            if (textureCache.ContainsKey(texture))
                return textureCache[texture];

            Color[] colorData = new Color[texture.Width * texture.Height];

            try
            {
                texture.GetData(colorData);
            }
            catch
            {
                textureCache.Add(texture, texture);
                return texture;
            }

            float adjust = 1f + (light / 100);

            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    colorData[x * texture.Height + y] = changeColor(ref colorData[x * texture.Height + y], light, r, g, b, saturation);
                }
            }

            Texture2D newTexture = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);
            newTexture.SetData(colorData);

            textureCache.Add(texture, newTexture);

            return newTexture;
        }

        public static Color changeColor(ref Color color, float light, float r, float g, float b, float saturation)
        {
            if (color.A == 0)
                return color;

            if (VisualizeMod._activeProfile.noColorTransparancy && color.A < 255)
                color.A = 255;
                
            if (colorCache.ContainsKey(color))
                return colorCache[color];

            float adjust = light / 100;

            Color newColor = new Color(color.R, color.G, color.B, color.A);
            
            float newR = (adjust * color.R);
            float newG = (adjust * color.G);
            float newB = (adjust * color.B);

            float l = 0.2125f * newR + 0.7154f * newG + 0.0721f * newB;

            float rs = 1f - (r / 100);
            float gs = 1f - (g / 100);
            float bs = 1f - (b / 100);

            newR += rs * (l - newR);
            newG += gs * (l - newG);
            newB += bs * (l - newB);

            float s = 1f - (saturation / 100);
            
            if(s != 0)
            {
                newR = newR + s * (l - newR);
                newG = newG + s * (l - newG);
                newB = newB + s * (l - newB);
            }

            newColor.R = (byte)MathHelper.Min(newR, 255);
            newColor.G = (byte)MathHelper.Min(newG, 255);
            newColor.B = (byte)MathHelper.Min(newB, 255);

            if (VisualizeMod._activeProfile.palette != "none" && VisualizeMod.palette.Count > 0)
                newColor = FindNearestColor(VisualizeMod.palette.ToArray(), newColor);

            newColor.A = color.A;

            colorCache.Add(color, newColor);

            return newColor;
        }

        public static Color multiply(Color color1, Color color2)
        {
            Color result = new Color();

            result.R = (byte)MathHelper.Min(((color1.R * color2.R) / 255), 255);
            result.G = (byte)MathHelper.Min(((color1.G * color2.G) / 255), 255);
            result.B = (byte)MathHelper.Min(((color1.B * color2.B) / 255), 255);
            result.A = color1.A;

            return result;
        }

        public static List<Color> loadPalette(Texture2D texture)
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

        public static int GetDistance(Color current, Color match)
        {
            int redDifference;
            int greenDifference;
            int blueDifference;

            redDifference = current.R - match.R;
            greenDifference = current.G - match.G;
            blueDifference = current.B - match.B;

            return redDifference * redDifference + greenDifference * greenDifference + blueDifference * blueDifference;
        }

        public static Color FindNearestColor(Color[] map, Color current)
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
