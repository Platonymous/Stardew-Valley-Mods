using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using PyTK;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FanShirts
{
    public class FanShirtsMod : Mod
    {
        internal static Config config;

        internal static Dictionary<long, ScaledTexture2D> playerJerseys = new Dictionary<long, ScaledTexture2D>();
        internal static Dictionary<long, int> playerBaseJerseys = new Dictionary<long, int>();
        internal static List<FanPack> packs = new List<FanPack>();
        internal static List<Jersey> jerseys = new List<Jersey>();
        internal static Texture2D vanillaShirts;
        internal static bool worldIsReady => Context.IsWorldReady;
        internal static string ShirtSyncerName = "Platonymous.FanShirts.Sync";
        internal static string ShirtReloaderName = "Platonymous.FanShirts.Reload";
        internal static IPyResponder ShirtSyncer;
        internal static IPyResponder ShirtReloader;

        internal static IMonitor _monitor;

        public override void Entry(IModHelper helper)
        {
            vanillaShirts = Helper.Content.Load<Texture2D>(@"Characters/Farmer/shirts", ContentSource.GameContent);
            ShirtSyncer = new PyResponder<bool, ShirtSync>(ShirtSyncerName, (s) =>
              {
                  try
                  {
                      if (playerJerseys.ContainsKey(s.FarmerId))
                          playerJerseys.Remove(s.FarmerId);

                      if (playerBaseJerseys.ContainsKey(s.FarmerId))
                          playerBaseJerseys.Remove(s.FarmerId);

                      playerJerseys.Add(s.FarmerId, (ScaledTexture2D) s.Texture.getTexture());
                      playerBaseJerseys.Add(s.FarmerId, s.BaseShirt);
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

            HarmonyInstance harmony = HarmonyInstance.Create("Platonymous.FanShirts");
            harmony.Patch(typeof(FarmerRenderer).GetMethod("drawHairAndAccesories"), new HarmonyMethod(typeof(Overrides.OvFarmerRenderer).GetMethod("Prefix_drawHairAndAccesories")), new HarmonyMethod(typeof(Overrides.OvFarmerRenderer).GetMethod("Postfix_drawHairAndAccesories")));

            
            packs = PyUtils.loadContentPacks<FanPack>(Path.Combine(helper.DirectoryPath, "FanPacks"), SearchOption.AllDirectories, Monitor);
            foreach (FanPack fp in packs)
                fp.jerseys.ForEach((j) =>
                {
                    j.fullid = fp.id + "." + j.id;
                    string path = @"FanPacks/" + fp.folderName + "/" + j.texture;
                    j.texture2d = ScaledTexture2D.FromTexture(vanillaShirts,Helper.Content.Load<Texture2D>(path),j.scale);
                    jerseys.Add(j);
                });

           config.SwitchKey.onPressed(() =>
            {
                if (!Context.IsWorldReady)
                    return;

                int index = jerseys.FindIndex(j => j.fullid == config.JerseyID);
                int next = index + 1 >= jerseys.Count ? 0 : index + 1;

                config.JerseyID = jerseys[next].fullid;

                if (config.SavedJerseys.Find(j => j.Id == Game1.player.UniqueMultiplayerID) is SavedJersey sj)
                    sj.JerseyID = config.JerseyID;
                else
                    config.SavedJerseys.Add(new SavedJersey(Game1.player.UniqueMultiplayerID, config.JerseyID));

                helper.WriteConfig(config);

                loadJersey();
            });

            SaveEvents.AfterLoad += (s, e) =>
            {
                loadJersey();
                PyNet.sendRequestToAllFarmers<long>(ShirtReloaderName, Game1.player.UniqueMultiplayerID, null);
            };

            foreach(SavedJersey sj in config.SavedJerseys)
                loadJersey(sj.Id, false);
        }

        private void loadJersey(long mid = -1, bool sendAround = true)
        {
            if (mid == -1)
                mid = Game1.player.UniqueMultiplayerID;

            string jerseyId = config.JerseyID;

            if (config.SavedJerseys.Find(sj => sj.Id == mid) is SavedJersey sav)
            {
                jerseyId = sav.JerseyID;

                if (mid == Game1.player.UniqueMultiplayerID)
                    config.JerseyID = jerseyId;
            }
            else
            {
                config.SavedJerseys.Add(new SavedJersey(mid, jerseyId));
                Helper.WriteConfig(config);
            }

            Jersey jersey = jerseys.Find(j => j.fullid == jerseyId);

            if (jersey == null)
            {
                Monitor.Log("Couldn't find jersey:" + jerseyId, LogLevel.Error);
                return;
            }

            if (playerJerseys.ContainsKey(mid))
                playerJerseys.Remove(mid);

            if (playerBaseJerseys.ContainsKey(mid))
                playerBaseJerseys.Remove(mid);

            playerBaseJerseys.Add(mid, jersey.baseid);
            playerJerseys.Add(mid, (ScaledTexture2D) jersey.texture2d);

            if(sendAround)
                PyNet.sendRequestToAllFarmers<bool>(ShirtSyncerName, new ShirtSync(jersey.texture2d, jersey.baseid, mid), null, SerializationType.JSON, 1000);

        }
    }
}
