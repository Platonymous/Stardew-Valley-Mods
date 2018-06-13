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
using System.Reflection;

namespace FanShirts
{
    public class FanShirtsMod : Mod
    {
        internal static Config config;
        internal static NetStringDictionary<Texture2D, PyNetTexture> playerJerseys = new NetStringDictionary<Texture2D, PyNetTexture>();
        internal static NetStringDictionary<int, NetInt> playerBaseJerseys = new NetStringDictionary<int, NetInt>();
        internal static List<Jersey> jerseys = new List<Jersey>();
        internal static Texture2D vanillaShirts;
        internal static bool worldIsReady => Context.IsWorldReady;
        internal static Vector2 syncDummyPlace = new Vector2(-20, -14);

        internal static IMonitor _monitor;

        public override void Entry(IModHelper helper)
        {
            _monitor = Monitor;
            config = helper.ReadConfig<Config>();

            HarmonyInstance harmony = HarmonyInstance.Create("Platonymous.FanShirts");
            if(config.SyncInMultiplayer)
                harmony.Patch(typeof(Farm).GetMethod("initNetFields", BindingFlags.Instance | BindingFlags.NonPublic), null, new HarmonyMethod(typeof(Overrides.OvFarmerRenderer).GetMethod("NetFieldFix")));
            harmony.Patch(typeof(FarmerRenderer).GetMethod("drawHairAndAccesories"), new HarmonyMethod(typeof(Overrides.OvFarmerRenderer).GetMethod("Prefix_drawHairAndAccesories")), new HarmonyMethod(typeof(Overrides.OvFarmerRenderer).GetMethod("Postfix_drawHairAndAccesories")));

            vanillaShirts = Helper.Content.Load<Texture2D>(@"Characters/Farmer/shirts", ContentSource.GameContent);


            Monitor.Log($"Started {helper.ModRegistry.ModID} from folder: {helper.DirectoryPath}");

            PyUtils.loadContentPacks(out jerseys, Path.Combine(helper.DirectoryPath, "Jerseys"), SearchOption.TopDirectoryOnly, Monitor);
            Keys.J.onPressed(() =>
            {
                if (!Context.IsWorldReady)
                    return;

                int index = jerseys.FindIndex(j => j.Id == config.JerseyID);
                int next = index + 1 >= jerseys.Count ? 0 : index + 1;
                config.JerseyID = jerseys[next].Id;
                helper.WriteConfig(config);
                loadJersey();
            });
            SaveEvents.AfterLoad += (s,e) => loadJersey();
        }

        private void loadJersey()
        {
            Jersey jersey = jerseys.Find(j => j.Id == config.JerseyID);

            if (jersey == null)
            {
                Monitor.Log("Couldn't find jersey id", LogLevel.Error);
                return;
            }

            Texture2D tex = FarmerRenderer.shirtsTexture;
            Texture2D tex4x = Helper.Content.Load<Texture2D>(@"Jerseys/" + jersey.Texture);
            ScaledTexture2D stex = ScaledTexture2D.FromTexture(tex, tex4x, 4);

            if (playerJerseys.ContainsKey(Game1.player.UniqueMultiplayerID.ToString()))
                playerJerseys.Remove(Game1.player.UniqueMultiplayerID.ToString());

            if (playerBaseJerseys.ContainsKey(Game1.player.UniqueMultiplayerID.ToString()))
                playerBaseJerseys.Remove(Game1.player.UniqueMultiplayerID.ToString());

            playerBaseJerseys.Add(Game1.player.UniqueMultiplayerID.ToString(), new NetInt(jersey.BaseShirt));
            playerJerseys.Add(Game1.player.UniqueMultiplayerID.ToString(), new PyNetTexture(stex));
            playerBaseJerseys.MarkDirty();
            playerJerseys.MarkDirty();
        }

        private void setJersey(Farmer farmer, int id, ScaledTexture2D stex)
        {   
            farmer.changeShirt(id);
            Rectangle sr = Game1.getSourceRectForStandardTileSheet(vanillaShirts, id, 8, 32);
            stex.DestinationPositionAdjustment = new Vector2(0, -96);
            stex.SourcePositionAdjustment = new Vector2(-(sr.X * 4), -(sr.Y * 4));
            FarmerRenderer.shirtsTexture = stex;
        }
    }
}
