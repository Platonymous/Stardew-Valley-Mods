using Microsoft.Xna.Framework;
using PyTK;
using PyTK.Extensions;
using PyTK.Lua;
using PyTK.Tiled;
using PyTK.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.IO;
using xTile;
using Harmony;
using System.Reflection;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using System.Xml;
using xTile.ObjectModel;
using System;
using xTile.Tiles;
using PyTK.PlatoUI;
using xTile.Dimensions;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework.Input;

namespace TMXLoader
{
    public class TMXLoaderMod : Mod
    {
        internal static string contentFolder = "Maps";
        internal static IMonitor monitor;
        internal static IModHelper helper;
        internal static Dictionary<string, Map> mapsToSync = new Dictionary<string, Map>();
        internal static List<Farmer> syncedFarmers = new List<Farmer>();
        internal static Dictionary<string, List<TileShopItem>> tileShops = new Dictionary<string, List<TileShopItem>>();
        internal static Config config;
        internal static SaveData saveData = new SaveData();
        internal static List<MapEdit> addedLocations = new List<MapEdit>();
        internal static List<string> festivals = new List<string>();
        internal static List<TMXAssetEditor> conditionals = new List<TMXAssetEditor>();
        internal static List<BuildableEdit> buildables = new List<BuildableEdit>();
        internal static List<SaveBuildable> buildablesBuild = new List<SaveBuildable>();
        internal static Dictionary<string, Warp> buildablesExits = new Dictionary<string, Warp>();

        internal string buildableReceiverName = "BuildableReceiver";
        internal string buildableRemoverName = "BuildableRemover";

        internal PyReceiver<SaveBuildable> BuildableReceiver;
        internal PyReceiver<SaveBuildable> BuildableRemover;


        public override void Entry(IModHelper helper)
        {
            BuildableReceiver = new PyReceiver<SaveBuildable>(buildableReceiverName, (s) =>
            {
                if (Game1.IsMasterGame)
                    return;

                foreach (SaveBuildable b in buildablesBuild)
                    if (b.UniqueId == s.UniqueId)
                        return;

                Monitor.Log("Received Placement for " + s.Id);
                loadSavedBuildable(s);
            }, 60, SerializationType.JSON);

            BuildableReceiver.start();

            BuildableRemover = new PyReceiver<SaveBuildable>(buildableRemoverName, (s) =>
            {
                if (Game1.IsMasterGame)
                    return;

                Monitor.Log("Received Removal for " + s.Id);
                foreach (SaveBuildable b in buildablesBuild)
                    if (b.UniqueId == s.UniqueId)
                    {
                        removeSavedBuildable(b, false, false);
                        Monitor.Log("Removed " + s.Id);

                        return;
                    }
            }, 60, SerializationType.JSON);

            BuildableRemover.start();

            config = Helper.ReadConfig<Config>();
            TMXLoaderMod.helper = Helper;
            monitor = Monitor;

            helper.ConsoleCommands.Add("buildables", "Show Buildables Menu", (s, p) =>
            {
                showAll = true;
                if (Context.IsWorldReady && buildables.Count > 0)
                    showBuildablesMenu();
            });

            helper.Events.GameLoop.ReturnedToTitle += (s, e) =>
            {
                removeAllSavedBuildables();
            };

            helper.Events.Input.ButtonReleased += (s, e) =>
            {
                showAll = false;
                if (e.Button == config.openBuildMenu.ToSButton() && Context.IsWorldReady && buildables.Count > 0)
                    showBuildablesMenu();
            };

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            // helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

            helper.Events.Multiplayer.PeerContextReceived += (s, e) =>
            {
                foreach (SaveBuildable b in buildablesBuild)
                    PyNet.sendDataToFarmer(buildableReceiverName, b, e.Peer.PlayerID, SerializationType.JSON);
            };

            helper.Events.Display.RenderingHud += (s, e) =>
            {
                PropertyValue rain = "";

                if (!(Game1.currentLocation is GameLocation location) || !(location.Map is Map map) || !map.Properties.TryGetValue("Raining", out rain))
                    return;

                if (Game1.isRaining && Game1.currentLocation.IsOutdoors && (!Game1.currentLocation.Name.Equals("Desert") && !(Game1.currentLocation is Summit)) && (!Game1.eventUp || Game1.currentLocation.isTileOnMap(new Vector2((float)(Game1.viewport.X / 64), (float)(Game1.viewport.Y / 64)))))
                    return;

                if (Game1.isRaining || rain == "Always")
                    for (int index = 0; index < Game1.rainDrops.Length; ++index)
                        Game1.spriteBatch.Draw(Game1.rainTexture, Game1.rainDrops[index].position, new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.rainTexture, Game1.rainDrops[index].frame, -1, -1)), Color.White);
            };

            helper.Events.Player.Warped += Player_Warped;


            helper.Events.GameLoop.DayStarted += (s, e) =>
            {
                foreach (var location in addedLocations)
                    if (Game1.getLocationFromName(location.name) is GameLocation l)
                        l.seasonUpdate(Game1.currentSeason);
            };


            helper.Events.GameLoop.Saving += (s, e) =>
            {
                if (!Game1.IsMasterGame)
                    return;

                saveData = new SaveData();
                saveData.Locations = new List<SaveLocation>();
                saveData.Buildables = new List<SaveBuildable>();

                foreach (var l in addedLocations)
                    if (Game1.getLocationFromName(l.name) is GameLocation location)
                        saveData.Locations.Add(getLocationSaveData(location));

                foreach (var b in buildablesBuild)
                {
                    BuildableEdit edit = buildables.Find(be => be.id == b.Id);

                    if (edit.indoorsFile != null && Game1.getLocationFromName(getLocationName(b.UniqueId)) is GameLocation location)
                        b.Indoors = getLocationSaveData(location);

                    saveData.Buildables.Add(b);
                }

                Helper.Data.WriteSaveData<SaveData>("Locations", saveData);
            };

