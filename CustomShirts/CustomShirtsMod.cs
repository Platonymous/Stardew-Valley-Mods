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

namespace CustomShirts
{
    public class CustomShirtsMod : Mod
    {
        internal static Config config;

        internal static Dictionary<long, Texture2D> playerShirts = new Dictionary<long, Texture2D>();
        internal static Dictionary<long, int> playerBaseShirts = new Dictionary<long, int>();
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

                      playerShirts.Add(s.FarmerId, (ScaledTexture2D)s.Texture.getTexture());
                      playerBaseShirts.Add(s.FarmerId, s.BaseShirt);
                  }
                  catch (Exception e)
                  {
                      Monitor.Log(e.Message + e.StackTrace, LogLevel.Alert);
                  }
                  return true;
              }, 60, SerializationType.PLAIN, SerializationType.JSON);

            ShirtReloader = new PyResponder<long, long>(ShirtReloaderName, (s) =>
            {
                loadJersey();
                return Game1.player.UniqueMultiplayerID;
            }, 60);

            ShirtSyncer.start();
            ShirtReloader.start();

            _monitor = Monitor;
            config = helper.ReadConfig<Config>();

            HarmonyInstance harmony = HarmonyInstance.Create("Platonymous.CustomShirts");
            harmony.Patch(typeof(FarmerRenderer).GetMethod("drawHairAndAccesories"), new HarmonyMethod(typeof(Overrides.OvFarmerRenderer).GetMethod("Prefix_drawHairAndAccesories")), new HarmonyMethod(typeof(Overrides.OvFarmerRenderer).GetMethod("Postfix_drawHairAndAccesories")));
            harmony.Patch(typeof(Farmer).GetMethod("changeShirt"), new HarmonyMethod(typeof(Overrides.OvFarmerRenderer).GetMethod("Prefix_changeShirt")), null);

            loadContentPacks();

            if(config.SwitchKey != Keys.Escape)
                config.SwitchKey.onPressed(() => Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.Wizard));

            GameEvents.QuarterSecondTick += (s, e) =>
            {
                if (recolor)
                {
                    OvFarmerRenderer.recolorShirt();
                    recolor = false;
                }
            };

            SaveEvents.AfterLoad += (s, e) =>
            {
                setJersey();
                loadJersey();
                PyNet.sendRequestToAllFarmers<long>(ShirtReloaderName, Game1.player.UniqueMultiplayerID, null);
            };

            MenuEvents.MenuClosed += (s, e) =>
            {
                if (e.PriorMenu is CharacterCustomization || e.PriorMenu.GetType().Name.ToLower().Contains("character"))
                    setJersey(true);
            };

            foreach (SavedShirt sj in config.SavedShirts)
                loadJersey(sj.Id, false);
        }

        private void loadContentPacks()
        {
            foreach(StardewModdingAPI.IContentPack pack in Helper.GetContentPacks())
            {
                ShirtPack shirtPack = pack.ReadJsonFile<ShirtPack>("content.json");

                foreach (Shirt shirt in shirtPack.shirts)
                {
                    shirt.fullid = pack.Manifest.UniqueID + "." + shirt.id;
                    shirt.texture2d = ScaledTexture2D.FromTexture(vanillaShirts, pack.LoadAsset<Texture2D>(shirt.texture), shirt.scale);
                    shirts.Add(shirt);
                }
            }
        }


        private void setJersey(bool fromDresser = false)
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
                loadJersey();
            }
        }

        private void loadJersey(long mid = -1, bool sendAround = true)
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
