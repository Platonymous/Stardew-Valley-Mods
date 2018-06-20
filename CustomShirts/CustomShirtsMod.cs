using CustomShirts.Overrides;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PyTK;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CustomShirts
{
    public class CustomShirtsMod : Mod
    {
        internal static Config config;

        internal static Dictionary<long, Texture2D> playerShirts = new Dictionary<long, Texture2D>();
        internal static Dictionary<long, int> playerBaseShirts = new Dictionary<long, int>();
        internal static Dictionary<long, Color[]> playerBaseColors = new Dictionary<long, Color[]>();
        internal static List<ShirtPack> packs = new List<ShirtPack>();
        internal static List<Shirt> shirts = new List<Shirt>();
        internal static Texture2D vanillaShirts;
        internal static bool worldIsReady => Context.IsWorldReady;
        internal static string ShirtSyncerName = "Platonymous.CustomShirts.Sync";
        internal static string ShirtReloaderName = "Platonymous.CustomShirts.Reload";
        internal static IPyResponder ShirtSyncer;
        internal static IPyResponder ShirtReloader;

        internal static IMonitor _monitor;
        internal static IModHelper _helper;

        internal static bool recolor = false;

        public override void Entry(IModHelper helper)
        {
            _helper = helper;
            vanillaShirts = Helper.Content.Load<Texture2D>(@"Characters/Farmer/shirts", ContentSource.GameContent);
            ShirtSyncer = new PyResponder<bool, ShirtSync>(ShirtSyncerName, (s) =>
              {
                  try
                  {
                      if (playerShirts.ContainsKey(s.FarmerId))
                          playerShirts.Remove(s.FarmerId);

                      if (playerBaseShirts.ContainsKey(s.FarmerId))
                          playerBaseShirts.Remove(s.FarmerId);

                      if (playerBaseColors.ContainsKey(s.FarmerId))
                          playerBaseColors.Remove(s.FarmerId);

                      try
                      {
                          playerShirts.Add(s.FarmerId, (ScaledTexture2D)s.Texture.getTexture());
                          playerBaseShirts.Add(s.FarmerId, s.BaseShirt);

                          if (s.BaseColors is int[][] colors && colors.Length == 3)
                          {
                              Color[] baseColors = new Color[3] { new Color(colors[0][0], colors[0][1], colors[0][2]), new Color(colors[1][0], colors[1][1], colors[1][2]), new Color(colors[2][0], colors[2][1], colors[2][2]) };
                              playerBaseColors.Add(s.FarmerId, baseColors);
                          }
                      }
                      catch
                      {

                      }
                  }
                  catch (Exception e)
                  {
                      Monitor.Log(e.Message + e.StackTrace, LogLevel.Alert);
                  }
                  return true;
              }, 60, SerializationType.PLAIN, SerializationType.JSON);

            ShirtReloader = new PyResponder<long, long>(ShirtReloaderName, (s) =>
            {
                loadShirt();
                return Game1.player.UniqueMultiplayerID;
            }, 60);

            ShirtSyncer.start();
            ShirtReloader.start();

            _monitor = Monitor;
            config = helper.ReadConfig<Config>();

            HarmonyInstance harmony = HarmonyInstance.Create("Platonymous.CustomShirts");
            harmony.Patch(typeof(FarmerRenderer).GetMethod("drawHairAndAccesories"), new HarmonyMethod(typeof(Overrides.OvFarmerRenderer).GetMethod("Prefix_drawHairAndAccesories")), new HarmonyMethod(typeof(Overrides.OvFarmerRenderer).GetMethod("Postfix_drawHairAndAccesories")));
            harmony.Patch(typeof(Farmer).GetMethod("changeShirt"), new HarmonyMethod(typeof(Overrides.OvFarmerRenderer).GetMethod("Prefix_changeShirt")), null);
            harmony.Patch(typeof(FarmerRenderer).GetMethod("doChangeShirt", BindingFlags.NonPublic | BindingFlags.Instance), new HarmonyMethod(typeof(Overrides.OvFarmerRenderer).GetMethod("Prefix_doChangeShirt")), null);
            loadContentPacks();

            if(config.SwitchKey != Keys.Escape)
                config.SwitchKey.onPressed(() => Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.Wizard));

            GameEvents.QuarterSecondTick += (s, e) =>
            {
                if (recolor)
                {
                    OvFarmerRenderer.recolorShirt(Game1.player.UniqueMultiplayerID);
                    recolor = false;
                }
            };

            SaveEvents.AfterLoad += (s, e) =>
            {
                setShirt();
                PyUtils.setDelayedAction(500, () =>
                 {
                     OvFarmerRenderer.Prefix_doChangeShirt(Game1.player.FarmerRenderer, Game1.player.shirt.Value);
                     OvFarmerRenderer.recolorShirt(Game1.player.UniqueMultiplayerID);
                     foreach (Farmer f in Game1.otherFarmers.Values)
                         OvFarmerRenderer.Prefix_doChangeShirt(f.FarmerRenderer, f.shirt.Value);
                 });
                PyNet.sendRequestToAllFarmers<long>(ShirtReloaderName, Game1.player.UniqueMultiplayerID, null);
            };

            MenuEvents.MenuClosed += (s, e) =>
            {
                if (e.PriorMenu is CharacterCustomization || e.PriorMenu.GetType().Name.ToLower().Contains("character"))
                    setShirt(true);
            };

            foreach (SavedShirt sj in config.SavedShirts)
                loadShirt(sj.Id, false);
        }

        private void loadContentPacks()
        {
            foreach(StardewModdingAPI.IContentPack pack in Helper.GetContentPacks())
            {
                ShirtPack shirtPack = pack.ReadJsonFile<ShirtPack>("content.json");

                int c = 0;
                foreach (Shirt shirt in shirtPack.shirts)
                {
                    c++;

                    if (shirt.id == "none")
                        shirt.id = "shirt" + c;

                    Texture2D texture = pack.LoadAsset<Texture2D>(shirt.texture);

                    if (texture.Width > (8 * shirt.scale))
                        texture = texture.getTile(shirt.tileindex, (int)(8 * shirt.scale), (int) (32 * shirt.scale));

                    shirt.fullid = pack.Manifest.UniqueID + "." + shirt.id;
                    shirt.texture2d = ScaledTexture2D.FromTexture(vanillaShirts, texture, shirt.scale);
                    shirts.Add(shirt);
                }
            }
        }


        private void setShirt(bool fromDresser = false)
        {
            Shirt shirt = new Shirt();

            if (Game1.player.shirt.Value < 0)
                shirt = CustomShirtsMod.shirts[(Game1.player.shirt.Value * -1) - 1];

            if (!fromDresser && config.SavedShirts.Find(j => j.Id == Game1.player.UniqueMultiplayerID) is SavedShirt sav && sav.ShirtId != "none")
                shirt = shirts.Find(fj => fj.fullid == sav.ShirtId);

            config.ShirtId = shirt.fullid;

            if (config.SavedShirts.Find(j => j.Id == Game1.player.UniqueMultiplayerID) is SavedShirt sj)
                sj.ShirtId = config.ShirtId;
            else
                config.SavedShirts.Add(new SavedShirt(Game1.player.UniqueMultiplayerID, config.ShirtId));

            Helper.WriteConfig(config);

            if (shirt.fullid != "none")
            {
                Game1.player.changeShirt(shirt.baseid - 1);
                loadShirt();
            }
            else
                PyNet.sendRequestToAllFarmers<bool>(ShirtSyncerName, new ShirtSync(PyUtils.getRectangle(1, 1, Color.White), -9999, Game1.player.UniqueMultiplayerID), null, SerializationType.JSON, 1000);
        }

        public void loadShirt(long mid = -1, bool sendAround = true)
        {
            if (mid == -1)
                mid = Game1.player.UniqueMultiplayerID;

            string shirtId = config.ShirtId;

            if (config.SavedShirts.Find(j => j.Id == mid) is SavedShirt sav)
            {
                shirtId = sav.ShirtId;

                if (mid == Game1.player.UniqueMultiplayerID)
                    config.ShirtId = shirtId;
            }
            else
            {
                config.SavedShirts.Add(new SavedShirt(mid, shirtId));
                Helper.WriteConfig(config);
            }

            Shirt jersey = shirts.Find(j => j.fullid == shirtId);

            if (jersey == null)
                jersey = new Shirt();

            if (playerShirts.ContainsKey(mid))
                playerShirts.Remove(mid);

            if (playerBaseShirts.ContainsKey(mid))
                playerBaseShirts.Remove(mid);

            playerBaseShirts.Add(mid, jersey.baseid);
            playerShirts.Add(mid, shirtId == "null" ? jersey.texture2d = PyUtils.getRectangle(1,1,Color.White) : (ScaledTexture2D)jersey.texture2d);

            if (shirtId == "none")
                PyNet.sendRequestToAllFarmers<bool>(ShirtSyncerName, new ShirtSync(PyUtils.getRectangle(1, 1, Color.White), -9999, mid), null, SerializationType.JSON, 1000);

            if (sendAround)
                if (shirtId == "none")
                    PyNet.sendRequestToAllFarmers<bool>(ShirtSyncerName, new ShirtSync(PyUtils.getRectangle(1, 1, Color.White), -9999, mid), null, SerializationType.JSON, 1000);
                else
                    PyNet.sendRequestToAllFarmers<bool>(ShirtSyncerName, new ShirtSync(jersey.texture2d, jersey.baseid, mid), null, SerializationType.JSON, 1000);
        }
    }
}
