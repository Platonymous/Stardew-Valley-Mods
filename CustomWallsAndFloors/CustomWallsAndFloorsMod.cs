using Microsoft.Xna.Framework.Graphics;
using PyTK;
using PyTK.Types;
using PyTK.Extensions;
using StardewModdingAPI;
using System.IO;
using StardewValley;
using Harmony;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Linq;
using StardewModdingAPI.Events;

namespace CustomWallsAndFloors
{
    public class CustomWallsAndFloorsMod : Mod
    {
        public static IMonitor monitor;
        internal static IModHelper helper;
        public static bool skip = false;

        public override void Entry(IModHelper helper)
        {
            CustomWallsAndFloorsMod.helper = helper;
            loadContentPacks();
            monitor = Monitor;

            HarmonyInstance harmony = HarmonyInstance.Create("Platonymous.CustomWallsAndFloors");
            harmony.Patch(PyUtils.getTypeSDV("Objects.Wallpaper").GetMethod("placementAction"), new HarmonyMethod(typeof(CustomWallsAndFloorsMod), "Prefix_placement"),null);
            harmony.Patch(PyUtils.getTypeSDV("GameLocation").GetMethod("setMapTileIndex"), new HarmonyMethod(typeof(CustomWallsAndFloorsMod), "Prefix_setMapTile"), null);

            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saving += OnSaving;
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            if(Game1.IsMasterGame)
                saveRoomData();
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Game1.IsMasterGame)
                return;

            CustomWallpaper.savFile = helper.Data.ReadSaveData<SaveFile>("Platonymous.CustomWallsAndFloors.Data");

            if (CustomWallpaper.savFile == null)
                CustomWallpaper.savFile = new SaveFile();

            foreach (SavedRoom room in CustomWallpaper.savFile.rooms.Where(r => r.Id == Game1.player.UniqueMultiplayerID))
            {
                try
                {
                    DecoratableLocation dec = (DecoratableLocation)Game1.getLocationFromName(room.Location);
                    if (room.Walls != "na")
                    {
                        CustomWallpaper walls = new CustomWallpaper(room.Walls, room.WallsNr, false);
                        if (CustomWallpaper.Walls.ContainsKey(room.Walls))
                        {
                            walls.Texture = CustomWallpaper.Floors[room.Walls];
                            walls.setChangeEventsAfterLoad(dec, room.Room);
                        }
                    }

                    if (room.Floors != "na")
                    {
                        CustomWallpaper floors = new CustomWallpaper(room.Floors, room.FloorsNr, true);
                        if (CustomWallpaper.Floors.ContainsKey(room.Floors))
                        {
                            floors.Texture = CustomWallpaper.Floors[room.Floors];
                            floors.setChangeEventsAfterLoad(dec, room.Room);
                        }
                    }
                }
                catch
                {

                }
            }
        }

        public static void saveRoomData()
        {
            helper.Data.WriteSaveData<SaveFile>("Platonymous.CustomWallsAndFloors.Data", CustomWallpaper.savFile);
        }

        public static void Prefix_setMapTile(ref GameLocation __instance, int tileX, int tileY, ref int index, string layer, int whichTileSheet = 0)
        {
            if (skip || !Game1.IsMasterGame)
                return;

            try
            {
                if (__instance.map.GetLayer(layer).Tiles[tileX, tileY] != null && __instance.map.GetLayer(layer).Tiles[tileX, tileY].TileSheet.Id.Contains("zCWF.Floors."))
                    index -= 336;
            }
            catch
            {
            }
        }

        public static void Prefix_placement(Wallpaper __instance, GameLocation location, int x, int y, Farmer who = null)
        {
            if (skip || __instance is CustomWallpaper || !Game1.IsMasterGame)
                return;

            skip = true;

            if (who == null)
                who = Game1.player;
            if (who.currentLocation is DecoratableLocation dec)
            {
                if (__instance.isFloor.Value)
                {
                    CustomWallpaper.floorReset = (r, w) => CustomWallpaper.Floor_OnChange1(r, w, dec);
                    dec.floor.OnChange += CustomWallpaper.floorReset;
                }
                else
                {
                    CustomWallpaper.wallReset = (r, w) => CustomWallpaper.WallPaper_OnChange1(r, w, dec);
                    dec.wallPaper.OnChange += CustomWallpaper.wallReset;
                }

                __instance.placementAction(location, x, y, who);
            }
            PyTK.PyUtils.setDelayedAction(500, () => skip = false);
        }

        private void loadContentPacks()
        {
            foreach (StardewModdingAPI.IContentPack pack in Helper.ContentPacks.GetOwned())
            {
                Animations animations = null;

                if (File.Exists(Path.Combine(pack.DirectoryPath, "settings.json")))
                    animations = pack.ReadJsonFile<Animations>("settings.json");

                if (File.Exists(Path.Combine(pack.DirectoryPath, "walls.png")) && !CustomWallpaper.Walls.ContainsKey(pack.Manifest.UniqueID))
                {
                    Texture2D wallTexture = pack.LoadAsset<Texture2D>("walls.png");

                    if (animations != null)
                        wallTexture = AnimatedTexture.FromTexture(wallTexture, animations.AnimatedTiles);

                    CustomWallpaper.Walls.Add(pack.Manifest.UniqueID, wallTexture);
                    string key = Path.Combine(pack.Manifest.UniqueID, "walls");
                    wallTexture.inject(key);

                    int walls = (wallTexture.Width / 16) * (wallTexture.Height / 48);
                    for (int i = 0; i < walls; i++)
                    {
                        if (wallTexture is AnimatedTexture awall && awall.AnimatedTiles.Find(t => !t.Floor && i > t.Index && i < t.Index + t.Frames) != null)
                            continue;

                        InventoryItem inv = new InventoryItem(new CustomWallpaper(pack.Manifest.UniqueID, i, false), 0);
                        inv.addToWallpaperCatalogue();
                    }
                }

                if (File.Exists(Path.Combine(pack.DirectoryPath, "floors.png")) && !CustomWallpaper.Floors.ContainsKey(pack.Manifest.UniqueID))
                {
                    Texture2D floorTexture = pack.LoadAsset<Texture2D>("floors.png");

                    if (animations != null)
                        floorTexture = AnimatedTexture.FromTexture(floorTexture, animations.AnimatedTiles);

                    CustomWallpaper.Floors.Add(pack.Manifest.UniqueID, floorTexture);
                    string key = Path.Combine(pack.Manifest.UniqueID, "floors");
                    floorTexture.inject(key);

                    int floors = (floorTexture.Width / 32) * (floorTexture.Height / 32);
                    for (int i = 0; i < floors; i++)
                    {
                        if (floorTexture is AnimatedTexture awall && awall.AnimatedTiles.Find(t => t.Floor && i > t.Index && i < t.Index + t.Frames) != null)
                            continue;

                        InventoryItem inv = new InventoryItem(new CustomWallpaper(pack.Manifest.UniqueID, i, true), 0);
                        inv.addToWallpaperCatalogue();
                    }
                }
            }
        }


    }
}
