using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Visualize
{

    public interface IVisualizeHandler
    {
      
        Texture2D ProcessTexture(Texture2D texture);

        Color ProcessColor(Color color);

        bool Draw(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, Vector2 origin, float rotation, SpriteEffects effects, float layerDepth);

        bool Draw(SpriteBatch __instance, SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2? origin, float scale, SpriteEffects effects, float layerDepth);


    }
}
