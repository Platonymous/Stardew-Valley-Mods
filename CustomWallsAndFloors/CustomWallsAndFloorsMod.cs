using Microsoft.Xna.Framework.Graphics;
using PyTK;
using PyTK.Types;
using PyTK.Extensions;
using StardewModdingAPI;
using System.IO;
using xTile.Tiles;
using StardewValley;
using xTile;
using Harmony;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Linq;

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


            if (File.Exists(Path.Combine(Helper.DirectoryPath, "sav.json")))
                CustomWallpaper.savFile = helper.ReadJsonFile<SaveFile>(Path.Combine(Helper.DirectoryPath, "sav.json"));

            HarmonyInstance harmony = HarmonyInstance.Create("Platonymous.CustomWallsAndFloors");
            harmony.Patch(PyUtils.getTypeSDV("Objects.Wallpaper").GetMethod("placementAction"), new HarmonyMethod(typeof(CustomWallsAndFloorsMod), "Prefix_placement"),null);
            harmony.Patch(PyUtils.getTypeSDV("GameLocation").GetMethod("setMapTileIndex"), new HarmonyMethod(typeof(CustomWallsAndFloorsMod), "Prefix_setMapTile"), null);

            StardewModdingAPI.Events.SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            StardewModdingAPI.Events.SaveEvents.BeforeSave += SaveEvents_BeforeSave;
        }

        private void SaveEvents_BeforeSave(object sender, System.EventArgs e)
        {
            saveRoomData();
        }

        private void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {
            foreach(SavedRoom room in CustomWallpaper.savFile.rooms.Where(r => r.Id == Game1.player.UniqueMultiplayerID))
            {
                DecoratableLocation dec = (DecoratableLocation) Game1.getLocationFromName(room.Location);
                if(room.Walls != "na")
                {
                    CustomWallpaper walls = new CustomWallpaper(room.Walls, room.WallsNr, false);
                    walls.setChangeEventsAfterLoad(dec, room.Room);
                }

                if (room.Floors != "na")
                {
                    CustomWallpaper floors = new CustomWallpaper(room.Floors, room.FloorsNr, true);
                    floors.setChangeEventsAfterLoad(dec, room.Room);
                }

            }
        }

        public static void saveRoomData()
        {
            helper.WriteJsonFile(Path.Combine(helper.DirectoryPath, "sav.json"), CustomWallpaper.savFile);
        }

        public static void Prefix_setMapTile(ref GameLocation __instance, int tileX, int tileY, ref int index, string layer, int whichTileSheet = 0)
        {
            if (skip)
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
            if (skip || __instance is CustomWallpaper)
                return;

            skip = true;

            if (who == null)
                who = Game1.player;
            if (who.currentLocation is DecoratableLocation dec)
            {
                if (__instance.isFloor)
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
            foreach (StardewModdingAPI.IContentPack pack in Helper.GetContentPacks())
            {
                if (File.Exists(Path.Combine(pack.DirectoryPath, "walls.png")) && !CustomWallpaper.Walls.ContainsKey(pack.Manifest.UniqueID))
                {
                    Texture2D wallTexture = pack.LoadAsset<Texture2D>("walls.png");
                    CustomWallpaper.Walls.Add(pack.Manifest.UniqueID, wallTexture);
                    string key = Path.Combine(pack.Manifest.UniqueID, "walls");
                    wallTexture.inject(key);
                    int walls = (wallTexture.Width / 16) * (wallTexture.Height / 48);
                    for (int i = 0; i < walls; i++)
                    {
                        InventoryItem inv = new InventoryItem(new CustomWallpaper(pack.Manifest.UniqueID, i, false), 100);
                        inv.addToWallpaperCatalogue();
                    }
                }

                if (File.Exists(Path.Combine(pack.DirectoryPath, "floors.png")) && !CustomWallpaper.Floors.ContainsKey(pack.Manifest.UniqueID))
                {
                    Texture2D floorTexture = pack.LoadAsset<Texture2D>("floors.png");
                    CustomWallpaper.Floors.Add(pack.Manifest.UniqueID, floorTexture);
                    string key = Path.Combine(pack.Manifest.UniqueID, "floors");
                    floorTexture.inject(key);

                    int floors = (floorTexture.Width / 32) * (floorTexture.Height / 32);
                    for (int i = 0; i < floors; i++)
                    {
                        InventoryItem inv = new InventoryItem(new CustomWallpaper(pack.Manifest.UniqueID, i, true), 100);
                        inv.addToWallpaperCatalogue();
                    }
                }
            }
        }


    }
}
