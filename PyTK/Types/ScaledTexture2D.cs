using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PyTK.Types
{

    public class ScaledTexture2D : Texture2D
    {
        public float Scale{ get; set; }
        public Texture2D STexture { get; set; }

        public ScaledTexture2D(GraphicsDevice graphicsDevice, int width, int height, Texture2D scaledTexture, float scale)
            :base(graphicsDevice, width,height)
        {
            Scale = scale;
            STexture = scaledTexture;
        }

        public static ScaledTexture2D FromTexture(Texture2D orgTexture, Texture2D scaledTexture, float scale)
        {
            Color[] data = new Color[orgTexture.Width * orgTexture.Height];
            orgTexture.GetData(data);
            ScaledTexture2D result = new ScaledTexture2D(orgTexture.GraphicsDevice, orgTexture.Width,orgTexture.Height,scaledTexture,scale);
            result.STexture = scaledTexture;
            result.Scale = scale;
            result.SetData(data);
            return result;
        }
    }
}
