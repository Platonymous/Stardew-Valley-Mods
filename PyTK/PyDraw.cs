using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Extensions;
using StardewValley;
using System;
using System.Collections.Generic;

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

            return getRectangle(width, height, (x,y, w, h) =>
            {
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

        public static Texture2D getPatterns(int width, int height, params Texture2D[] textures)
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

            int tpw = width / tWidth;
            int tph = height / tHeight;

            int n = tpw * tph;

            List<Color[]> placements = new List<Color[]>();
            int j = 0;
            for (int i = 0; i < n; i++) {
                placements.Add(textureColors[j]);
                j++;
                if (j >= textureColors.Count)
                    j = 0;
            }
            return getRectangle(width, height, (x, y, w, h) =>
            {
                int t = (x / tWidth) + ((y / tHeight) * (tpw));

                int xt = x % tWidth;
                int yt = y % tHeight;

                return placements[t][yt * tWidth + xt];
            });
        }

        public static Texture2D getBorderedRectangle(int width, int height, Color color, int border, Color borderColor)
        {
            return getRectangle(width, height, (x, y, w, h) =>
            {
                Point p = new Point(x, y);

                if (x < border || y < border || x >= width - border || y >= height - border)
                    return borderColor;
                else
                    return color;
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

            return getRectangle(diameter, diameter, (x, y , w, h) =>
            {
                Point p = new Point(x, y);

                if (p.GetSquaredDistance(c) > sDist)
                    return color2;
                else
                    return color;
            });
        }

        public static Texture2D getFade(int width, int height, Color color1, Color color2, bool horizontal = true)
        {
            return getRectangle(width, height, (x,y, w, h) =>
            {
                float nx = (float)(x + 1) / width;
                float ny = (float)(y + 1) / height;
                return Color.Lerp(color1,color2, horizontal ? nx : ny);
            });
        }

        public static Texture2D getRadialFade(int diameter, Color backColor, Color color1, Color color2, bool ensureOddDiameter = true)
        {
            diameter += ensureOddDiameter ? (diameter + 1) % 2 : diameter;
            int radius = (int)Math.Floor(diameter / 2f);
            Rectangle r = new Rectangle(0, 0, diameter, diameter);
            Point c = r.Center;
            int sDist = radius * radius;

            return getRectangle(diameter, diameter, (x, y, w, h) =>
            {
                Point p = new Point(x, y);
                float d = p.GetSquaredDistance(c);
                if (d > sDist)
                    return backColor;
                else
                    return Color.Lerp(color1, color2, d / sDist);

            });
        }

        public static Texture2D getMasked(Texture2D image, Texture2D mask, bool inverted = false)
        {
            Color[] imageData = new Color[image.Width * image.Height];
            Color[] maskData = new Color[mask.Width * mask.Height];

            image.GetData(imageData);
            mask.GetData(maskData);

            PyTKMod._monitor.Log(image.Width + "x" + image.Height + ":" + mask.Width + "x" + mask.Height);
            return getRectangle(image.Width, image.Height, (i, w, h) =>
            {
                if (maskData.Length <= i)
                    return (inverted ? imageData[i] : Color.Transparent);
                else
                    return imageData[i] * (!inverted ? ((float) maskData[i].A / 255f) : ((float)(255f - maskData[i].A) / 255f));
            });
        }
            public static Texture2D getRectangle(int width, int height, Color color)
        {
            return getRectangle(width, height, (i, w, h) => color);
        }

        public static Texture2D getRectangle(int width, int height, Func<int, int, int, int, Color> colorPicker)
        {
            Texture2D rect = new Texture2D(Device, width, height);

            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i)
            {
                int x = i % width;
                int y = (i - x) / width;
                data[i] = colorPicker(x,y, width, height);
            }
            rect.SetData(data);
            return rect;
        }

        public static Texture2D getRectangle(int width, int height, Func<int, int, int, Color> colorPicker)
        {
            Texture2D rect = new Texture2D(Device, width, height);

            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i)
                data[i] = colorPicker(i, width, height);
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
