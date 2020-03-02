using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Extensions;

namespace PyTK.Types
{

    public class ScaledTexture2D : Texture2D
    {
        public float Scale{ get; set; }
        public virtual Texture2D STexture { get; set; }
        public Rectangle? ForcedSourceRectangle { get; set; } = null;

        public bool AsOverlay { get; set; } = false;

        public ScaledTexture2D(GraphicsDevice graphicsDevice, int width, int height)
            : base(graphicsDevice, width, height)
        {
            Scale = 1;
            STexture = this;
        }

        public ScaledTexture2D(Texture2D tex, bool vessel = false, float scale = 1)
            : base(tex.GraphicsDevice, tex.Width, tex.Height)
        {
            if (vessel)
            {
                Color[] data = new Color[tex.Width * tex.Height];
                tex.GetData(data);
                SetData(data);
            }
            Scale = scale;
            STexture = tex;
        }

        public ScaledTexture2D(Texture2D tex, float scale = 1)
            : base(tex.GraphicsDevice, (int)(tex.Width / scale), (int)(tex.Height / scale))
        {
            Color[] data = new Color[(int)(tex.Width / scale) * (int)(tex.Height / scale)];
            PyUtils.getRectangle((int)(tex.Width / scale), (int)(tex.Height / scale), Color.White).GetData(data);
            SetData(data);

            Scale = scale;
            STexture = tex;
        }

        public ScaledTexture2D(Texture2D tex, int width, int height, float scale = 1)
            : base(tex.GraphicsDevice, (int)(width / scale), (int)(height / scale))
        {
            Color[] data = new Color[(int)(width / scale) * (int)(height / scale)];
            PyUtils.getRectangle((int)(width / scale), (int)(height / scale), Color.White).GetData(data);
            SetData(data);

            Scale = scale;
            STexture = tex;
        }

        public ScaledTexture2D(GraphicsDevice graphicsDevice, int width, int height, Texture2D scaledTexture, float scale, Rectangle? forcedSourceRectangle = null)
            :base(graphicsDevice, width,height)
        {
            Scale = scale;
            STexture = scaledTexture;
            ForcedSourceRectangle = forcedSourceRectangle;
        }

        public static ScaledTexture2D FromTexture(Texture2D orgTexture, Texture2D scaledTexture, float scale, Rectangle? forcedSourceRectangle = null)
        {
            Color[] data = new Color[orgTexture.Width * orgTexture.Height];
            orgTexture.GetData(data);
            ScaledTexture2D result = new ScaledTexture2D(orgTexture.GraphicsDevice, orgTexture.Width,orgTexture.Height,scaledTexture,scale,forcedSourceRectangle);
            result.SetData(data);
            return result;
        }
    }
}
