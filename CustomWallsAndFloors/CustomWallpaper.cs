using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;
using StardewValley.Locations;
using xTile;
using xTile.Tiles;
using System.IO;
using System;
using System.Linq;

namespace CustomWallsAndFloors
{
    public class CustomWallpaper : Wallpaper, ICustomObject
    {
        public static SaveFile savFile = new SaveFile();
        public static Dictionary<string, Texture2D> Walls = new Dictionary<string, Texture2D>();
        public static Dictionary<string, Texture2D> Floors = new Dictionary<string, Texture2D>();
        private static readonly Rectangle wallpaperContainerRect = new Rectangle(193, 496, 16, 16);
        private static readonly Rectangle floorContainerRect = new Rectangle(209, 496, 16, 16);

        public DecorationFacade.ChangeEvent wallChange;
        public static DecorationFacade.ChangeEvent wallReset;
        public DecorationFacade.ChangeEvent floorChange;
        public static DecorationFacade.ChangeEvent floorReset;

        public Texture2D Texture;

        public CustomWallpaper(string id, int which, bool floor = false)
            : base(which, floor)
        {
            name = id;
        }

        public override Item getOne()
        {
            return new CustomWallpaper(name, parentSheetIndex, isFloor);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            if (Texture == null)
            {
                if (!isFloor && Walls.ContainsKey(name))
                    Texture = Walls[name];

                if (isFloor && Floors.ContainsKey(name))
                    Texture = Floors[name];

                if (Texture != null)
                {
                    Rectangle sr = isFloor ? Game1.getSourceRectForStandardTileSheet(Texture, parentSheetIndex, 32, 32) : Game1.getSourceRectForStandardTileSheet(Texture, parentSheetIndex, 16, 48);

                    if (isFloor)
                    {
                        sr.Width = 28;
                        sr.Height = 26;
                    }
                    else
                        sr.Height = 28;

                    sourceRect.Value = sr;
                }
            }

            if (Texture == null)
            {
                if (wallpaperTexture == null)
                    wallpaperTexture = Game1.content.Load<Texture2D>("Maps\\walls_and_floors");

                Texture = wallpaperTexture;
                sourceRect.Value = isFloor ? new Rectangle(1 % 8 * 32, 336 + 1 / 8 * 32, 28, 26) : new Rectangle(1 % 16 * 16, 1 / 16 * 48 + 8, 16, 28);
            }

            if (isFloor)
            {
                spriteBatch.Draw(wallpaperTexture, location + new Vector2(32f, 32f), new Rectangle?(floorContainerRect), color * transparency, 0.0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
                spriteBatch.Draw(Texture, location + new Vector2(32f, 30f), new Rectangle?((Rectangle)((NetFieldBase<Rectangle, NetRectangle>)this.sourceRect)), color * transparency, 0.0f, new Vector2(14f, 13f), 2f * scaleSize, SpriteEffects.None, layerDepth + 1f / 1000f);
            }
            else
            {
                spriteBatch.Draw(wallpaperTexture, location + new Vector2(32f, 32f), new Rectangle?(wallpaperContainerRect), color * transparency, 0.0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
                spriteBatch.Draw(Texture, location + new Vector2(32f, 32f), new Rectangle?((Rectangle)((NetFieldBase<Rectangle, NetRectangle>)this.sourceRect)), color * transparency, 0.0f, new Vector2(8f, 14f), 2f * scaleSize, SpriteEffects.None, layerDepth + 1f / 1000f);
            }
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            CustomWallsAndFloorsMod.skip = true;

            initTilesheet(ref who.currentLocation.map);

            if (who == null)
                who = Game1.player;
            if (who.currentLocation is DecoratableLocation dec)
            {
                if (isFloor)
                {
                    floorChange = (r, w) => Floor_OnChange(r, w, dec);
                    base.placementAction(location, x, y, who);
                    dec.floor.OnChange += floorChange;
                }
                else
                {
                    wallChange = (r, w) => WallPaper_OnChange(r, w, dec);
                    base.placementAction(location, x, y, who);
                    dec.wallPaper.OnChange += wallChange;
                }
            }

            PyTK.PyUtils.setDelayedAction(500, () => CustomWallsAndFloorsMod.skip = false);
            return base.placementAction(location, x, y, who);
        }

        public void setChangeEventsAfterLoad(DecoratableLocation dec, int whichRoom)
        {
            CustomWallsAndFloorsMod.skip = true;

            if (isFloor)
            {
                floorChange = (r, w) => Floor_OnChange(r, w, dec);
                dec.setFloor((int)((NetFieldBase<int, NetInt>)this.parentSheetIndex), whichRoom, true);
                dec.floor.OnChange += floorChange;
            }
            else
            {
                wallChange = (r, w) => WallPaper_OnChange(r, w, dec);
                dec.setWallpaper((int)((NetFieldBase<int, NetInt>)this.parentSheetIndex), whichRoom, true);
                dec.wallPaper.OnChange += wallChange;
            }

            PyTK.PyUtils.setDelayedAction(500, () => CustomWallsAndFloorsMod.skip = false);

            if(isFloor)
                dec.setFloor((int)((NetFieldBase<int, NetInt>)this.parentSheetIndex), whichRoom, true);
            else
                dec.setWallpaper((int)((NetFieldBase<int, NetInt>)this.parentSheetIndex), whichRoom, true);
        }


        public static void WallPaper_OnChange1(int whichRoom, int which, DecoratableLocation dec)
        {
            setWallsTilesheet(whichRoom, which, dec.map.TileSheets[2], dec);

            if (savFile.rooms.Find(sr => sr.Location == dec.Name && sr.Room == whichRoom) is SavedRoom sav)
                sav.Walls = "na";

            dec.wallPaper.OnChange -= wallReset;
        }

        private void WallPaper_OnChange(int whichRoom, int which, DecoratableLocation dec)
        {
            TileSheet tilesheet = initTilesheet(ref dec.map);
            AnimatedTile anim = null;

            if (Texture is AnimatedTexture && (Texture as AnimatedTexture).AnimatedTiles.Find(a => a.Index == which && a.Floor == isFloor) is AnimatedTile aList)
                anim = aList;

            setWallsTilesheet(whichRoom, which, tilesheet, dec, anim);

            if (savFile.rooms.Find(sr => sr.Id == Game1.player.UniqueMultiplayerID && sr.Location == dec.Name && sr.Room == whichRoom) is SavedRoom sav)
            {
                sav.Walls = name;
                sav.WallsNr = parentSheetIndex;
            }
            else
                savFile.rooms.Add(new SavedRoom(Game1.player.UniqueMultiplayerID, dec.Name, whichRoom, "na", name, parentSheetIndex, 0));

            dec.wallPaper.OnChange -= wallChange;
        }

        public static void Floor_OnChange1(int whichRoom, int which, DecoratableLocation dec)
        {
            setFloorsTilesheet(whichRoom, which, dec.map.TileSheets[2], dec);

            if (savFile.rooms.Find(sr => sr.Location == dec.Name && sr.Room == whichRoom) is SavedRoom sav)
                sav.Floors = "na";

            dec.floor.OnChange -= floorReset;
        }

        private void Floor_OnChange(int whichRoom, int which, DecoratableLocation dec)
        {
            TileSheet tilesheet = initTilesheet(ref dec.map);
            AnimatedTile anim = null;

            if (Texture is AnimatedTexture && (Texture as AnimatedTexture).AnimatedTiles.Find(a => a.Index == which && a.Floor == isFloor) is AnimatedTile aList)
                anim = aList;

            setFloorsTilesheet(whichRoom, which, tilesheet, dec, anim);

            if (savFile.rooms.Find(sr => sr.Id == Game1.player.UniqueMultiplayerID && sr.Location == dec.Name && sr.Room == whichRoom) is SavedRoom sav)
            {
                sav.Floors = name;
                sav.FloorsNr = parentSheetIndex;
            }
            else
                savFile.rooms.Add(new SavedRoom(Game1.player.UniqueMultiplayerID, dec.Name, whichRoom, name, "na", 0, parentSheetIndex));

            dec.floor.OnChange -= floorChange;
        }

        public static void setWallsTilesheet(int whichRoom, int which, TileSheet tilesheet, DecoratableLocation dec, AnimatedTile animations = null)
        {
            List<Rectangle> walls = dec.getWalls();
            if (whichRoom == -1)
            {
                foreach (Rectangle rectangle in walls)
                {
                    for (int x = rectangle.X; x < rectangle.Right; ++x)
                    {
                        setMapTilesheet(dec.map, tilesheet, x, rectangle.Y, "Back", 0, 0, animations);
                        setMapTilesheet(dec.map, tilesheet, x, rectangle.Y + 1, "Back", 0, 0, animations);
                        if (rectangle.Height >= 3)
                        {
                            if (dec.map.GetLayer("Buildings").Tiles[x, rectangle.Y + 2].TileSheet.Equals(dec.map.TileSheets[2]) || dec.map.GetLayer("Buildings").Tiles[x, rectangle.Y + 2].TileSheet.Id.StartsWith("zCWF"))
                                setMapTilesheet(dec.map, tilesheet, x, rectangle.Y + 2, "Buildings", 0, 0, animations);
                            else
                                setMapTilesheet(dec.map, tilesheet, x, rectangle.Y + 2, "Back", 0, 0, animations);
                        }
                    }
                }
            }
            else
            {
                if (walls.Count <= whichRoom)
                    return;
                Rectangle rectangle = walls[whichRoom];
                for (int x = rectangle.X; x < rectangle.Right; ++x)
                {
                    setMapTilesheet(dec.map, tilesheet, x, rectangle.Y, "Back", 0, 0, animations);
                    setMapTilesheet(dec.map, tilesheet, x, rectangle.Y + 1, "Back", 0, 0, animations);
                    if (rectangle.Height >= 3)
                    {
                        if (dec.map.GetLayer("Buildings").Tiles[x, rectangle.Y + 2].TileSheet.Equals(dec.map.TileSheets[2]) || dec.map.GetLayer("Buildings").Tiles[x, rectangle.Y + 2].TileSheet.Id.StartsWith("zCWF"))
                            setMapTilesheet(dec.map, tilesheet, x, rectangle.Y + 2, "Buildings", 0, 0, animations);
                        else
                            setMapTilesheet(dec.map, tilesheet, x, rectangle.Y + 2, "Back", 0, 0, animations);
                    }
                }
            }


            PyTK.PyUtils.setDelayedAction(500, () => dec.updateMap());
        }

        public static void setFloorsTilesheet(int whichRoom, int which, TileSheet tilesheet, DecoratableLocation dec, AnimatedTile animations = null)
        {
            List<Rectangle> floors = dec.getFloors();
            int i = 336;
            if (whichRoom == -1)
            {
                foreach (Rectangle rectangle in floors)
                {
                    int x = rectangle.X;
                    while (x < rectangle.Right)
                    {
                        int y = rectangle.Y;
                        while (y < rectangle.Bottom)
                        {
                            if (rectangle.Contains(x, y))
                                setMapTilesheet(dec.map, tilesheet, x, y, "Back", 0, i, animations);
                            if (rectangle.Contains(x + 1, y))
                                setMapTilesheet(dec.map, tilesheet, x + 1, y, "Back", 0, i, animations);
                            if (rectangle.Contains(x, y + 1))
                                setMapTilesheet(dec.map, tilesheet, x, y + 1, "Back", 0, i, animations);
                            if (rectangle.Contains(x + 1, y + 1))
                                setMapTilesheet(dec.map, tilesheet, x + 1, y + 1, "Back", 0, i, animations);
                            y += 2;
                        }
                        x += 2;
                    }
                }
            }
            else
            {
                if (floors.Count <= whichRoom)
                    return;
                Rectangle rectangle = floors[whichRoom];
                int x = rectangle.X;
                while (x < rectangle.Right)
                {
                    int y = rectangle.Y;
                    while (y < rectangle.Bottom)
                    {
                        if (rectangle.Contains(x, y))
                            setMapTilesheet(dec.map, tilesheet, x, y, "Back", 0, i, animations);
                        if (rectangle.Contains(x + 1, y))
                            setMapTilesheet(dec.map, tilesheet, x + 1, y, "Back", 0, i, animations);
                        if (rectangle.Contains(x, y + 1))
                            setMapTilesheet(dec.map, tilesheet, x, y + 1, "Back", 0, i, animations);
                        if (rectangle.Contains(x + 1, y + 1))
                            setMapTilesheet(dec.map, tilesheet, x + 1, y + 1, "Back", 0, i, animations);
                        y += 2;
                    }
                    x += 2;
                }
            }

            PyTK.PyUtils.setDelayedAction(500, () => dec.updateMap());
        }

        public TileSheet initTilesheet(ref Map map)
        {
            return loadTilesheet(ref map, name, isFloor);
        }

        public static TileSheet loadTilesheet(ref Map map, string name, bool isFloor)
        {
            string ts = "zCWF" + (isFloor ? ".Floors." : ".Walls.") + name;
            TileSheet tilesheet = map.GetTileSheet(ts);

            if (tilesheet == null)
            {
                string path = isFloor ? Path.Combine(name, "floors") : Path.Combine(name, "walls");
                Texture2D texture = isFloor ? Floors[name] : Walls[name];
                tilesheet = new TileSheet(ts, map, path, new xTile.Dimensions.Size(texture.Width / 16, texture.Height / 16), new xTile.Dimensions.Size(16, 16));
                map.AddTileSheet(tilesheet);
                map.LoadTileSheets(Game1.mapDisplayDevice);
            }

            return tilesheet;
        }

        public static void setMapTilesheet(Map map, TileSheet tilesheet, int tileX, int tileY, string layer, int whichTileSheet = 0, int index = 0, AnimatedTile animations = null)
        {
            try
            {
                if (!tilesheet.Id.Contains("zCWF"))
                    index = 0;
                Tile tile = map.GetLayer(layer).Tiles[tileX, tileY];
                Tile newTile = null;
                if (animations != null)
                {
                    List<StaticTile> statics = new List<StaticTile>();
                    for (int i = 0; i < animations.Frames; i++)
                        statics.Add(new StaticTile(map.GetLayer(layer), tilesheet, BlendMode.Alpha, (i * (animations.Floor ? 2 : 1)) + (tile.TileIndex - index)));
                    newTile = new xTile.Tiles.AnimatedTile(map.GetLayer(layer), statics.ToArray(), animations.Length);
                    foreach (var p in tile.Properties)
                        newTile.Properties.Add(p);

                    PyTK.PyUtils.setDelayedAction(200, () => map.GetLayer(layer).Tiles[tileX, tileY] = newTile);
                }
                else
                {
                    newTile = new StaticTile(map.GetLayer(layer), tilesheet, BlendMode.Alpha, tile.TileIndex - index);
                    foreach (var p in tile.Properties)
                        newTile.Properties.Add(p);

                    PyTK.PyUtils.setDelayedAction(200, () => map.GetLayer(layer).Tiles[tileX, tileY] = newTile);
                }
            }
            catch
            {

            }
        }

        public ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            return new CustomWallpaper(additionalSaveData["id"], int.Parse(additionalSaveData["which"]), (replacement as Wallpaper).isFloor);
        }

        public object getReplacement()
        {
            return new Wallpaper(1, isFloor);
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            var savedata = new Dictionary<string, string>();
            savedata.Add("id", name);
            savedata.Add("whiche", parentSheetIndex.ToString());

            return savedata;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            
        }
    }
}
