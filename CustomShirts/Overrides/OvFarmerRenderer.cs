using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using PyTK;
using PyTK.ContentSync;
using PyTK.CustomElementHandler;
using PyTK.Types;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace CustomShirts.Overrides
{
    internal class OvFarmerRenderer
    {
        public static bool addedFields = false;
        public static List<string> requestedHatSyncs = new List<string>();

        internal static bool menuIsCC()
        {
            return Game1.activeClickableMenu is IClickableMenu c && c.GetType().Name.ToLower().Contains("character");
        }

        public static void Prefix_drawHairAndAccesories(FarmerRenderer __instance, Farmer who, int facingDirection, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor)
        {
            if (Game1.activeClickableMenu is TitleMenu && who.hat.Value is Hat h && !(h is CustomHat) && SaveHandler.getAdditionalSaveData(h) is Dictionary<string, string> savdata)
                if (savdata.ContainsKey("blueprint") && savdata["blueprint"] is string bid && CustomShirtsMod.hats.Find(bp => bp.fullid == bid) is HatBlueprint hbp)
                    who.hat.Value = new CustomHat(hbp);

            if (who.hat.Value is CustomHat c)
            {
                if (c.texture == null && CustomShirtsMod.syncedHats.ContainsKey(c.hatId + "." + who.UniqueMultiplayerID))
                    c.texture = CustomShirtsMod.syncedHats[c.hatId + "." + who.UniqueMultiplayerID];

                if (c.texture == null && !requestedHatSyncs.Contains(c.hatId + "." + who.UniqueMultiplayerID))
                {
                    requestedHatSyncs.Add(c.hatId + "." + who.UniqueMultiplayerID);
                    try
                    {
                    Task.Run(async () =>
                    {
                       await PyNet.sendRequestToFarmer<HatSync>(CustomShirtsMod.HatSyncerRequestName, c.hatId, who, (hs) =>
                       {
                           requestedHatSyncs.Remove(c.hatId + "." + who.UniqueMultiplayerID);

                           if (hs == null || hs.Texture == null)
                               return;

                           if (CustomShirtsMod.syncedHats.ContainsKey(hs.SyncId))
                               CustomShirtsMod.syncedHats.Remove(hs.SyncId);

                           CustomShirtsMod.syncedHats.Add(hs.SyncId, hs.Texture.getTexture());
                       }, SerializationType.PLAIN, 1000);
                   });
                    }
                    catch (Exception e)
                    {
                        CustomShirtsMod._monitor.Log(e.Message + ":" + e.StackTrace);
                    }
                }

                if (c.texture != null)
                {
                    int direction = who.FacingDirection;
                    FarmerRenderer.hatsTexture = c.texture;
                    if (direction == 0)
                        direction = 3;
                    else if (direction == 2)
                        direction = 0;
                    else if (direction == 3)
                        direction = 2;

                    if (Game1.activeClickableMenu is TitleMenu || Game1.activeClickableMenu is GameMenu)
                        direction = 0;

                    if (c.texture is ScaledTexture2D sct)
                        sct.ForcedSourceRectangle = new Rectangle(0, (int)(direction * 20 * sct.Scale), (int)(20 * sct.Scale), (int)(20 * sct.Scale));
                }
            }
            else
                FarmerRenderer.hatsTexture = CustomShirtsMod.vanillaHats;

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
                    if (!savedShirt && who == Game1.player)
                        FarmerRenderer.shirtsTexture = CustomShirtsMod.shirts[(who.shirt.Value * -1) - 1].texture2d;
                    else
                    {
                        if (CustomShirtsMod.playerShirts[who.UniqueMultiplayerID] == null)
                            return;
                        FarmerRenderer.shirtsTexture = CustomShirtsMod.playerShirts[who.UniqueMultiplayerID];
                    }
                }
                catch { }
            }
            
            if (FarmerRenderer.shirtsTexture is ScaledTexture2D st)
                st.ForcedSourceRectangle = new Rectangle?(new Rectangle(0, (int)((facingDirection == 0 ? 24 : facingDirection == 1 ? 8 : facingDirection == 3 ? 16 : 0) * st.Scale), (int)(8 * st.Scale), (int)(8 * st.Scale)));
        }

        public static bool Prefix_kiskae()
        {
            return false;
        }

        public static bool Prefix_doChangeShirt(FarmerRenderer __instance, int whichShirt)
        {
            if(whichShirt < 0 && ((whichShirt * -1) - 1) > 0 && ((whichShirt * -1) - 1) < CustomShirtsMod.shirts.Count)
            {
                int id = CustomShirtsMod.shirts[(whichShirt * -1) - 1].baseid - 1;
                CustomShirtsMod._helper.Reflection.GetMethod(__instance, "doChangeShirt").Invoke(new object[] { null, null, id });
                return false;
            }

            return true;
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

        public static void recolorShirt(long mid)
        {
            if (!CustomShirtsMod.playerBaseShirts.ContainsKey(mid))
                return;
            Texture2D baseTexture = CustomShirtsMod._helper.Reflection.GetField<Texture2D>(Game1.player.FarmerRenderer, "baseTexture").GetValue();

            if (baseTexture != null)
            {
                Color color1 = Color.White;
                Color color2 = Color.White;
                Color color3 = Color.White;

                if (!CustomShirtsMod.playerBaseColors.ContainsKey(mid))
                {
                    int id = CustomShirtsMod.playerBaseShirts[mid] - 1;
                    if (id < 0)
                    {
                        if (Game1.otherFarmers.ContainsKey(mid))
                            id = Game1.otherFarmers[mid].shirt.Value;
                        else
                            id = 19;

                    }
                    Color[] data = new Color[CustomShirtsMod.vanillaShirts.Bounds.Width * CustomShirtsMod.vanillaShirts.Bounds.Height];
                    CustomShirtsMod.vanillaShirts.GetData<Color>(data);
                    int index = id * 8 / CustomShirtsMod.vanillaShirts.Bounds.Width * 32 * 128 + id * 8 % CustomShirtsMod.vanillaShirts.Bounds.Width + CustomShirtsMod.vanillaShirts.Width * 4;
                    color1 = data[index];
                    color2 = data[index - CustomShirtsMod.vanillaShirts.Width];
                    color3 = data[index - CustomShirtsMod.vanillaShirts.Width * 2];

                    CustomShirtsMod.playerBaseColors.Add(mid, new Color[3] { color1, color2, color3 });
                }
                else
                {
                    Color[] colors = CustomShirtsMod.playerBaseColors[mid];
                    color1 = colors[0];
                    color2 = colors[1];
                    color3 = colors[2];
                }
                swapColor(baseTexture, 256, (int)color1.R, (int)color1.G, (int)color1.B);
                swapColor(baseTexture, 257, (int)color2.R, (int)color2.G, (int)color2.B);
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
