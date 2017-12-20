using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Visualize;
using StardewValley;
using StardewValley.Menus;

namespace Capitalism
{
    class VisualizeHandler : IVisualizeHandler
    {
        public bool Begin(ref SpriteBatch __instance, ref SpriteSortMode sortMode, ref BlendState blendState, ref SamplerState samplerState, ref DepthStencilState depthStencilState, ref RasterizerState rasterizerState, ref Effect effect, ref Matrix transformMatrix)
        {
            return true;
        }

        public bool Draw(ref SpriteBatch __instance, ref Texture2D texture, ref Vector4 destination, ref bool scaleDestination, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effects, ref float depth)
        {
            if (texture == Game1.mouseCursors && sourceRectangle.Value is Rectangle r && r.X == 286 && CapitalismMod.counter >= 0)
            {
                int index = CapitalismMod.counter;

                int d = 7;
                if (Game1.activeClickableMenu is ShippingMenu)
                    d = 6;

                string money = "12.3";
                int digits = money.Length;
                int num = -1;
                if (money[index] != '.')
                    int.TryParse(money[index].ToString(), out num);

                if ((num == 0 && !CapitalismMod.showZero))
                {
                    CapitalismMod.counter++;
                    return false;
                }

                else
                    CapitalismMod.showZero = true;

                r.Y = num < 0 ? 0 : 502 - num * 8;
                r.X = num < 0 ? 0 : r.X;

                texture = num < 0 ? CapitalismMod.pointTex : texture;

                sourceRectangle = new Rectangle?(r);
                CapitalismMod.counter++;
            }

            return true;
        }
    }
}
