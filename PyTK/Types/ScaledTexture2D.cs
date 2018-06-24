using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PyTK.Types
{

    public class ScaledTexture2D : Texture2D
    {
        public float Scale{ get; set; }
        public Texture2D STexture { get; set; }
        public Rectangle? ForcedSourceRectangle { get; set; } = null;

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
