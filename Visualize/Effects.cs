using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;

namespace Visualize
{
    internal class Effects : IVisualizeHandler
    {
        public static Dictionary<Texture2D, Texture2D> textureCache = new Dictionary<Texture2D, Texture2D>();
        public static Dictionary<Color, Color> colorCache = new Dictionary<Color, Color>();
        public static Dictionary<Texture2D, List<Color>> paletteCache = new Dictionary<Texture2D, List<Color>>();
        internal static int[] whitecolor = new int[] { 255, 255, 255, 255 };

        public bool Draw(ref SpriteBatch __instance, ref Texture2D texture, ref Vector4 destination, ref bool scaleDestination, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effects, ref float depth)
        {
            Profile useProfile = VisualizeMod._activeProfile;
            
            if (useProfile.noAmbientLight)
                Game1.drawLighting = false;
            else if(Game1.currentLocation is GameLocation gl)
                Game1.drawLighting = gl.IsOutdoors && !Game1.outdoorLight.Equals(Color.White) || (!Game1.ambientLight.Equals(Color.White) || gl is MineShaft ms&& !ms.getLightingColor(Game1.currentGameTime).Equals(Color.White));
            
            if (useProfile.noLightsources && Game1.currentLocation is GameLocation l && (color == Color.PaleGoldenrod * (l.IsOutdoors ? 0.35f : 0.43f) || ((color == Color.White * 0.75f) && (texture == Game1.mouseCursors) && (sourceRectangle.GetValueOrDefault() == new Rectangle(88, 1779, 32, 32)))))
                return false;

            if (useProfile.noShadow && (texture == Game1.shadowTexture))
                return false;

            if (useProfile.noTransparancy && color != Color.White && color.R == color.G && color.G == color.B && color.B == color.A)
                color = Color.White;
            else
                color = changeColor(color, useProfile.light, useProfile.red, useProfile.green, useProfile.blue, useProfile.saturation, useProfile.palette);

            texture = changeColor(ref texture, useProfile.light, useProfile.red, useProfile.green, useProfile.blue, useProfile.saturation, useProfile.palette);

            if (useProfile.tint != whitecolor)
                color = multiply(color, new Color(useProfile.tint[0], useProfile.tint[1], useProfile.tint[2],useProfile.tint[3]));

            return true;
        }

        public bool Begin (ref SpriteBatch __instance, ref SpriteSortMode sortMode, ref BlendState blendState, ref SamplerState samplerState, ref DepthStencilState depthStencilState, ref RasterizerState rasterizerState, ref Effect effect, ref Matrix transformMatrix)
        {
            if (VisualizeMod.shader is Effect e)
                effect = e;

            return true;
        }

        public bool Begin(ref SpriteBatch __instance, ref SpriteSortMode sortMode, ref BlendState blendState, ref SamplerState samplerState, ref DepthStencilState depthStencilState, ref RasterizerState rasterizerState, ref Effect effect, ref Matrix? transformMatrix)
        {
            if (VisualizeMod.shader is Effect e)
                effect = e;

            return true;
        }



        private Texture2D changeColor(ref Texture2D texture, float light, float r, float g, float b, float saturation, string palletteFile)
        {
            if (textureCache.ContainsKey(texture))
                return textureCache[texture];

            if (texture.Width == Game1.viewport.Width && texture.Height == Game1.viewport.Height)
                return texture;

            VisualizeMod.pass++;

            if (VisualizeMod.pass >= VisualizeMod._config.passes)
               return texture;
                
            Texture2D newTexture = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);

            float adjust = 1f + (light / 100);

            Color[] colorData = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(colorData);

            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                    colorData[x * texture.Height + y] = changeColor(colorData[x * texture.Height + y], light, r, g, b, saturation, palletteFile);
            
            newTexture.SetData<Color>(colorData);
            textureCache.Add(texture, newTexture);

            return newTexture;
        }

        private Color changeColor(Color color, float light, float r, float g, float b, float saturation, string palletteFile)
        {
            if (color.A == 0)
                return color;

            saturation *= VisualizeMod._config.saturation / 100;

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

            if (palletteFile != "none" && VisualizeMod.palette.Count > 0)
                newColor = FindNearestColor(VisualizeMod.palette.ToArray(), newColor);

            newColor.A = color.A;

            colorCache.Add(color, newColor);

            return newColor;
        }

        private Color multiply(Color color1, Color color2)
        {
            Color result = new Color();

            result.R = (byte)MathHelper.Min(((color1.R * color2.R) / 255), 255);
            result.G = (byte)MathHelper.Min(((color1.G * color2.G) / 255), 255);
            result.B = (byte)MathHelper.Min(((color1.B * color2.B) / 255), 255);
            result.A = color1.A;

            return result;
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
