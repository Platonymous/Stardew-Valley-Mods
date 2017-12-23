using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Visualize;

namespace Capitalism
{
    class MainVisualizeHandler : IVisualizeHandler
    {
        public bool Draw(ref SpriteBatch __instance, ref Texture2D texture, ref Vector4 destination, ref bool scaleDestination, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effects, ref float depth)
        {
            foreach(IVisualizeHandler handler in CapitalismMod.vHandlers)
            if (!handler.Draw(ref __instance, ref texture, ref destination, ref scaleDestination, ref sourceRectangle, ref color, ref rotation, ref origin, ref effects, ref depth))
                return false;

            return true;
        }
    }
}
