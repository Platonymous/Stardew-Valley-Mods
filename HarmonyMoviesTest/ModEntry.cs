using HarmonyLib;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Movies;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using xTile.Dimensions;

namespace HarmonyMoviesTest
{
    public class ModEntry : Mod
    {
        internal Config config;
        internal ITranslationHelper i18n => Helper.Translation;
        public static IMonitor mon = null;
        public override void Entry(IModHelper helper)
        {
               mon = Monitor;
            

           

            helper.Events.GameLoop.GameLaunched += (sender, e) =>
            {
                var m1 = AccessTools.Method(typeof(FarmHouse), "performAction");
                var m2 = new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Postfix1));
                var m3 = typeof(System.IO.BinaryReader)
                .GetMethod("Read7BitEncodedInt", BindingFlags.NonPublic | BindingFlags.Instance);
                var m4 = new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Read));

                var m5 = Type.GetType("Microsoft.Xna.Framework.Content.ReflectiveReaderMemberHelper, Microsoft.Xna.Framework")
               .GetMethod("Read", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                var m6 = new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.ReadCT));
                var hm = new HarmonyMethod(m1);

                new Harmony(this.ModManifest.UniqueID)
                    .Patch(
                        original: hm.method,
                        postfix: m2
                    );


                //var data = helper.Data.ReadJsonFile<Dictionary<string, MovieData>>("Movies.json");
                var data = Game1.content.Load<Dictionary<string, MovieData>>("Data//Movies");
                Monitor.Log(string.Join(",", data.Keys),LogLevel.Warn);
            };
        }
      
        public static int id = 0;
        public static string ctr = "";
        public static object creaders = null;
        public static ContentReader iinput = null;

        public static void Postfix1(string action, Farmer who, Location tileLocation)
        {

        }

        public static void ReadCT(object __instance, ref ContentReader input, object parentInstance)
        {
                var ctrr = ((ContentTypeReader) __instance.GetType().GetField("typeReader", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance));
        }

        public static void Read(ref int __result)
        {
            id++;
              int r = __result;
              {
                  if (id == 16)
                      r = 2;
                  if (id == 17)
                      r = 24;
                  if (id == 18)
                      r = 2;
                  if (id == 19)
                      r = 88;

              }
              {
                  if (id == 130)
                      r = 2;
                  if (id == 131)
                      r = 9;
                  if (id == 132)
                      r = 2;
                  if (id == 133)
                      r = 64;
              }
            mon.Log(id + ": " + ctr, LogLevel.Warn);
           // mon.Log(id + ":" + __result + (r != __result ? (" -> " + r) : ""), LogLevel.Warn);
           __result = r;

        }
        public static void Postfix(ref System.Object parentInstance)
        {
            if (parentInstance is MovieData data)
            {
                mon.Log("--POST--", LogLevel.Info);
                mon.Log("ID:" + data.ID, LogLevel.Warn);
                mon.Log("SheetIndex:" + data.SheetIndex.ToString(), LogLevel.Warn);
                mon.Log("Title:" + data.Title, LogLevel.Warn);
                mon.Log("Description:" + data.Description, LogLevel.Warn);
                mon.Log("Tags:" + ((data.Tags != null ? string.Join(",", data.Tags) : "null")), LogLevel.Warn);
                mon.Log("Scenes:" + ((data.Scenes != null) ? "List" : "null"), LogLevel.Warn);
            }
        }

        public static void Prefix() {

            mon.Log("Prefix", LogLevel.Error);
        }
    }
}
