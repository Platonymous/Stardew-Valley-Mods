using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Locations;
using StardewValley.Buildings;

namespace NoSoilDecayRedux
{
    public class NoSoilDecayReduxMod : Mod
    {
        private static SaveData savedata;
        private static Config config;

        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<Config>();
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += (s,e) => restoreHoeDirt();
            helper.Events.GameLoop.DayEnding += (s, e) => saveHoeDirt();
            helper.Events.GameLoop.GameLaunched += (s, e) => addConfig();
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Game1.IsMasterGame)
            {
                savedata = Helper.Data.ReadSaveData<SaveData>("nsd.save");
                if (savedata == null)
                    savedata = new SaveData();
                restoreHoeDirt();
            }
        }

        private void restoreHoeDirt()
        {
            if (Game1.IsMasterGame)
            {
                foreach (SaveTiles st in savedata.data)
                    foreach (GameLocation l in getAllLocationsAndBuidlings().Where(lb => lb.Name == st.location))
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
        }

        private IEnumerable<GameLocation> getAllLocationsAndBuidlings()
        {
            foreach (GameLocation location in Game1.locations)
            {
                yield return location;
                if (location is BuildableGameLocation bgl)
                    foreach (Building building in bgl.buildings)
                        if (building.indoors.Value != null)
                            yield return building.indoors.Value;
            }
        }

        private void saveHoeDirt()
        {
            if (Game1.IsMasterGame)
            {
                var hoeDirtChache = new Dictionary<GameLocation, List<Vector2>>();
                foreach (GameLocation location in getAllLocationsAndBuidlings())
                {
                    if (location is GameLocation l)
                    {
                        if (config.farmonly && !(l is Farm || l.IsGreenhouse || l is BuildableGameLocation))
                            continue;

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

        private void addConfig()
        {
            if (!Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
                return;

            var api = Helper.ModRegistry.GetApi<IGMCMAPI>("spacechase0.GenericModConfigMenu");

            api.RegisterModConfig(ModManifest, () => config = new Config(), () => Helper.WriteConfig<Config>(config));
            api.RegisterSimpleOption(ModManifest, "Only on Farm-Locations", "", () => config.farmonly, (value) => config.farmonly = value);
        }
    }
    
}
