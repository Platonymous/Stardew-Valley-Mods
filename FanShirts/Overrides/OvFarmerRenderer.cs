using Microsoft.Xna.Framework;
using PyTK.Types;
using StardewValley;

namespace FanShirts.Overrides
{
    internal class OvFarmerRenderer
    {
        public static bool addedFields = false;

        public static void Prefix_drawHairAndAccesories(Farmer who)
        {
            if (!FanShirtsMod.worldIsReady || !FanShirtsMod.playerJerseys.ContainsKey(who.UniqueMultiplayerID.ToString()) || !FanShirtsMod.playerBaseJerseys.ContainsKey(who.UniqueMultiplayerID.ToString()))
            {
                FarmerRenderer.shirtsTexture = FanShirtsMod.vanillaShirts;
                return;
            }

            FarmerRenderer.shirtsTexture = FanShirtsMod.playerJerseys[who.UniqueMultiplayerID.ToString()];

            int id = FanShirtsMod.playerBaseJerseys[who.UniqueMultiplayerID.ToString()];

            if (who.shirt.Value != id)
                who.changeShirt(id);

            if (FarmerRenderer.shirtsTexture is ScaledTexture2D st && st.DestinationPositionAdjustment == Vector2.Zero)
            {
                Rectangle sr = Game1.getSourceRectForStandardTileSheet(FanShirtsMod.vanillaShirts, id, 8, 32);
                st.DestinationPositionAdjustment = new Vector2(0, -96);
                st.SourcePositionAdjustment = new Vector2(-(sr.X * 4), -(sr.Y * 4));
            }

            if(FarmerRenderer.shirtsTexture is ScaledTexture2D stex && Game1.activeClickableMenu != null)
                stex.DestinationPositionAdjustment = Vector2.Zero;
        }

        public static void Postfix_drawHairAndAccesories()
        {
            if (FanShirtsMod.worldIsReady)
                FarmerRenderer.shirtsTexture = FanShirtsMod.vanillaShirts;
        }

        public static void NetFieldFix(Farm __instance)
        {
                __instance.NetFields.AddFields(FanShirtsMod.playerBaseJerseys, FanShirtsMod.playerJerseys);
        }
    }
}
