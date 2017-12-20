using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Visualize
{
    public interface IVisualizeHandler
    {
        bool Draw(ref SpriteBatch __instance, ref Texture2D texture, ref Vector4 destination, ref bool scaleDestination, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effects, ref float depth);
        bool Begin(ref SpriteBatch __instance, ref SpriteSortMode sortMode, ref BlendState blendState, ref SamplerState samplerState, ref DepthStencilState depthStencilState, ref RasterizerState rasterizerState, ref Effect effect, ref Matrix transformMatrix);
    }
}