            helper.Events.GameLoop.SaveLoaded += (s, e) =>
            {
                restoreAllSavedBuildables();
            };

        }

        private void restoreAllSavedBuildables()
        {
            buildablesBuild = new List<SaveBuildable>();
            buildablesExits = new Dictionary<string, Warp>();

            foreach (var location in addedLocations)
                if (Game1.getLocationFromName(location.name) == null)
                    addLocation(location).updateSeasonalTileSheets();

            if (!Game1.IsMasterGame)
                return;

            saveData = Helper.Data.ReadSaveData<SaveData>("Locations");
            if (saveData != null)
            {
                foreach (SaveLocation loc in saveData.Locations)
                    setLocationObejcts(loc);

                foreach (var b in saveData.Buildables)
                    loadSavedBuildable(b);
            }
        }

        private void removeAllSavedBuildables()
        {
            foreach (var toRemove in buildablesBuild)
                removeSavedBuildable(toRemove,false, false);
        }

        private void removeSavedBuildable(SaveBuildable toRemove, bool pay, bool distribute)
        {
            try
            {
                buildablesBuild.Remove(toRemove);
                removeAssetEditor(toRemove._editor);
                if (Game1.getLocationFromName(toRemove.Location) is GameLocation location)
                {
                    helper.Content.InvalidateCache<Map>();

                    Helper.Reflection.GetMethod(location, "reloadMap").Invoke();
                    location.map.enableMoreMapLayers();
                }

                if (pay && Game1.IsMasterGame && buildables.Find(b => b.id == toRemove.Id) is BuildableEdit be)
                {
                    Game1.player.Money += be.price;
                    List<Item> items = new List<Item>();

                    foreach (var buildItem in be.buildItems)
                    {
                        if (TMXActions.getItem(buildItem.type, buildItem.index, buildItem.name) is Item item)
                        {
                            item.Stack = buildItem.stack;
                            items.Add(item);
                        }
                    }

                    if (items.Count > 0)
                        Game1.player.addItemsByMenuIfNecessary(items);

                }
            }
            catch(Exception e)
            {
                Monitor.Log(e.Message + ":" + e.StackTrace);
            }

            if (distribute && Game1.IsMultiplayer)
            {
                Monitor.Log("Send Removal request");
                PyNet.sendRequestToAllFarmers<bool>(buildableRemoverName, toRemove, null, SerializationType.JSON, -1);
            }
        }
        private void loadSavedBuildable(SaveBuildable b)
        {
            if (Game1.getLocationFromName(b.Location) is GameLocation location)
            {
                BuildableEdit edit;

                if (buildables.Find(bd => bd.id == b.Id) is BuildableEdit build)
                    edit = build;
                else
                    return;

                buildBuildableEdit(false, edit, location, new Point(b.Position[0], b.Position[1]), b.Colors, b.UniqueId, false);
                if (b.Indoors != null)
                    setLocationObejcts(b.Indoors);

            }
        }

        private string getLocationName(string uniqueId)
        {
            return "BuildableIndoors" + "-" + uniqueId;
        }

        private GameLocation buildBuildableIndoors(BuildableEdit edit, string uniqueId, GameLocation location)
        {
            if (edit.indoorsFile != null && edit._pack != null)
            {
                string buildFile = edit.indoorsFile;

                Map map = TMXContent.Load(buildFile, Helper, edit._pack);
                var e = edit.Clone();
                e.name = getLocationName(uniqueId);
                e._map = map;
                Map m = null;

                try
                {
                    m = Helper.Content.Load<Map>("Maps/" + e.name, ContentSource.GameContent);
                }
                catch (Exception ex)
                {
                    Monitor.Log(ex.Message + ":" + ex.StackTrace);
                }

                if (m == null)
                    map.inject("Maps/" + e.name);

                return addLocation(e);
            }
            else
                return null;
        }

        private void buildBuildableEdit(bool pay, BuildableEdit edit, GameLocation location, Point position, Dictionary<string, string> colors,  string uniqueId = null, bool distribute = true)
        {
            if (uniqueId == null)
                uniqueId = ((ulong)Helper.Multiplayer.GetNewID()).ToString();

            edit.position = new int[] { position.X, position.Y };
            GameLocation indoors = buildBuildableIndoors(edit, uniqueId, location);

            if (indoors != null)
                buildablesExits.AddOrReplace(indoors.Name, new Warp(0, 0, location.Name, edit.exitTile[0] + position.X, edit.exitTile[1] + position.Y, false));

            string buildFile = edit.file;

            Map map = edit._map;
            if (edit._pack != null)
                map = TMXContent.Load(buildFile, Helper, edit._pack);

            var size = new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1);

            Func<KeyValuePair<string, PropertyValue>, bool> propCheck = (prop) =>
            {
                return prop.Value.ToString().Contains("XPOSITION") || prop.Value.ToString().Contains("YPOSITION") || prop.Value.ToString().Contains("UNIQUEID") || prop.Value.ToString().Contains("INDOORS");
            };

            Func<string, string, string> propChange = (key, value) =>
             {
                return value.Replace("INDOORS", getLocationName(uniqueId)).Replace("UNIQUEID", uniqueId).Replace("XPOSITION", position.X.ToString()).Replace("YPOSITION", position.Y.ToString());
             };
            List<string> keys = new List<string>();

            foreach (var property in map.Properties.Where(prop => propCheck(prop)))
                keys.Add(property.Key);

            foreach (string key in keys)
                map.Properties[key] = propChange(key, map.Properties[key]);

            foreach (xTile.Layers.Layer layer in map.Layers)
            {
                keys.Clear();
                foreach (var property in layer.Properties.Where(prop => propCheck(prop)))
                    keys.Add(property.Key);

                if (layer.Properties.ContainsKey("ColorId") && colors.ContainsKey(layer.Properties["ColorId"]))
                    layer.Properties["Color"] = colors[layer.Properties["ColorId"]];


                foreach (string key in keys)
                    layer.Properties[key] = propChange(key, layer.Properties[key]);

                if (layer.Id.Contains("UNIQUEID"))
                {
                    if (layer.Properties.ContainsKey("isImageLayer"))
                    {
                        if (layer.Properties.ContainsKey("offsetx"))
                            layer.Properties["offsetx"] = (int.Parse(layer.Properties["offsetx"].ToString()) + position.X * 64).ToString();

                        if (layer.Properties.ContainsKey("offsety"))
                            layer.Properties["offsety"] = (int.Parse(layer.Properties["offsety"].ToString()) + position.Y * 64).ToString();

                        layer.Properties.Add("UseImageFrom", layer.Id);
                    }
                    layer.Id = layer.Id.Replace("UNIQUEID", uniqueId);
                }

                size = new Microsoft.Xna.Framework.Rectangle(0, 0, layer.DisplayWidth / Game1.tileSize, layer.DisplayHeight / Game1.tileSize);


                for (int x = 0; x < size.Width; x++)
                    for (int y = 0; y < size.Height; y++)
                    {
                        try
                        {
                            Location tileLocation = new Location(x, y);
                            Tile tile = layer.Tiles[tileLocation];

                            if (tile == null)
                                continue;

                            keys.Clear();

                            foreach (var property in tile.Properties.Where(prop => propCheck(prop)))
                                keys.Add(property.Key);

                            foreach (string key in keys)
                                tile.Properties[key] = propChange(key, tile.Properties[key]);
                        }
                        catch (Exception ex)
                        {
                            Monitor.Log(ex.Message + ":" + ex.StackTrace);
                            continue;
                        }
                    }
            }

            if (config.clearBuildingSpace)
            {
                foreach(xTile.Layers.Layer layer in map.Layers.Where(l => l.Id == "Back" || l.Id == "Buildings"))
                {
                    size = new Microsoft.Xna.Framework.Rectangle(0, 0, layer.DisplayWidth / Game1.tileSize, layer.DisplayHeight / Game1.tileSize);

                    for (int x = 0; x < size.Width; x++)
                        for (int y = 0; y < size.Height; y++)
                        {
                            Vector2 key = new Vector2(x + position.X, y + position.Y);
                            if (layer.Id == "Buildings" && location.Objects.ContainsKey(key))
                                location.Objects.Remove(key);

                            if (layer.Id == "Back" && location.terrainFeatures.ContainsKey(key))
                                location.terrainFeatures.Remove(key);


                            List<LargeTerrainFeature> ltfToRemove = new List<LargeTerrainFeature>();

                            foreach(var ltf in location.largeTerrainFeatures)
                                if (LuaUtils.getDistance(ltf.tilePosition.Value, key) < 4)
                                    ltfToRemove.Add(ltf);

                            foreach (var ltf in ltfToRemove)
                                location.largeTerrainFeatures.Remove(ltf);

                            if (location is Farm farm)
                            {
                                List<ResourceClump> rcToRemove = new List<ResourceClump>();

                                foreach (var rc in farm.resourceClumps)
                                    if (rc.occupiesTile(x,y))
                                        rcToRemove.Add(rc);

                                foreach (var rc in rcToRemove)
                                    farm.resourceClumps.Remove(rc);
                            }
                        }
                }
            }

            edit._mapName = location.mapPath.Value;
            edit._location = location.Name;
            var e = edit.Clone();
            e._map = map;
            SaveBuildable sav = (new SaveBuildable(edit.id, location.Name, position, uniqueId, colors, addAssetEditor(new TMXAssetEditor(e, e._map, EditType.Merge))));
            buildablesBuild.Add(sav);

            if (distribute && Game1.IsMultiplayer)
                PyNet.sendRequestToAllFarmers<bool>(buildableReceiverName, sav, null, SerializationType.JSON, -1);

            helper.Content.InvalidateCache(edit._mapName);

            try
            {
                Helper.Reflection.GetMethod(location, "reloadMap").Invoke();
            }
            catch (Exception ex)
            {
                Monitor.Log(ex.Message + ":" + ex.StackTrace);
            }
            location.updateSeasonalTileSheets();
            location.map.enableMoreMapLayers();

            if (pay)
            {
                Game1.player.Money -= edit.price;

                if (edit.buildItems.Count > 0)
                    foreach (TileShopItem tItem in edit.buildItems)
                    {
                        Item item = TMXActions.getItem(tItem.type, tItem.index, tItem.name);
                        if (item == null || !(item is StardewValley.Object))
                            continue;

                        if ((item as StardewValley.Object).bigCraftable.Value)
                            continue;

                        Game1.player.removeItemsFromInventory(item.ParentSheetIndex, tItem.stock);
                    }

            }
        }

        private bool setLocationObejcts(SaveLocation loc)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Auto;

            var inGame = Game1.getLocationFromName(loc.Name);

            if (inGame == null)
                return false;

            StringReader objReader = new StringReader(loc.Objects);
            GameLocation saved;
            using (var reader = XmlReader.Create(objReader, settings))
                saved = (GameLocation)SaveGame.locationSerializer.Deserialize(reader);

            if (saved != null)
            {
                inGame.Objects.Clear();
                if (saved.Objects.Count() > 0)
                    foreach (var obj in saved.Objects.Keys)
                        inGame.Objects.Add(obj, saved.Objects[obj]);

                inGame.terrainFeatures.Clear();
                if (saved.terrainFeatures.Count() > 0)
                    foreach (var obj in saved.terrainFeatures.Keys)
                        inGame.terrainFeatures.Add(obj, saved.terrainFeatures[obj]);

                inGame.debris.Clear();
                if (saved.debris.Count() > 0)
                    foreach (var obj in saved.debris)
                        inGame.debris.Add(obj);

                inGame.largeTerrainFeatures.Clear();
                if (saved.largeTerrainFeatures.Count > 0)
                    foreach (var obj in saved.largeTerrainFeatures)
                        inGame.largeTerrainFeatures.Add(obj);
            }

            PyTK.CustomElementHandler.SaveHandler.RebuildAll(inGame, Game1.locations);
            inGame.DayUpdate(Game1.dayOfMonth);
            return true;
        }

        private SaveLocation getLocationSaveData(GameLocation location)
        {
            PyTK.CustomElementHandler.SaveHandler.ReplaceAll(location, Game1.locations);
            string objects = "";

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.ConformanceLevel = ConformanceLevel.Auto;
            settings.CloseOutput = true;

            StringWriter objWriter = new StringWriter();
            using (var writer = XmlWriter.Create(objWriter, settings))
                SaveGame.locationSerializer.Serialize(writer, location);

            objects = objWriter.ToString();

            return new SaveLocation(location.Name, objects);
        }

        private string getAssetNameWithoutFolder(string asset)
        {
            return Path.GetFileNameWithoutExtension(asset).Split('/').Last().Split('\\').Last();
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            foreach (TMXAssetEditor editor in conditionals)
                if (e.NewLocation is GameLocation gl && gl.mapPath.Value is string mp)
                {

                    if (!getAssetNameWithoutFolder(mp).Equals(getAssetNameWithoutFolder(editor.assetName)))
                        continue;

                    if (PyTK.PyUtils.checkEventConditions(editor.conditions) is bool c)
                    {
                        bool inlocation = (editor.inLocation == null || editor.inLocation == gl.Name);
                        bool r = c && inlocation;

                        if (r != editor.lastCheck)
                        {
                            editor.lastCheck = r;

                            Helper.Content.InvalidateCache(e.NewLocation.mapPath.Value);
                            helper.Content.InvalidateCache(Game1.currentLocation.mapPath.Value);
                            try
                            {
                                Helper.Reflection.GetMethod(Game1.currentLocation, "reloadMap").Invoke();
                            }
                            catch (Exception ex)
                            {
                                Monitor.Log(ex.Message + ":" + ex.StackTrace);

                            }
                            e.NewLocation.updateSeasonalTileSheets();
                        }
                    }
                }

            foreach (string map in festivals)
                if (e.NewLocation is GameLocation gl && gl.mapPath.Value is string mp && mp.Contains(map))
                    Helper.Content.InvalidateCache(e.NewLocation.mapPath.Value);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (config.converter)
            {
                exportAllMaps();
                convert();
            }
            loadContentPacks();
            setTileActions();

            PyLua.registerType(typeof(Map), false, true);
            PyLua.registerType(typeof(TMXActions), false, false);
            PyLua.addGlobal("TMX", new TMXActions());
            new ConsoleCommand("loadmap", "Teleport to a map", (s, st) =>
            {
                if (st.Length < 3)
                    monitor.Log("Mising Parameter. Use: loadmap {Map} {x} {y}");
                else
                    Game1.player.warpFarmer(new Warp(int.Parse(st[1]), int.Parse(st[2]), st[0], int.Parse(st[1]), int.Parse(st[2]), false));
            }).register();

            new ConsoleCommand("removeNPC", "Removes an NPC", (s, st) =>
            {
                if (Game1.getCharacterFromName(st.Last()) == null)
                    Monitor.Log("Couldn't find NPC with that name!", LogLevel.Alert);
                else
                {
                    Game1.removeThisCharacterFromAllLocations(Game1.getCharacterFromName(st.Last()));
                    if (Game1.player.friendshipData.ContainsKey(st.Last()))
                        Game1.player.friendshipData.Remove(st.Last());
                    Monitor.Log(st.Last() + " was removed!", LogLevel.Info);
                }

            }).register();

            fixCompatibilities();
            harmonyFix();
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (e.IsOneSecond && Context.IsWorldReady && Game1.IsMasterGame && Game1.IsMultiplayer && Game1.otherFarmers.Values.Where(f => f.isActive() && f != Game1.player && !syncedFarmers.Contains(f)) is IEnumerable<Farmer> ef && ef.Count() is int i && i > 0)
                syncMaps(ef);
        }

        private void syncMaps(IEnumerable<Farmer> farmers)
        {
            foreach (Farmer farmer in farmers)
            {
                foreach (KeyValuePair<string, Map> map in mapsToSync)
                    PyNet.syncMap(map.Value, map.Key, farmer);

                syncedFarmers.AddOrReplace(farmer);
            }
        }

        private void fixCompatibilities()
        {
            Compatibility.CustomFarmTypes.LoadingTheMaps();
            helper.Events.GameLoop.SaveLoaded += (s, e) => Compatibility.CustomFarmTypes.fixGreenhouseWarp();
        }

        private void harmonyFix()
        {
            HarmonyInstance instance = HarmonyInstance.Create("Platonymous.TMXLoader");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void setTileActions()
        {
            PyUtils.addTileAction("ExitBuildable", (key, values, location, position, layer) =>
             {
                 Monitor.Log("WarpOut");
                 if (!buildablesExits.ContainsKey(location.Name))
                     return false;

                 Game1.player.warpFarmer(buildablesExits[location.Name]);
                 return true;
             });

            TileAction Lock = new TileAction("Lock", TMXActions.lockAction).register();
            TileAction Say = new TileAction("Say", TMXActions.sayAction).register();
            TileAction SwitchLayers = new TileAction("SwitchLayers", TMXActions.switchLayersAction).register();
            TileAction CopyLayers = new TileAction("CopyLayers", TMXActions.copyLayersAction).register();
            TileAction SpwanTreasure = new TileAction("SpawnTreasure", TMXActions.spawnTreasureAction).register();
            TileAction Confirm = new TileAction("Confirm", TMXActions.confirmAction).register();
            TileAction OpenShop = new TileAction("OpenShop", TMXActions.shopAction).register();
        }

        private GameLocation addLocation(MapEdit edit)
        {
            GameLocation location;
            Monitor.Log("Adding:" + edit.name);
            if (edit.type == "Deco")
                location = new DecoratableLocation(Path.Combine("Maps", edit.name), edit.name);
            else if (edit.type == "Cellar")
                location = new Cellar(Path.Combine("Maps", edit.name), edit.name);
            else
                location = new GameLocation(Path.Combine("Maps", edit.name), edit.name);

            if (edit._map.Properties.ContainsKey("Outdoors") && edit._map.Properties["Outdoors"] == "F")
            {
                location.IsOutdoors = false;
                try
                {
                    location.loadLights();
                }
                catch (Exception ex)
                {
                    Monitor.Log(ex.Message + ":" + ex.StackTrace);

                }
                location.IsOutdoors = false;
            }

            if (edit._map.Properties.ContainsKey("IsGreenHouse"))
                location.IsGreenhouse = true;

            if (edit._map.Properties.ContainsKey("IsStructure"))
                location.isStructure.Value = true;

            if (edit._map.Properties.ContainsKey("IsFarm"))
                location.IsFarm = true;

            if (!Game1.locations.Contains(location))
                Game1.locations.Add(location);

            location.seasonUpdate(Game1.currentSeason, true);

            return location;

        }

        private void loadContentPacks()
        {
            foreach (StardewModdingAPI.IContentPack pack in Helper.ContentPacks.GetOwned())
            {
                TMXContentPack tmxPack = pack.ReadJsonFile<TMXContentPack>("content.json");
                
                if (tmxPack.scripts.Count > 0)
                    foreach (string script in tmxPack.scripts)
                        PyLua.loadScriptFromFile(Path.Combine(pack.DirectoryPath, script), pack.Manifest.UniqueID);

                PyLua.loadScriptFromFile(Path.Combine(Helper.DirectoryPath, "sr.lua"), "Platonymous.TMXLoader.SpouseRoom");

                List<MapEdit> spouseRoomMaps = new List<MapEdit>();
                foreach (SpouseRoom room in tmxPack.spouseRooms)
                {
                    if (room.tilefix && !Overrides.NPCs.Contains(room.name))
                        Overrides.NPCs.Add(room.name);

                    if (room.file != "none")
                    {
                        spouseRoomMaps.Add(new MapEdit() { info = room.name, name = "FarmHouse1_marriage", file = room.file, position = new[] { 29, 1 } });
                        spouseRoomMaps.Add(new MapEdit() { info = room.name, name = "FarmHouse2_marriage", file = room.file, position = new[] { 35, 10 } });
                    }
                }

                foreach (MapEdit edit in spouseRoomMaps)
                {
                    string filePath = Path.Combine(pack.DirectoryPath, edit.file);
                    Map map = TMXContent.Load(edit.file, Helper, pack);
                    edit._pack = pack;
                    Helper.Content.AssetEditors.Add(new TMXAssetEditor(edit, map, EditType.SpouseRoom));
                    //  mapsToSync.AddOrReplace(edit.name, map);
                }

                foreach (TileShop shop in tmxPack.shops)
                {
                    tileShops.AddOrReplace(shop.id, shop.inventory);
                    foreach (string path in shop.portraits)
                        pack.LoadAsset<Texture2D>(path).inject(@"Portraits/" + Path.GetFileNameWithoutExtension(path));
                }

                foreach (NPCPlacement edit in tmxPack.festivalSpots)
                {
                    festivals.AddOrReplace(edit.map);
                    Helper.Content.AssetEditors.Add(new TMXAssetEditor(edit, EditType.Festival));
                    // mapsToSync.AddOrReplace(edit.map, original);
                }

                foreach (NPCPlacement edit in tmxPack.placeNPCs)
                {
                    helper.Events.GameLoop.SaveLoaded += (s, e) =>
                    {
                        if (Game1.getCharacterFromName(edit.name) == null)
                            Game1.locations.Where(gl => gl.Name == edit.map).First().addCharacter(new NPC(new AnimatedSprite("Characters\\" + edit.name, 0, 16, 32), new Vector2(edit.position[0], edit.position[1]), edit.map, 0, edit.name, edit.datable, (Dictionary<int, int[]>)null, Helper.Content.Load<Texture2D>("Portraits\\" + edit.name, ContentSource.GameContent)));
                    };
                }

                foreach (MapEdit edit in tmxPack.addMaps)
                {
                    Map map = TMXContent.Load(edit.file, Helper, pack);
                    TMXAssetEditor.editWarps(map, edit.addWarps, edit.removeWarps, map);
                    map.inject("Maps/" + edit.name);

                    edit._map = map;
                    edit._pack = pack;
                    if (edit.addLocation)
                        addedLocations.Add(edit);
                    //mapsToSync.AddOrReplace(edit.name, map);
                }

                foreach (MapEdit edit in tmxPack.replaceMaps)
                {
                    Map map = TMXContent.Load(edit.file, Helper, pack);
                    edit._pack = pack;
                    addAssetEditor(new TMXAssetEditor(edit, map, EditType.Replace));
                    // mapsToSync.AddOrReplace(edit.name, map);
                }

                foreach (MapEdit edit in tmxPack.mergeMaps)
                {
                    Map map = TMXContent.Load(edit.file, Helper, pack);
                    edit._pack = pack;
                    addAssetEditor(new TMXAssetEditor(edit, map, EditType.Merge));
                    // mapsToSync.AddOrReplace(edit.name, map);
                }

                foreach (MapEdit edit in tmxPack.onlyWarps)
                {
                    addAssetEditor(new TMXAssetEditor(edit, null, EditType.Warps));
                    // mapsToSync.AddOrReplace(edit.name, map);
                }

                foreach (BuildableEdit edit in tmxPack.buildables)
                {
                    edit._icon = pack.LoadAsset<Texture2D>(edit.iconFile);
                    edit._map = TMXContent.Load(edit.file, Helper, pack);
                    edit._pack = pack;
                    buildables.Add(edit);
                }
            }
        }

        private TMXAssetEditor addAssetEditor(TMXAssetEditor editor)
        {
            if (editor.conditions != "" || editor.inLocation != null)
                conditionals.Add(editor);

            Helper.Content.AssetEditors.Add(editor);
            return editor;
        }

        private TMXAssetEditor removeAssetEditor(TMXAssetEditor editor)
        {
            if (conditionals.Contains(editor))
                conditionals.Remove(editor);

            Helper.Content.AssetEditors.Remove(editor);
            return editor;
        }

        private void convert()
        {
            Monitor.Log("Converting..", LogLevel.Trace);
            string inPath = Path.Combine(Helper.DirectoryPath, "Converter", "IN");



            string[] directories = Directory.GetDirectories(inPath, "*.*", SearchOption.TopDirectoryOnly);

            foreach (string dir in directories)
            {
                DirectoryInfo inddir = new DirectoryInfo(dir);
                DirectoryInfo outdir = new DirectoryInfo(dir.Replace("IN", "OUT"));
                if (!outdir.Exists)
                    outdir.Create();

                foreach (FileInfo file in inddir.GetFiles())
                {
                    string importPath = Path.Combine("Converter", "IN", inddir.Name, file.Name);
                    string exportPath = Path.Combine("Converter", "OUT", inddir.Name, file.Name).Replace(".xnb", ".tmx").Replace(".tbin", ".tmx");
                    if (TMXContent.Convert(importPath, exportPath, Helper, ContentSource.ModFolder, Monitor))
                        file.Delete();
                }
            }
            Monitor.Log("..Done!", LogLevel.Trace);
        }

        private void exportAllMaps()
        {
            string exportFolderPath = Path.Combine(Helper.DirectoryPath, "Converter", "FullMapExport");
            DirectoryInfo exportFolder = new DirectoryInfo(exportFolderPath);
            DirectoryInfo modFolder = new DirectoryInfo(Helper.DirectoryPath);
            string contentPath = PyUtils.ContentPath;

            if (!exportFolder.Exists && contentPath != null && contentPath != "")
                exportFolder.Create();
            else
                return;

            string[] files = Directory.GetFiles(Path.Combine(contentPath, "Maps"), "*.xnb", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                string fileName = new FileInfo(file).Name;
                string folderName = new FileInfo(file).Directory.Name;

                if (fileName[0] == fileName.ToLower()[0])
                    continue;

                Map map = null;
                string path = Path.Combine(folderName, fileName);

                try
                {
                    map = Helper.Content.Load<Map>(path, ContentSource.GameContent);
                    map.LoadTileSheets(Game1.mapDisplayDevice);
                }
                catch (Exception ex)
                {
                    Monitor.Log(ex.Message + ":" + ex.StackTrace);
                    continue;
                }

                if (map == null)
                    continue;

                string exportPath = Path.Combine(exportFolderPath, fileName.Replace(".xnb", ".tmx"));
                TMXContent.Save(map, exportPath, true, Monitor);
            }
        }

        private bool showAll = false;
        private string set = "all";

        private void showBuildablesMenu(int position = 0, string selected = "none", bool remove = false, SaveBuildable selectedToRemove = null)
        {
            if (!Game1.IsMasterGame)
                return;

            if (buildables.Count == 0 || !Context.IsWorldReady || Game1.currentLocation.isStructure.Value)
                return;

            if (remove && buildablesBuild.Where(bb => bb.Location == Game1.currentLocation.Name).Count() < 1)
            {
                showBuildablesMenu();
                return;
            }

            BuildableEdit edit = null;
            edit = buildables.Find(bd => bd.id == selected);

           // Dictionary<string, string> colors = new Dictionary<string, string>();

            UIElement overlay = null;

            UIElement container = UIElement.GetContainer("BuildablesMenuContainer", 0, UIHelper.GetBottomRight(-64, -32, 480, 640)).WithInteractivity(draw: (b, e) =>
            {
                if (selected != "none" && !remove && overlay != null)
                {
                    Point pos = new Point(Game1.getMouseX(), Game1.getMouseY());

                    if (e.Bounds.Contains(pos))
                        return;

                    if (edit is BuildableEdit bb && bb._map is Map bmap)
                    {
                        var o = overlay.Bounds;
                        xTile.Dimensions.Rectangle v = new xTile.Dimensions.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height);
                        pos = new Point(o.X, o.Y);

                        foreach (xTile.Layers.Layer layer in bmap.Layers)
                        {
                            layer.Properties["tempOffsetx"] = pos.X;
                            layer.Properties["tempOffsety"] = pos.Y;

                            if (layer.Properties.ContainsKey("isImageLayer"))
                            {
                                Point buildPosition = new Point(pos.X + Game1.viewport.X, pos.Y + Game1.viewport.Y);

                                if (layer.Properties.ContainsKey("offsetx"))
                                    layer.Properties["tempOffsetx"] = (int.Parse(layer.Properties["offsetx"].ToString()) + buildPosition.X).ToString();

                                if (layer.Properties.ContainsKey("offsety"))
                                    layer.Properties["tempOffsety"] = (int.Parse(layer.Properties["offsety"].ToString()) + buildPosition.Y).ToString();
                            }
                        }

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawBefore") && l.Properties["DrawBefore"].ToString() == "Back"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        if (bmap.GetLayer("Back") is xTile.Layers.Layer backLayer)
                            PyMaps.drawLayer(backLayer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("Draw") && l.Properties["Draw"].ToString() == "Back"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawAbove") && l.Properties["DrawAbove"].ToString() == "Back"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawBefore") && l.Properties["DrawBefore"].ToString() == "Buildings"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        if (bmap.GetLayer("Buildings") is xTile.Layers.Layer buildingsLayer)
                            PyMaps.drawLayer(buildingsLayer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("Draw") && l.Properties["Draw"].ToString() == "Buildings"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawAbove") && l.Properties["DrawAbove"].ToString() == "Buildings"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawBefore") && l.Properties["DrawBefore"].ToString() == "Front"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        if (bmap.GetLayer("Front") is xTile.Layers.Layer frontLayer)
                            PyMaps.drawLayer(frontLayer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("Draw") && l.Properties["Draw"].ToString() == "Front"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawAbove") && l.Properties["DrawAbove"].ToString() == "Front"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawBefore") && l.Properties["DrawBefore"].ToString() == "AlwaysFront"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        if (bmap.GetLayer("AlwaysFront") is xTile.Layers.Layer alwaysfrontLayer)
                            PyMaps.drawLayer(alwaysfrontLayer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("Draw") && l.Properties["Draw"].ToString() == "AlwaysFront"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawAbove") && l.Properties["DrawAbove"].ToString() == "AlwaysFront"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);


                    }
                }
            });

            (UIHelper.BounceClose as AnimatedTexture2D).Paused = true;
            (UIHelper.BounceClose as AnimatedTexture2D).CurrentFrame = 0;
            (UIHelper.BounceClose as AnimatedTexture2D).SetSpeed(12);

            UIElement closeBtn = UIElement.GetImage(UIHelper.BounceClose, Color.White, "CloseBtn", 1, 9, UIHelper.GetTopRight(20, -40, 40)).WithInteractivity(click: (point, right, released, hold, element) =>
             {
                 if (released)
                     Game1.activeClickableMenu.exitThisMenu();
             }, hover: (point, hoverin, element) =>
              {
                 if (hoverin != element.WasHover)
                     Game1.playSound("smallSelect");

                 AnimatedTexture2D a = (element.Theme as AnimatedTexture2D);

                 if (hoverin)
                     a.Paused = false;
                 else
                 {
                     a.Paused = true;
                     a.CurrentFrame = 0;
                 }

             });
            container.Add(closeBtn);


            container.WithInteractivity(keys: (key, released, e) =>
             {
                 if (key == Microsoft.Xna.Framework.Input.Keys.F2)
                 {
                     showBuildablesMenu();
                     return;
                 }

                 if (key == Microsoft.Xna.Framework.Input.Keys.F3)
                 {
                     showBuildablesMenu(0, "none", !remove, null);
                     return;
                 }

                 if (key == Microsoft.Xna.Framework.Input.Keys.Back && buildablesBuild.Count > 0)
                 {
                     var last = buildablesBuild.Where(l => l.Location == Game1.currentLocation.Name).Last();

                     removeSavedBuildable(last,true,true);
                     showBuildablesMenu(position, selected, remove, selectedToRemove);
                     return;
                 }

                 if (key == Microsoft.Xna.Framework.Input.Keys.Delete)
                     if (!remove)
                         showBuildablesMenu(position);
                     else if (selectedToRemove != null)
                     {
                         var toRemove = selectedToRemove;

                         removeSavedBuildable(toRemove,true,true);
                         showBuildablesMenu(position, selected, remove, null);
                     }
             }
            );

            UIElement pickBuild = UIElement.GetImage(UIHelper.TabTheme, Color.White, "buildTab", (remove ? 0.6f : 0.9f), -1, UIHelper.GetTopLeft(20, -40, 120, 60)).AsTiledBox(16, true).WithInteractivity(hover: (point, hoverin, element) => {
                if (remove)
                {
                    (element.GetElementById("buildText") as UITextElement).TextColor = hoverin ? Color.Cyan * 0.8f : Color.Black * 0.8f;

                    if (hoverin != element.WasHover)
                        Game1.playSound("smallSelect");
                }
            }, click: (point, right, release, hold, element) => {

                if (release && !right && remove)
                    showBuildablesMenu();
            });
            UITextElement textBuild = new UITextElement("Build", Game1.smallFont, Color.Black * 0.8f, 0.5f, 1, "buildText", 0, UIHelper.GetCentered());
            pickBuild.Add(textBuild);
            container.Add(pickBuild);

            UIElement pickRemove = UIElement.GetImage(UIHelper.TabTheme, Color.White, "removeTab", (!remove ? 0.6f : 9f), -1, UIHelper.GetTopLeft(160, -40, 120, 60)).AsTiledBox(16, true).WithInteractivity(hover: (point, hoverin, element) => {
                if (!remove)
                {
                    (element.GetElementById("removeText") as UITextElement).TextColor = hoverin ? Color.Orange * 0.8f : Color.Black * 0.8f;

                    if (hoverin != element.WasHover)
                        Game1.playSound("smallSelect");
                }
            }, click: (point, right, release, hold, element) => {

                if (release && !right && !remove)
                    showBuildablesMenu(remove: true);
            }); ;
            UITextElement textRemove = new UITextElement("Remove", Game1.smallFont, Color.Black * 0.8f, 0.5f, 1, "removeText", 0, UIHelper.GetCentered());
            pickRemove.Add(textRemove);
            container.Add(pickRemove);

            UIElement back = UIElement.GetImage(UIHelper.DarkTheme, Color.White * 0.9f, "BMCBack").AsTiledBox(16, true);
            container.Add(back);
            UIElement listContainer = UIElement.GetContainer("BMCListContainer", 0, UIHelper.GetCentered(0, 0, 450, 620));
            back.Add(listContainer);
            UIElementList list = new UIElementList("BMCList", startPosition: position, margin: 10, elementPositioner: UIHelper.GetFixed(0, 0, 1f, 200));
            listContainer.Add(list);
            Texture2D entryBack = PyDraw.getBorderedRectangle(18, 18, Color.White * 0.1f, 4, Color.White * 0.5f);
            Texture2D entryBackHover = PyDraw.getBorderedRectangle(18, 18, Color.White * 0.1f, 4, Color.LightCyan);
            Texture2D entryBackSelected = PyDraw.getBorderedRectangle(18, 18, Color.White * 0.1f, 4, Color.Orange);
            Texture2D entryBackSelectedDelete = PyDraw.getBorderedRectangle(18, 18, Color.White * 0.1f, 4, Color.Red);

            UIElement fullContainer = UIElement.GetContainer("BMView");

            if (selected != null && !remove && edit is BuildableEdit be && be._map is Map map)
            {
                UIElement colorPicker = null;

                Texture2D pattern = PyDraw.getPattern(map.DisplayWidth, map.DisplayHeight, PyDraw.getRectangle(32, 32, Color.White * 0.2f), PyDraw.getRectangle(32, 32, Color.White * 0.4f));
                overlay = UIElement.GetImage(pattern, Color.Cyan, "BMPattern", 1, -1, (t, p) =>
                {
                    if (colorPicker != null && colorPicker.Bounds.Contains(Game1.getMousePosition()))
                        return new Microsoft.Xna.Framework.Rectangle(50, 50, pattern.Width, pattern.Height);


                    Point pt = Game1.GlobalToLocal((Game1.currentLocation.getTileAtMousePosition() * Game1.tileSize)).toPoint();
                    return new Microsoft.Xna.Framework.Rectangle(pt.X, pt.Y, pattern.Width, pattern.Height);
                }).WithInteractivity(update: (t, e) =>
                {
                    Point pos = new Point(Game1.getMouseX(), Game1.getMouseY());

                    e.Visible = !container.Bounds.Contains(pos);

                    e.UpdateBounds();
                }, click: (point, right, release, hold, element) =>
                {
                    if (selected == "none" || edit == null || listContainer.Bounds.Contains(Game1.getMousePosition()) || (colorPicker != null && colorPicker.Bounds.Contains(Game1.getMousePosition())))
                        return;

                    if (right)
                    {
                        showBuildablesMenu(list.Position);
                        return;
                    }
                    if (release)
                    {
                        try
                        {
                            Dictionary<string, string> colors = new Dictionary<string, string>();
                            foreach (xTile.Layers.Layer layer in map.Layers)
                            {
                                try
                                {
                                    if (layer.Properties.ContainsKey("tempOffsetx"))
                                        layer.Properties.Remove("tempOffsetx");

                                    if (layer.Properties.ContainsKey("tempOffsety"))
                                        layer.Properties.Remove("tempOffsety");

                                    if (layer.Properties.ContainsKey("ColorId") && layer.Properties.ContainsKey("Color"))
                                        colors.AddOrReplace(layer.Properties["ColorId"].ToString(), layer.Properties["Color"].ToString());
                                }
                                catch (Exception ex)
                                {
                                    Monitor.Log(ex.Message + ":" + ex.StackTrace, LogLevel.Error);

                                }
                            }

                            Point pos = Game1.currentLocation.getTileAtMousePosition().toPoint();

                            buildBuildableEdit(true, edit, Game1.currentLocation, pos, colors);

                            Game1.playSound("drumkit0");

                            showBuildablesMenu(list.Position);
                        }
                        catch(Exception ex)
                        {
                            Monitor.Log(ex.Message + ":" + ex.StackTrace, LogLevel.Error);
                        }
                    }

                });
                try
                {
                    List<string> cLayers = new List<string>();
                    List<Color> cColors = new List<Color>();
                    foreach (xTile.Layers.Layer layer in map.Layers)
                        if (layer.Properties.ContainsKey("isImageLayer") && layer.Properties.ContainsKey("Color") && layer.Properties.ContainsKey("ColorId"))
                        {
                            string[] c = layer.Properties["Color"].ToString().Split(' ');
                                if (!cLayers.Contains(layer.Properties["ColorId"].ToString()))
                                {
                                    cLayers.Add(layer.Properties["ColorId"].ToString());
                                    cColors.Add(new Color(int.Parse(c[0]), int.Parse(c[1]), int.Parse(c[2]), c.Length > 3 ? int.Parse(c[3]) : 255));
                                }
                            
                        }

                    if (cColors.Count > 0)
                    {
                        colorPicker = PyTK.PlatoUI.UIPresets.GetColorPicker(cColors, (index, color) =>
                        {
                            foreach (xTile.Layers.Layer l in map.Layers.Where(ly => ly.Properties != null && ly.Properties.ContainsKey("ColorId") && ly.Properties.ContainsKey("Color") && ly.Properties["ColorId"] == cLayers[index]))
                                l.Properties["Color"] = color.R + " " + color.G + " " + color.B + " " + color.A;
                        }, true);
                        colorPicker.Positioner = UIHelper.GetBottomLeft(10, -10, colorPicker.Bounds.Width + 20, colorPicker.Bounds.Height + 20);
                        colorPicker.Z = 99;
                        fullContainer.Add(colorPicker);
                    }
                }
                catch (Exception e)
                {
                    Monitor.Log(e.Message + ":" + e.StackTrace, LogLevel.Error);
                }
            }

            UIElement listDown = UIElement.GetImage(UIHelper.ArrowDown, Color.White, "Down", 0.5f, 1, UIHelper.GetBottomLeft(-48, 0, 32)).WithInteractivity(hover:(point, hoverin,element) =>
            {
                element.Opacity = hoverin ? 1f : 0.5f;

                if (hoverin != element.WasHover)
                    Game1.playSound("smallSelect");
            }, click:(point, right,release,hold,element) =>
            {
                if (release)
                {
                    Game1.playSound("smallSelect");
                    list.NextPosition();
                }
            });

            UIElement listUp = UIElement.GetImage(UIHelper.ArrowUp, Color.White, "Up", 0.5f, 1, UIHelper.GetBottomLeft(-48, -64, 32)).WithInteractivity(hover: (point, hoverin, element) =>
            {
                element.Opacity = hoverin ? 1f : 0.5f;

                if (hoverin != element.WasHover)
                    Game1.playSound("smallSelect");
            }, click: (point, right, release, hold, element) =>
            {
                if (release)
                {
                    Game1.playSound("smallSelect");
                    list.PreviousPosition();
                }
            }); ;

            back.Add(listDown);
            back.Add(listUp);


            if (overlay != null)
                fullContainer.Add(overlay);

            fullContainer.Add(container);

            if (remove)
            {
                var rlist = new List<SaveBuildable>(buildablesBuild.Where(bbe => bbe.Location == Game1.currentLocation.Name || showAll));
                rlist.Reverse();

                foreach (var bb in rlist)
                {
                    var b = buildables.Find(bd => bd.id == bb.Id);
                    if (b == null)
                        continue;

                    if (set != "all" && b.set != set)
                        continue;

                    UIElement buildableEntry = UIElement.GetImage(selectedToRemove == bb ? entryBackSelected : entryBack, Color.White, b.id, 1f).AsTiledBox(6, true).WithInteractivity(hover: (p, hoverin, element) =>
                    {
                        if (selectedToRemove != bb)
                            element.Theme = hoverin ? entryBackHover : entryBack;
                        else
                            element.Theme = hoverin ? entryBackSelectedDelete : entryBackSelected;

                        if (hoverin != element.WasHover)
                            Game1.playSound("smallSelect");

                    }, click: (p, right, release, hold, element) =>
                    {
                        if(release && !right && selectedToRemove == bb)
                        {
                            var toRemove = selectedToRemove;

                            removeSavedBuildable(toRemove,true,true);
                            showBuildablesMenu(position, selected, remove, null);
                        }
                        else if (release && !right)
                            showBuildablesMenu(list.Position,"none", true,bb);
                        else if (right)
                            showBuildablesMenu(list.Position, "none", true, null);
                    });
                    list.Add(buildableEntry);
                    UIElement buildableImage = UIElement.GetImage(b._icon, Color.White, b.id + "_icon", positioner: UIHelper.GetTopLeft(10, 10, 0, 180));
                    buildableEntry.Add(buildableImage);

                    UITextElement buildablePrice = new UITextElement("X " + bb.Position[0] + "  Y " + bb.Position[1], Game1.smallFont,  Color.White, 0.75f, 1, b.id + "_position", positioner: UIHelper.GetBottomRight(-10, -10));
                    buildableEntry.Add(buildablePrice);

                    UITextElement buildableName = new UITextElement(b.name, Game1.smallFont, Color.White, 1f, 1, b.id + "_name", positioner: UIHelper.GetBottomRight(-10, -40));
                    buildableEntry.Add(buildableName);
                }
            }
            if (!remove)
            {
                foreach (var b in buildables.Where(c => PyUtils.checkEventConditions(c.conditions, Game1.currentLocation) && (set == "all" || set == c.set)))
                {
                    bool affordable = Game1.player.Money >= b.price;
                    bool buildable = true;

                    string itemString = "";

                    foreach(TileShopItem tItem in b.buildItems)
                    {
                        Item item = TMXActions.getItem(tItem.type, tItem.index, tItem.name);
                        if (item == null || !(item is StardewValley.Object))
                            continue;

                        if ((item as StardewValley.Object).bigCraftable.Value)
                            continue;

                        itemString += "  " + tItem.stock + " " + item.Name;
                        if(!Game1.player.hasItemInInventory(item.ParentSheetIndex,tItem.stock))
                            buildable = false;
                    }

                    UIElement buildableEntry = UIElement.GetImage(b.id == selected ? entryBackSelected : entryBack, Color.White, b.id, affordable && buildable ? 1f : 0.7f).AsTiledBox(6, true).WithInteractivity(hover: (p, hoverin, element) =>
                    {
                        if (element.Id != selected)
                            element.Theme = hoverin ? entryBackHover : entryBack;

                        if (hoverin != element.WasHover)
                            Game1.playSound("smallSelect");

                    }, click: (p, right, release, hold, element) =>
                    {
                        try
                        {
                            if (release && !right && element.Opacity == 1f)
                                showBuildablesMenu(list.Position, element.Id, false);
                        }
                        catch(Exception e)
                        {
                            Monitor.Log(e.Message + ":" + e.StackTrace, LogLevel.Error);
                        }
                    });
                    list.Add(buildableEntry);
                    UIElement buildableImage = UIElement.GetImage(b._icon, Color.White, b.id + "_icon", positioner: UIHelper.GetTopLeft(10, 10, 0, 180));
                    buildableEntry.Add(buildableImage);

                    UITextElement buildableItems = new UITextElement((itemString == "" ? " ---" : itemString), Game1.smallFont, buildable ? Color.White : Color.Red, 0.5f, 1, b.id + "_price", positioner: UIHelper.GetBottomRight(-10, -10));
                    buildableEntry.Add(buildableItems);

                    UITextElement buildablePrice = new UITextElement(b.price + "g", Game1.smallFont, affordable ? Color.White : Color.Red, 0.75f, 1, b.id + "_price", positioner: UIHelper.GetBottomRight(-10, -40));
                    buildableEntry.Add(buildablePrice);

                    UITextElement buildableName = new UITextElement(b.name, Game1.smallFont, Color.White, 1f, 1, b.id + "_name", positioner: UIHelper.GetBottomRight(-10, -70));
                    buildableEntry.Add(buildableName);
                }
            }
            
            Game1.activeClickableMenu = new PlatoUIMenu("BuildablesMenu", fullContainer);
        }

    }
}

