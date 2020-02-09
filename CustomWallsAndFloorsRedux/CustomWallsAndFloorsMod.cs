using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.Reflection;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using PyTK.Extensions;
using System.Linq;
using StardewValley.Objects;
using PyTK.Types;
using StardewValley.Locations;
using System;

namespace CustomWallsAndFloorsRedux
{
    public class CustomWallsAndFloorsMod : Mod
    {
        internal static List<CustomSet> Sets = new List<CustomSet>();

        internal static SaveData Saved = new SaveData();

        const string saveDataKey = "Platonymous.CWFRedux.SaveData";

        const string wpReceiverName = "Platonymous.CWFRedux.Receiver";

        PyReceiver<SavedWallpaper> wallpaperReceiver;

        public static List<SavedWallpaper> Requests = new List<SavedWallpaper>();

        internal static bool Placing { get; set; } = false;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.ReturnedToTitle += (s,e) => Requests = new List<SavedWallpaper>();
            helper.Events.GameLoop.Saving += (s, e) =>
            {
                Requests = new List<SavedWallpaper>();
                if (!Game1.IsMasterGame)
                    return;

                Helper.Data.WriteSaveData(saveDataKey, Saved);
            };

            helper.Events.GameLoop.SaveLoaded += (s, e) =>
            {
                    LoadSavedWallsAndFloors();
            };
            
            helper.Events.GameLoop.DayStarted += (s, e) =>
            {

                if (!Game1.IsMultiplayer)
                {
                    Requests.Clear();
                    LoadSavedWallsAndFloors();
                }
            };

            helper.Events.Multiplayer.PeerContextReceived += (s, e) =>
            {
                if(Game1.IsMasterGame)
                    PyTK.PyUtils.setDelayedAction(1000, () => LoadSavedWallsAndFloors(true));
            };

            helper.Events.Player.Warped += (st, e) =>
            {
                foreach (var req in Requests.Where(r => r.Location == e.NewLocation.Name).ToList())
                    LoadReceivedData(req, e.NewLocation);
            };

            helper.ConsoleCommands.Add("reset_walls", "removes all custom walls and floors", (s, p) => reset());
            helper.ConsoleCommands.Add("reload_walls", "reloads all custom walls and floors", (s, p) => LoadSavedWallsAndFloors());

        }

        private void reset()
        {
            foreach (GameLocation location in Game1.locations)
                foreach (var layer in location.Map.Layers.ToList())
                    if (layer.Id.StartsWith("z_cwf"))
                        location.Map.RemoveLayer(layer);
            Saved = new SaveData();
            if(Game1.IsMasterGame)
                Helper.Data.WriteSaveData(saveDataKey, Saved);
        }

        private void LoadSavedWallsAndFloors(bool send = false)
        {
            Saved = new SaveData();

            if (!Game1.IsMasterGame)
                return;

            SaveData saveData = Helper.Data.ReadSaveData<SaveData>(saveDataKey);

            if (saveData != null)
                foreach (SavedWallpaper sav in saveData.Wallpaper)
                {

                    if (Game1.IsMultiplayer || (Game1.currentLocation is GameLocation l && sav.Location == l.Name))
                    {
                        if (Sets.FirstOrDefault(s => s.Pack.Manifest.UniqueID == sav.SetId) is CustomSet set && Game1.getLocationFromName(sav.Location) is GameLocation location)
                            if (sav.IsFloors && set.HasFloors && set.GetFloors().FirstOrDefault(f => f.CustomIndex == sav.CustomIndex) is CustomWallpaper cf)
                                cf.place(location, sav.X, sav.Y, send);
                            else if (!sav.IsFloors && set.HasWalls && set.GetWalls().FirstOrDefault(w => w.CustomIndex == sav.CustomIndex) is CustomWallpaper cw)
                                cw.place(location, sav.X, sav.Y, send);
                    }
                        Requests.Add(sav);
                }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            wallpaperReceiver = new PyReceiver<SavedWallpaper>(wpReceiverName, (w) =>
            {
                if (Game1.getLocationFromName(w.Location) is GameLocation location)
                {
                    if (!Game1.IsMasterGame)
                    {
                        if (Game1.currentLocation.Name == w.Location)
                        {
                            LoadReceivedData(w, location);
                        }
                        else
                        {
                            Requests.Add(w);
                        }
                    }
                    else
                    {
                        LoadReceivedData(w, location);
                    }

                }
            }, 100, SerializationType.JSON);
            wallpaperReceiver.start();

            var harmony = HarmonyInstance.Create("Platonymous.CustomWallsAndFloorsRedux");
            harmony.Patch(typeof(GameLocation).GetMethod("setMapTileIndex", BindingFlags.Public | BindingFlags.Instance), new HarmonyMethod(GetType().GetMethod("setMapTileIndexPrefix", BindingFlags.Public | BindingFlags.Static)), new HarmonyMethod(GetType().GetMethod("setMapTileIndex", BindingFlags.Public | BindingFlags.Static)));
            harmony.Patch(typeof(Wallpaper).GetMethod("placementAction", BindingFlags.Public | BindingFlags.Instance), prefix: new HarmonyMethod(GetType().GetMethod("placementAction", BindingFlags.Public | BindingFlags.Static)));

            foreach (var pack in Helper.ContentPacks.GetOwned())
                Sets.Add(new CustomSet(pack));
        }

        private void LoadReceivedData(SavedWallpaper w, GameLocation location)
        {
            if (Requests.Contains(w))
                Requests.Remove(w);

            if (!Saved.Wallpaper.Contains(w) && Sets.FirstOrDefault(s => s.Id == w.SetId) is CustomSet set && (w.IsFloors ? set.GetFloors() : set.GetWalls()).FirstOrDefault(cw => cw.CustomIndex == w.CustomIndex) is CustomWallpaper wall)
                wall.place(location, w.X, w.Y);
        }

