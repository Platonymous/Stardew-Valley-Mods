using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using PyTK;
using PyTK.Extensions;
using Harmony;
using System.Reflection;
using PyTK.Events;
using Netcode;

namespace NoSoilDecayRedux
{
    public class NoSoilDecayReduxMod : Mod
    {
        private static Dictionary<GameLocation, List<Vector2>> hoeDirtChache;
        private static SaveTiles savetiles;
        private static string player;
        internal static IMonitor monitor;
        internal static IModHelper helper;

        public override void Entry(IModHelper helper)
        {
            monitor = Monitor;
            NoSoilDecayReduxMod.helper = Helper;
            savetiles = Helper.ReadConfig<SaveTiles>();
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            TimeEvents.AfterDayStarted += (s,e) => restoreHoeDirt();
            PyTimeEvents.BeforeSleepEvents += (s, e) => saveHoeDirt();
            harmonyFix();
        }

        private void harmonyFix()
        {
            HarmonyInstance instance = HarmonyInstance.Create("Platonymous.NoSoilDecayRedux");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {
            player = Game1.player.Name + "_" + Game1.uniqueIDForThisGame.ToString();
            loadHoeDirt();
            restoreHoeDirt();
        }

        private static void restoreHoeDirt()
        {
            foreach (GameLocation l in hoeDirtChache.Keys)
                if (l is GameLocation)
                {
                    foreach (Vector2 v in hoeDirtChache[l])
                    {
                        if (!l.terrainFeatures.Keys.Contains(v))
                            l.terrainFeatures.Add(v, new NetRef<TerrainFeature>(Game1.isRaining ? new HoeDirt(1) : new HoeDirt(0)));

                        if (!(l.terrainFeatures[v] is HoeDirt))
                            l.terrainFeatures[v] = new NetRef<TerrainFeature>(Game1.isRaining ? new HoeDirt(1) : new HoeDirt(0));

                        if (l.objects.Keys.Contains(v) && l.objects[v] is SObject o && (o.Name.Equals("Weeds") || o.Name.Equals("Stone") || o.Name.Equals("Twig")))
                            l.objects.Remove(v);

                    }

                    foreach (SObject o in l.objects.Values)
                        if (o.name.Contains("Sprinkler"))
                            o.DayUpdate(l);
                }
        }

        internal static void saveHoeDirt()
        {
            hoeDirtChache = new Dictionary<GameLocation, List<Vector2>>();
            foreach (GameLocation location in PyUtils.getAllLocationsAndBuidlings())
            {
                if (location is GameLocation)
                {
                    if (!hoeDirtChache.ContainsKey(location))
                        hoeDirtChache.Add(location, new List<Vector2>());

                    hoeDirtChache[location] = location.terrainFeatures.Keys
                        .Where(t => location.terrainFeatures[t] is HoeDirt)
                        .ToList();
                }
            }

            string savestring = string.Join(";",hoeDirtChache.toList(l => l.Key.Name + ":" + string.Join(",", l.Value.toList(v => $"{v.X}-{v.Y}"))));

            int index = savetiles.save.FindIndex(s => s.StartsWith(player + ">"));

            if (index == -1)
                savetiles.save.Add(player + ">" + savestring);
            else
                savetiles.save[index] = player + ">" + savestring;

            helper.WriteConfig(savetiles);
        }

        private static void loadHoeDirt()
        {
            hoeDirtChache = new Dictionary<GameLocation, List<Vector2>>();

            if (savetiles.save == null)
                savetiles.save = new List<string>();

            int index = savetiles.save.FindIndex(s => s.StartsWith(player + ">"));
            if (index != -1)
            {
                string savestring = savetiles.save[index];
                savestring.Replace(player + ">", "");
                List<string> locationstrings = new List<string>(savestring.Split(';'));
                foreach(string loc in locationstrings)
                {
                    string locationname = loc.Split(':')[0];
                    loc.Replace(locationname + ":", "");
                    List<string> vstrings = new List<string>(loc.Split(','));
                    GameLocation location = PyUtils.getAllLocationsAndBuidlings().Find(e => e.Name == locationname);

                    if (location != null && !hoeDirtChache.ContainsKey(location))
                        hoeDirtChache.Add(location, new List<Vector2>());

                    foreach(string v in vstrings)
                    {
                        string[] split = v.Split('-');

                        if (split.Length < 2)
                            continue;

                        int x = -1;
                        int y = -1;

                        int.TryParse(split[0], out x);
                        int.TryParse(split[1], out y);

                        if (x >= 0 && y >= 0)
                            hoeDirtChache[location].AddOrReplace(new Vector2(x, y));
                    }
                }
            }
            helper.WriteConfig(savetiles);
        }

    }
    
}
