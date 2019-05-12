using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Types;
using System;
using System.IO;

namespace PyTK.APIs
{
    public class PyTKAPI : IScalerAPI
    {
        public Texture2D CreateScaledTexture2D(Texture2D orgTexture, Texture2D scaledTexture, float scale = -1, Rectangle? forcedSourceRectangle = null)
        {
            if(scale == -1)
                scale = (float)(Convert.ToDouble(scaledTexture.Width) / Convert.ToDouble(orgTexture.Width));

            return ScaledTexture2D.FromTexture(orgTexture, scaledTexture, scale, forcedSourceRectangle); ;
        }

        public Texture2D CreateScaledTexture2D(Rectangle orgSize, Texture2D scaledTexture, float scale = -1, Rectangle? forcedSourceRectangle = null)
        {
            if (scale == -1)
                scale = (float)(Convert.ToDouble(scaledTexture.Width) / Convert.ToDouble(orgSize.Width));

            return ScaledTexture2D.FromTexture(PyUtils.getRectangle(orgSize.Width,orgSize.Height,Color.White), scaledTexture, scale, forcedSourceRectangle); ;
        }

        public Texture2D CreateScaledTexture2D(Texture2D scaledTexture, float scale, Rectangle? forcedSourceRectangle = null)
        {
            return ScaledTexture2D.FromTexture(PyUtils.getRectangle((int)((float)scaledTexture.Width / scale), (int)((float)scaledTexture.Height/scale), Color.White), scaledTexture, scale, forcedSourceRectangle); ;
        }
        
        public bool IsScaledTexture2D(Texture2D texture)
        {
            return (texture is ScaledTexture2D);
        }

        public Texture2D SetForcedSourceRectangle(Texture2D texture, Rectangle? forcedSourceRectangle = null)
        {
            if (texture is ScaledTexture2D s)
            {
                s.ForcedSourceRectangle = forcedSourceRectangle;
                return s;
            }

            return texture;
        }

        public Texture2D SetScale(Texture2D scaledTexture, float scale)
        {
            if (scaledTexture is ScaledTexture2D s)
            {
                s.Scale = scale;
                return s;
            }

            return CreateScaledTexture2D(scaledTexture, scale);
        }

        public float GetScale(Texture2D scaledTexture)
        {
            if (scaledTexture is ScaledTexture2D s)
                return s.Scale;

            return 1;
        }

        public Texture2D SetScaledTexture(Texture2D scaledTexture, Texture2D newTexture)
        {
            if (scaledTexture is ScaledTexture2D s)
            {
                s.STexture = newTexture;
                return s;
            }

            return newTexture;
        }

        public Texture2D GetScaledTexture(Texture2D scaledTexture)
        {
            if (scaledTexture is ScaledTexture2D s)
                return s.STexture;

            return scaledTexture;
        }

        public Texture2D ScaleUpTexture(Texture2D texture, float scale, bool asScaledTexture2D = true, Rectangle? forcedSourceRectangle = null)
        {
            Color[] orgColor = new Color[texture.Width * texture.Height];
            Color[] scaledColor = new Color[(int)(orgColor.Length * scale)];
            int sWidth = (int)(texture.Width * scale);
            int sHeight = (int)(texture.Height * scale);
            Texture2D sTexture;
            using (MemoryStream mem = new MemoryStream())
            {
                texture.SaveAsPng(mem, sWidth, sHeight);
                sTexture = Texture2D.FromStream(texture.GraphicsDevice, mem);
            }

            if (!asScaledTexture2D)
                return sTexture;

            return CreateScaledTexture2D(texture, sTexture, scale, forcedSourceRectangle);
        }
    }
}