        public static void placementAction()
        {
            Placing = true;
            PyTK.PyUtils.setDelayedAction(10, () => Placing = false);
        }

        public static bool TryGetLayer(Map map, string layerId, out Layer layer)
        {
            try
            {
                Layer l = map.GetLayer(layerId);
                layer = l;
                return l != null;
            }
            catch
            {
                layer = null;
                return false;
            }
        }

        public static bool setMapTileIndexPrefix(GameLocation __instance, int tileX, int tileY, ref int index, string layer, ref int whichTileSheet)
        {
            if (CustomWallpaper.BeingPlaced == null)
                return true;

            Layer tileLayer;

            if (!TryGetLayer(__instance.Map, layer, out tileLayer))
                return true;

            try
            {
                if (tileLayer.Tiles[tileX, tileY] is Tile tile && tile.TileSheet.Id != "walls_and_floors")
                    return false;
            }
            catch { }

            return true;
        }

        public static void setMapTileIndex(GameLocation __instance, int tileX, int tileY, ref int index, string layer, ref int whichTileSheet)
        {
            try
            {
                if (Placing)
                {
                    foreach (Layer la in __instance.Map.Layers.Where(l => l.Id.StartsWith("z_cwf") && l.Id.EndsWith(layer)))
                        la.Tiles[tileX, tileY] = null;

                    Saved.Wallpaper.RemoveAll(s => s.Location == __instance.Name && s.TileX == tileX && s.TileY == tileY && s.Layer == layer);
                }
            }
            catch
            {

            }

            Layer tileLayer;

            if (!TryGetLayer(__instance.Map, layer, out tileLayer))
                return;

            if (CustomWallpaper.BeingPlaced == null)
                return;

            try
            {
                if (tileLayer.Tiles[tileX, tileY] is Tile tile && tile.TileSheet.Id != "walls_and_floors")
                    return;
            }
            catch { }

            if (CustomWallpaper.BeingSaved != null)
            {
                CustomWallpaper.BeingSaved.TileX = tileX;
                CustomWallpaper.BeingSaved.TileY = tileY;
                CustomWallpaper.BeingSaved.Layer = layer;

                if(!Saved.Wallpaper.Contains(CustomWallpaper.BeingSaved))
                    Saved.Wallpaper.Add(CustomWallpaper.BeingSaved);

                if (CustomWallpaper.BeingSaved.ShouldBeSend)
                    foreach (Farmer farmer in Game1.getAllFarmers().Where(f => f.isActive() && f != Game1.player))
                    {
                        PyTK.PyNet.sendDataToFarmer(wpReceiverName, CustomWallpaper.BeingSaved, farmer, SerializationType.JSON);
                    }
                CustomWallpaper.BeingSaved = null;
            }

            int anims = 1;
            int speed = 1000;

            if (CustomWallpaper.BeingPlaced.Animation is Animation a)
            {
                anims = a.Frames;
                speed = a.Length;
            }

            bool isFloor = CustomWallpaper.BeingPlaced.isFloor.Value;
            int cIndex = CustomWallpaper.BeingPlaced.CustomIndex;
            CustomSet set = CustomWallpaper.BeingPlaced.Set;
            string tsid = $"z_cwf_{set.Id}_{(isFloor ? "floors" : "walls")}_{cIndex}";
            TileSheet tileSheet = __instance.Map.TileSheets.FirstOrDefault(t => t.Id == tsid);
            if (tileSheet == null)
            {
                tileSheet = new TileSheet(tsid, __instance.map, CustomWallpaper.BeingPlaced.AssetKey, new Size((isFloor ? 2 * anims : 1 * anims), (isFloor ? 2 : 3)), new Size(Game1.tileSize, Game1.tileSize));
                __instance.Map.AddTileSheet(tileSheet);
                __instance.Map.LoadTileSheets(Game1.mapDisplayDevice);
            }

            Layer cwfLayer;

            if (!TryGetLayer(__instance.Map, tsid + "_" + layer, out cwfLayer))
            {
                cwfLayer = new Layer(tsid + "_" + layer, __instance.Map, tileLayer.LayerSize, tileLayer.TileSize);
                cwfLayer.Properties.Add("DrawAbove", layer);
                __instance.Map.AddLayer(cwfLayer);
                __instance.Map.enableMoreMapLayers();
            }

            int nIndex = (index / 16) * anims;

            if (isFloor)
                switch (index)
                {
                    case 336: nIndex = 0; break;
                    case 337: nIndex = 1; break;
                    case 352: nIndex = 2 + ((anims - 1) * 2); break;
                    case 353: nIndex = 3 + ((anims - 1) * 2); break;
                }

            index = nIndex;
            whichTileSheet = __instance.Map.TileSheets.IndexOf(tileSheet);
            try
            {
                foreach (Layer la in __instance.Map.Layers.Where(l => l.Id.StartsWith("z_cwf") && l.Id.EndsWith(layer)))
                    la.Tiles[tileX, tileY] = null;

                List<StaticTile> tiles = new List<StaticTile>() { new StaticTile(cwfLayer, tileSheet, BlendMode.Alpha, index) };

                for (int i = 1; i < anims; i++)
                    tiles.Add(new StaticTile(cwfLayer, tileSheet, BlendMode.Alpha, ((isFloor ? i * 2 : i) + index)));

                cwfLayer.Tiles[tileX, tileY] = tiles.Count == 1 ? tiles[0] : (Tile)new AnimatedTile(cwfLayer, tiles.ToArray(), speed);
            }
            catch
            {

            }
        }
    }
}
