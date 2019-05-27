using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Extensions;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyTK
{
    public class PyDraw
    {
        internal static GraphicsDevice Device => Game1.graphics.GraphicsDevice;

        public static Texture2D getPattern(int width, int height, params Texture2D[] textures)
        {
            List<Color[]> textureColors = new List<Color[]>();
            int tWidth = textures[0].Width;
            int tHeight = textures[0].Height;

            foreach (Texture2D texture in textures)
            {
                int iWidth = texture.Width;
                int iHeight = texture.Height;
                Color[] colors = new Color[iWidth * iHeight];
                texture.GetData(colors);
                textureColors.Add(colors);
            }

            return getRectangle(width, height, (i, w, h) =>
            {
                int x = i % w;
                int y = (i - x) / w;
                int t = 0;
                int x2 = x;
                int y2 = y;

                if (x >= tWidth)
                {
                    t = (int)Math.Floor(x / tWidth * 1f);
                    x2 = x2 % tWidth;
                }

                if (y >= tHeight)
                {
                    t += (int)Math.Floor(y / tHeight * 1f);
                    y2 = y2 % tHeight;
                }

                if (t >= textureColors.Count)
                    t = t % textureColors.Count;

                return textureColors[t][y2 * tWidth + x2];
            });
        }

        public static Texture2D getCircle(int diameter, Color color, bool ensureOddDiameter = true)
        {
            return getCircle(diameter, color, Color.Transparent, ensureOddDiameter);
        }

        public static Texture2D getCircle(int diameter, Color color, Color color2, bool ensureOddDiameter = true)
        {
            diameter += ensureOddDiameter ? (diameter + 1) % 2 : diameter;
            int radius = (int)Math.Floor(diameter / 2f);
            Rectangle r = new Rectangle(0, 0, diameter, diameter);
            Point c = r.Center;
            int sDist = radius * radius;

            return getRectangle(diameter, diameter, (i, w, h) =>
            {
                int x = i % w;
                int y = (i - x) / w;
                Point p = new Point(x, y);

                if (p.GetSquaredDistance(c) > sDist)
                    return color2;
                else
                    return color;
            });
        }

        public static Texture2D getRectangle(int width, int height, Color color)
        {
            return getRectangle(width, height, (i, w, h) => color);
        }

        public static Texture2D getRectangle(int width, int height, Func<int, int, int, Color> colorPicker)
        {
            Texture2D rect = new Texture2D(Device, width, height);

            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i) data[i] = colorPicker(i, width, height);
            rect.SetData(data);
            return rect;
        }

        public static Texture2D getWhitePixel()
        {
            return getRectangle(1, 1, Color.White);
        }

        public static Rectangle? getSourceRectangle(Texture2D texture, int tileWidth, int tileHeight, int tileIndex)
        {
            return new Rectangle(tileIndex * tileWidth % texture.Width, tileIndex * tileWidth / texture.Width * tileHeight, tileWidth, tileHeight);
        }
    }
}
