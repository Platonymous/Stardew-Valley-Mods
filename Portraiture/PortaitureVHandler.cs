using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Visualize;
using StardewValley;
using StardewValley.Menus;

namespace Portraiture
{
    class PortaitureVHandler : IVisualizeHandler
    {
        public bool Begin(ref SpriteBatch __instance, ref SpriteSortMode sortMode, ref BlendState blendState, ref SamplerState samplerState, ref DepthStencilState depthStencilState, ref RasterizerState rasterizerState, ref Effect effect, ref Matrix transformMatrix)
        {
            return true;
        }

        public bool Draw(ref SpriteBatch __instance, ref Texture2D texture, ref Vector4 destination, ref bool scaleDestination, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effects, ref float depth)
        {

            if (TextureLoader.activeFolder <= 0 || !(Game1.activeClickableMenu is ShopMenu || Game1.activeClickableMenu is DialogueBox) || texture.Width != 128)
                return true;

            NPC npc = getNPCForTexture(texture);

            if (npc == null)
                return true;
            Texture2D newTexture = TextureLoader.getPortrait(npc.name);

            if (newTexture == null)
                return true;

            texture = newTexture;
            sourceRectangle = new Rectangle?(TextureLoader.getSoureRectangle(texture));
            destination.W = 64 * Game1.pixelZoom;
            destination.Z = 64 * Game1.pixelZoom;
            scaleDestination = false;

            PortraitureMod.activeTexure = texture;
            return true;
        }

        public NPC getNPCForTexture(Texture2D texture)
        {
            foreach (NPC npc in Utility.getAllCharacters())
                if (npc.Portrait == texture)
                    return npc;

            return null;
        }
    }
}
