using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using PyTK.Types;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Reflection;

namespace CustomShirts.Overrides
{
    internal class OvFarmerRenderer
    {
        public static bool addedFields = false;

        internal static bool menuIsCC()
        {
            return Game1.activeClickableMenu is IClickableMenu c && c.GetType().Name.ToLower().Contains("character");
        }

        public static void Prefix_drawHairAndAccesories(FarmerRenderer __instance, Farmer who, int facingDirection, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor)
        {
            bool savedShirt = CustomShirtsMod.playerShirts.ContainsKey(who.UniqueMultiplayerID) && CustomShirtsMod.playerBaseShirts.ContainsKey(who.UniqueMultiplayerID) && CustomShirtsMod.playerBaseShirts[who.UniqueMultiplayerID] != -9999;

            if (savedShirt && (Game1.activeClickableMenu is CharacterCustomization || menuIsCC()))
            {
                savedShirt = false;
                CustomShirtsMod.playerShirts.Remove(who.UniqueMultiplayerID);
                CustomShirtsMod.playerBaseShirts.Remove(who.UniqueMultiplayerID);
                who.shirt.Value = ((CustomShirtsMod.shirts.FindIndex(fj => fj.fullid == CustomShirtsMod.config.ShirtId) + 1) * -1);
            }

            if (who.shirt.Value >= 0 && !savedShirt)
            {
                FarmerRenderer.shirtsTexture = CustomShirtsMod.vanillaShirts;
                return;
            }
            else
            {
                try
                {
                    if (!savedShirt)
                        FarmerRenderer.shirtsTexture = CustomShirtsMod.shirts[(who.shirt.Value * -1) - 1].texture2d;
                    else
                        FarmerRenderer.shirtsTexture = CustomShirtsMod.playerShirts[who.UniqueMultiplayerID];
                }
                catch { }
            }

            if (FarmerRenderer.shirtsTexture is ScaledTexture2D st)
            {
                if (st.Scale > 1)
                    st.DestinationPositionAdjustment = new Vector2(0, -(80 + st.Scale / 2 * 8));
                else
                    st.DestinationPositionAdjustment = Vector2.Zero;
                st.ForcedSourceRectangle = new Rectangle?(new Rectangle(0, (int)((facingDirection == 0 ? 24 : facingDirection == 1 ? 8 : facingDirection == 3 ? 16 : 0) * st.Scale), (int)(8 * st.Scale), (int)(8 * st.Scale)));
            }

            if (FarmerRenderer.shirtsTexture is ScaledTexture2D stex && (Game1.activeClickableMenu is GameMenu || Game1.activeClickableMenu is TitleMenu || Game1.activeClickableMenu is CharacterCustomization || menuIsCC()))
                stex.DestinationPositionAdjustment = Vector2.Zero;
        }

        public static void Postfix_drawHairAndAccesories()
        {
            FarmerRenderer.shirtsTexture = CustomShirtsMod.vanillaShirts;
        }

        public static bool Prefix_changeShirt(Farmer __instance, int whichShirt)
        {
            if (Game1.activeClickableMenu is TitleMenu)
            {
                IClickableMenu sub = (IClickableMenu)typeof(TitleMenu).GetField("_subMenu", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                string submenu = sub == null ? "null" : sub.GetType().ToString().ToLower();

                if (!submenu.Contains("character"))
                    return true;
            }

            int max = CustomShirtsMod.vanillaShirts.Height / 32 * (CustomShirtsMod.vanillaShirts.Width / 8) - 1;
            if (whichShirt >= 0 && whichShirt <= max)
                return true;

            if (whichShirt > 0)
                whichShirt = -1 * ((CustomShirtsMod.shirts.Count + (max - whichShirt)) + 1);

            if (whichShirt * -1 > CustomShirtsMod.shirts.Count)
                return true;

            __instance.shirt.Set(whichShirt);
            __instance.FarmerRenderer.changeShirt(whichShirt);

            CustomShirtsMod.recolor = true;

            return false;
        }

        public static void recolorShirt()
        {
            Shirt jersey = CustomShirtsMod.shirts[(Game1.player.shirt.Value * -1) - 1];
            Texture2D baseTexture = CustomShirtsMod._helper.Reflection.GetField<Texture2D>(Game1.player.FarmerRenderer, "baseTexture").GetValue();
            if (baseTexture != null)
            {
                int id = jersey.baseid - 1;
                Color[] data = new Color[CustomShirtsMod.vanillaShirts.Bounds.Width * CustomShirtsMod.vanillaShirts.Bounds.Height];
                CustomShirtsMod.vanillaShirts.GetData<Color>(data);
                int index = id * 8 / CustomShirtsMod.vanillaShirts.Bounds.Width * 32 * 128 + id * 8 % CustomShirtsMod.vanillaShirts.Bounds.Width + CustomShirtsMod.vanillaShirts.Width * 4;
                Color color1 = data[index];
                swapColor(baseTexture, 256, (int)color1.R, (int)color1.G, (int)color1.B);
                Color color2 = data[index - CustomShirtsMod.vanillaShirts.Width];
                swapColor(baseTexture, 257, (int)color2.R, (int)color2.G, (int)color2.B);
                Color color3 = data[index - CustomShirtsMod.vanillaShirts.Width * 2];
                swapColor(baseTexture, 258, (int)color3.R, (int)color3.G, (int)color3.B);
            }
        }

        public static Texture2D swapColor(Texture2D texture, int targetColorIndex, int red, int green, int blue)
        {
            return swapColor(texture, targetColorIndex, red, green, blue, 0, texture.Width * texture.Height);
        }

        public static Texture2D swapColor(Texture2D texture, int targetColorIndex, int red, int green, int blue, int startPixel, int endPixel)
        {
            red = Math.Min(Math.Max(1, red), (int)byte.MaxValue);
            green = Math.Min(Math.Max(1, green), (int)byte.MaxValue);
            blue = Math.Min(Math.Max(1, blue), (int)byte.MaxValue);
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(data);
            Color color = data[targetColorIndex];
            for (int index = 0; index < data.Length; ++index)
            {
                if (index >= startPixel && index < endPixel && ((int)data[index].R == (int)color.R && (int)data[index].G == (int)color.G) && (int)data[index].B == (int)color.B)
                    data[index] = new Color(red, green, blue);
            }
            texture.SetData<Color>(data);
            return texture;
        }

    }
}
