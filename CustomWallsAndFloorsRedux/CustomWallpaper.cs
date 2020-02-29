using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomWallsAndFloorsRedux
{
    public class CustomWallpaper : Wallpaper, ICustomObject
    {
        internal CustomSet Set { get; set; }

        public static CustomWallpaper BeingPlaced { get; set; }

        public static SavedWallpaper BeingSaved { get; set; }

        public Animation Animation => (Set.Settings.AnimatedTiles.Find(a => a.Index == CustomIndex && a.Floor == isFloor.Value) is Animation anim) ? anim : null;

        public int CustomIndex { get; set; }

        internal bool Injected { get; set; } = false;

        public CustomWallpaper()
        {

        }

        public CustomWallpaper(int index, CustomSet set, bool isFloors = false)
            : base(index, isFloors)
        {
            ParentSheetIndex = 0;
            name = name + ":" + set.Id + ":" + index;
            build(set, index);
        }

        public override string Name
        {
            get
            {
                return this.name.Split(':')[0];
            }
        }

        public void build(CustomSet set, int index)
        {
            Set = set;
            CustomIndex = index;
            Rectangle sr = isFloor.Value ? Game1.getSourceRectForStandardTileSheet(Set.Floors, CustomIndex, 32, 32) : Game1.getSourceRectForStandardTileSheet(Set.Walls, CustomIndex, 16, 48);
            ParentSheetIndex = 0;
            if (isFloor)
            {
                sr.Width = 28;
                sr.Height = 26;
            }
            else
                sr.Height = 28;

            sourceRect.Value = sr;
        }

        public void checkForMP()
        {
            if (Set == null && name.Split(':') is string[] splits)
                if (splits.Length > 2 && CustomWallsAndFloorsMod.Sets.FirstOrDefault(s => s.Id == splits[1]) is CustomSet set && int.TryParse(splits[2], out int index))
                    build(set, index);

            if (Set == null)
                return;
        }

        public override string getDescription()
        {
            return $"{ Set.Pack.Manifest.Name } ${CustomIndex}  ({ Set.Pack.Manifest.Author})"; ;
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            checkForMP();
            base.drawWhenHeld(spriteBatch, objectPosition, f);
        }

        public override void drawAsProp(SpriteBatch b)
        {
            checkForMP();
            base.drawAsProp(b);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            checkForMP();

            Texture2D wt = wallpaperTexture;
            wallpaperTexture = isFloor ? Set.Floors : Set.Walls;
            ParentSheetIndex = CustomIndex;
            base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
            ParentSheetIndex = 0;
            wallpaperTexture = wt;
        }
        
        public void place(GameLocation location, int x, int y, bool send = false)
        {
            if (location is DecoratableLocation currentLocation && new Point(x / 64, y / 64) is Point point)
                if (isFloor.Value && currentLocation.getFloors() is List<Rectangle> floors)
                {
                    for (int whichRoom = 0; whichRoom < floors.Count; ++whichRoom)
                        if (floors[whichRoom].Contains(point))
                        {
                            CustomWallsAndFloorsMod.Placing = true;

                            if (location.isStructure.Value) {
                            }


                            BeingSaved = new SavedWallpaper(Set.Pack.Manifest.UniqueID, CustomIndex, location.isStructure.Value ? location.uniqueName.Value : location.Name, x, y, isFloor, send, location.isStructure.Value, CustomWallsAndFloorsMod.getWarpString(location));
                            BeingPlaced = this;

                            currentLocation.GetType().GetMethod("doSetVisibleFloor", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(currentLocation, new object[] { whichRoom, ParentSheetIndex });

                            if (currentLocation is FarmHouse fh)
                                foreach (var r in FHRHandler.GetConnectedWalls(fh, whichRoom, isFloor.Value))
                                    currentLocation.GetType().GetMethod("doSetVisibleFloor", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(currentLocation, new object[] { r, ParentSheetIndex });
                                    
                            BeingPlaced = null;
                            CustomWallsAndFloorsMod.Placing = false;
                        }
                }
                else if (currentLocation.getWalls() is List<Rectangle> walls)
                    for (int whichRoom = 0; whichRoom < walls.Count; ++whichRoom)
                        if (walls[whichRoom].Contains(point))
                        {
                            CustomWallsAndFloorsMod.Placing = true;
                            BeingSaved = new SavedWallpaper(Set.Pack.Manifest.UniqueID, CustomIndex, location.isStructure.Value ? location.uniqueName.Value : location.Name, x, y, isFloor, send, location.isStructure.Value, CustomWallsAndFloorsMod.getWarpString(location));
                            BeingPlaced = this;

                            currentLocation.GetType().GetMethod("doSetVisibleWallpaper", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(currentLocation, new object[] { whichRoom, ParentSheetIndex });
                            
                            if (currentLocation is FarmHouse fh)
                                foreach (var r in FHRHandler.GetConnectedWalls(fh, whichRoom, isFloor.Value))
                                    currentLocation.GetType().GetMethod("doSetVisibleWallpaper", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(currentLocation, new object[] { r, ParentSheetIndex });
                                    
                            BeingPlaced = null;
                            CustomWallsAndFloorsMod.Placing = false;
                        }
        }


        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            CustomWallsAndFloorsMod.Placing = false;
            if (base.placementAction(location, x, y, null) )
            {
                place(location, x, y, true);

                location.playSound("coin", NetAudio.SoundContext.Default);
                return true;
            }

            return false;
        }

        public override Item getOne()
        {
            return new CustomWallpaper(CustomIndex, Set, isFloor);
        }

        public string AssetKey => @"CWF//" + Set.Pack.Manifest.UniqueID + (isFloor ? "_Floors_" : "_Walls_") + CustomIndex;

        public void Inject()
        {
            if (Injected)
                return;

            Texture2D texture = (isFloor ? Set.Floors : Set.Walls);
            Rectangle sr = Game1.getSourceRectForStandardTileSheet(texture, CustomIndex, isFloor ? 32 : 16, isFloor ? 32 : 48);

            if (Animation is Animation anim)
               sr.Width = sr.Width * (anim.Frames);

            texture.getArea(sr).inject(AssetKey);

            Injected = true;
        }

        public ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            if (additionalSaveData.ContainsKey("index") && additionalSaveData.ContainsKey("set") && additionalSaveData.ContainsKey("floor"))
                if (CustomWallsAndFloorsMod.Sets.FirstOrDefault(s => s.Id == additionalSaveData["set"]) is CustomSet set && bool.TryParse(additionalSaveData["floor"], out bool f) && int.TryParse(additionalSaveData["index"], out int i))
                    return new CustomWallpaper(i, set, f);

            return null;
        }

        public object getReplacement()
        {
            return new Wallpaper(1, isFloor);
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            var savedata = new Dictionary<string, string>();
            savedata.Add("index", CustomIndex.ToString());
            savedata.Add("set", Set.Id);
            savedata.Add("floor", isFloor.ToString());

            return savedata;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {

        }
    }
}
