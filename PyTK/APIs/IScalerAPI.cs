using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace PyTK.APIs
{
    public interface IScalerAPI
    {
        Texture2D CreateScaledTexture2D(Texture2D orgTexture, Texture2D scaledTexture, float scale = -1, Rectangle? forcedSourceRectangle = null);

        Texture2D CreateScaledTexture2D(Rectangle orgSize, Texture2D scaledTexture, float scale = -1, Rectangle? forcedSourceRectangle = null);

        Texture2D CreateScaledTexture2D(int orgWidth, Texture2D scaledTexture, Rectangle? forcedSourceRectangle = null);

        Texture2D CreateScaledTexture2D(Texture2D scaledTexture, float scale, Rectangle? forcedSourceRectangle = null);

        bool IsScaledTexture2D(Texture2D texture);

        Texture2D SetForcedSourceRectangle(Texture2D texture, Rectangle? forcedSourceRectangle = null);

        Texture2D SetScale(Texture2D scaledTexture, float scale);

        float GetScale(Texture2D scaledTexture);

        Texture2D SetScaledTexture(Texture2D scaledTexture, Texture2D newTexture);

        Texture2D GetScaledTexture(Texture2D scaledTexture);

        Texture2D ScaleUpTexture(Texture2D texture, float scale, bool asScaledTexture2D = true, Rectangle? forcedSourceRectangle = null);

        void ReplaceAssetAt(string assetName, Rectangle sourceRectangle, Texture2D texture);


    }
}
