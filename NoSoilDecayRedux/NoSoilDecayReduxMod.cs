using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using PyTK;
using PyTK.Events;
using StardewValley.Locations;

namespace NoSoilDecayRedux
{
    public class NoSoilDecayReduxMod : Mod
    {
        private static SaveData savedata;
        private static Config config;

        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<Config>();
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            TimeEvents.AfterDayStarted += (s,e) => restoreHoeDirt();
            PyTimeEvents.BeforeSleepEvents += (s, e) => saveHoeDirt();
        }

        private void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {
            if (!Game1.IsMultiplayer || Game1.IsServer)
            {
                savedata = Helper.Data.ReadSaveData<SaveData>("nsd.save");
                if (savedata == null)
                    savedata = new SaveData();
                restoreHoeDirt();
            }
        }

        private void restoreHoeDirt()
        {
            foreach (SaveTiles st in savedata.data)
                foreach (GameLocation l in PyUtils.getAllLocationsAndBuidlings().Where(lb => lb.name == st.location))
                {
                    if (config.farmonly && !(l is Farm || l.IsGreenhouse || l is BuildableGameLocation))
                        continue;

                    foreach (Vector2 v in st.tiles)
                        if (!l.terrainFeatures.ContainsKey(v) || !(l.terrainFeatures[v] is HoeDirt))
                        {
                            l.terrainFeatures.Remove(v);
                            l.terrainFeatures.Add(v, Game1.isRaining ? new HoeDirt(1) : new HoeDirt(0));

                            if (l.objects.Keys.Contains(v) && l.objects[v] is SObject o && (o.Name.Equals("Weeds") || o.Name.Equals("Stone") || o.Name.Equals("Twig")))
                                l.objects.Remove(v);
                        }

                    foreach (SObject o in l.objects.Values.Where(obj => obj.name.Contains("Sprinkler")))
                        o.DayUpdate(l);
                }
        }

        private void saveHoeDirt()
        {
            if (!Game1.IsMultiplayer || Game1.IsServer)
            {
                var hoeDirtChache = new Dictionary<GameLocation, List<Vector2>>();
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

                savedata = new SaveData(hoeDirtChache);
                Helper.Data.WriteSaveData("nsd.save", savedata);
            }
        }
    }
    
}
