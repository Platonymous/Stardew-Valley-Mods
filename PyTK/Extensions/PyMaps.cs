using System.Collections.Generic;
using StardewValley;
using SObject = StardewValley.Object;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using StardewValley.Objects;
using StardewValley.Locations;
using xTile;
using xTile.Tiles;
using xTile.Layers;
using System.IO;
using PyTK.Types;
using xTile.Dimensions;
using System;
using xTile.ObjectModel;
using PyTK.Extensions;
using PyTK.Tiled;

namespace PyTK.Extensions
{
    public static class PyMaps
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        /// <summary>Looks for an object of the requested type on this map position.</summary>
        /// <returns>Return the object or null.</returns>
        public static T sObjectOnMap<T>(this Vector2 t) where T : SObject
        {
            if (Game1.currentLocation is GameLocation location)
            {
                Dictionary<Vector2, SObject> objects = (Dictionary<Vector2, SObject>)location.objects.Pairs;
                if (objects.ContainsKey(t) && (objects[t] is T))
                    return ((T) objects[t]);
            }
            return null;
        }

        /// <summary>Looks for an object of the requested type on this map position.</summary>
        /// <returns>Return the object or null.</returns>
        public static T terrainOnMap<T>(this Vector2 t) where T : TerrainFeature
        {
            if (Game1.currentLocation is GameLocation location)
            {
                Dictionary<Vector2, TerrainFeature> terrain = (Dictionary < Vector2, TerrainFeature > ) location.terrainFeatures.FieldDict;
                if (terrain.ContainsKey(t) && (terrain[t] is T))
                    return ((T) terrain[t]);
            }

            return null;
        }

        /// <summary>Looks for an object of the requested type on this map position.</summary>
        /// <returns>Return the object or null.</returns>
        public static T furnitureOnMap<T>(this Vector2 t) where T : Furniture
        {
            if (Game1.currentLocation is DecoratableLocation location)
            {
                List<Furniture> furniture = new List<Furniture>(location.furniture);
                return ((T) furniture.Find(f => f.getBoundingBox(t).Intersects(new Microsoft.Xna.Framework.Rectangle((int) t.X * Game1.tileSize, (int) t.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize))));
            }
            return null;
        }

        /// <summary>Looks for an object of the requested type on this map position.</summary>
        /// <returns>Return the object or null.</returns>
        public static SObject sObjectOnMap(this Vector2 t)
        {
            if (Game1.currentLocation is GameLocation location)
            {
                Dictionary<Vector2, SObject> objects = (Dictionary<Vector2, SObject>) location.objects.Pairs;
                if (objects.ContainsKey(t))
                    return objects[t];
            }
            return null;
        }

        public static bool hasTileSheet(this Map map, TileSheet tilesheet)
        {
            foreach (TileSheet ts in map.TileSheets)
                if (tilesheet.ImageSource.EndsWith(new FileInfo(ts.ImageSource).Name) || tilesheet.Id == ts.Id)
                    return true;

            return false;
        }

        public static Map enableMoreMapLayers (this Map map)
        {
            foreach (Layer layer in map.Layers)
                if (layer.Properties.ContainsKey("Draw") && map.GetLayer(layer.Properties["Draw"]) is Layer maplayer)
                {
                    maplayer.AfterDraw -= drawLayer;
                    maplayer.AfterDraw += drawLayer;
                }

            return map;
        }

        private static void drawLayer(object sender, LayerEventArgs e)
        {
            e.Layer.Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, false, Game1.pixelZoom);
        }

        public static Map switchLayers(this Map t, string layer1, string layer2)
        {
            Layer newLayer1 = t.GetLayer(layer1);
            Layer newLayer2 = t.GetLayer(layer2);

            t.RemoveLayer(t.GetLayer(layer1));
            t.RemoveLayer(t.GetLayer(layer2));

            newLayer1.Id = layer2;
            newLayer2.Id = layer1;
            
            t.AddLayer(newLayer1);
            t.AddLayer(newLayer2);
            
            return t;
        }

        public static Map switchTileBetweenLayers(this Map t, string layer1, string layer2, int x, int y)
        {
            Location tileLocation = new Location(x , y);

            Tile tile1 = t.GetLayer(layer1).Tiles[tileLocation];
            Tile tile2 = t.GetLayer(layer2).Tiles[tileLocation];

            t.GetLayer(layer1).Tiles[tileLocation] = tile2;
            t.GetLayer(layer2).Tiles[tileLocation] = tile1;
            
            return t;
        }

        public static GameLocation clearArea(this GameLocation l, Microsoft.Xna.Framework.Rectangle area)
        {

            for (int x = area.X; x < area.Width; x++)
                for (int y = area.Y; y < area.Height; y++)
                {
                    l.objects.Remove(new Vector2(x, y));
                    l.largeTerrainFeatures.Remove(new List<LargeTerrainFeature>(l.largeTerrainFeatures).Find(p => p.tilePosition.Value == new Vector2(x,y)));
                    l.terrainFeatures.Remove(new Vector2(x, y));
                }

            return l;
        }

        public static Map mergeInto(this Map t, Map map, Vector2 position, Microsoft.Xna.Framework.Rectangle? sourceArea = null, bool includeEmpty = true, bool properties = true)
        {
            Microsoft.Xna.Framework.Rectangle sourceRectangle = sourceArea.HasValue ? sourceArea.Value : new Microsoft.Xna.Framework.Rectangle(0, 0, t.DisplayWidth / Game1.tileSize, t.DisplayHeight / Game1.tileSize);

            foreach (TileSheet tilesheet in t.TileSheets)
                if (!map.hasTileSheet(tilesheet))
                    map.AddTileSheet(new TileSheet(tilesheet.Id, map, tilesheet.ImageSource, tilesheet.SheetSize, tilesheet.TileSize));

            if(properties)
            foreach (KeyValuePair<string, PropertyValue> p in t.Properties)
                if (map.Properties.ContainsKey(p.Key))
                    if (p.Key == "EntryAction")
                        map.Properties[p.Key] = map.Properties[p.Key] + ";" + p.Value;
                    else
                        map.Properties[p.Key] = p.Value;
                else
                    map.Properties.Add(p);

            for (Vector2 _x = new Vector2(sourceRectangle.X, position.X); _x.X < sourceRectangle.Width; _x += new Vector2(1, 1))
            {
                for (Vector2 _y = new Vector2(sourceRectangle.Y, position.Y); _y.X < sourceRectangle.Height; _y += new Vector2(1, 1))
                {
                    foreach (Layer layer in t.Layers)
                    {
                        

                        Tile sourceTile = layer.Tiles[(int)_x.X, (int)_y.X];
                        Layer mapLayer = map.GetLayer(layer.Id);

                        if (mapLayer == null)
                        {
                            map.InsertLayer(new Layer(layer.Id, map, map.Layers[0].LayerSize, map.Layers[0].TileSize), map.Layers.Count);
                            mapLayer = map.GetLayer(layer.Id);
                        }

                        if (properties)
                            foreach (var prop in layer.Properties)
                                if (!mapLayer.Properties.ContainsKey(prop.Key))
                                    mapLayer.Properties.Add(prop);
                                else
                                    mapLayer.Properties[prop.Key] = prop.Value;

                        if (sourceTile == null)
                        {
                            if (includeEmpty)
                            {
                                try
                                {
                                    mapLayer.Tiles[(int)_x.Y, (int)_y.Y] = null;
                                }
                                catch { }
                            }
                            continue;
                        }

                        TileSheet tilesheet = map.GetTileSheet(sourceTile.TileSheet.Id);
                        Tile newTile = new StaticTile(mapLayer, tilesheet, BlendMode.Additive, sourceTile.TileIndex);

                        if (sourceTile is AnimatedTile aniTile)
                        {
                            List<StaticTile> staticTiles = new List<StaticTile>();

                            foreach (StaticTile frame in aniTile.TileFrames)
                                staticTiles.Add(new StaticTile(mapLayer, tilesheet, BlendMode.Additive, frame.TileIndex));

                            newTile = new AnimatedTile(mapLayer, staticTiles.ToArray(), aniTile.FrameInterval);
                        }

                        if(properties)
                            foreach (var prop in sourceTile.Properties)
                                newTile.Properties.Add(prop);
                        try
                        {
                            mapLayer.Tiles[(int)_x.Y, (int)_y.Y] = newTile;
                        }catch(Exception e){
                            Monitor.Log($"{e.Message} ({map.DisplayWidth} -> {layer.Id} -> {_x.Y}:{_y.Y})");
                        }
                    }

                }
            }
            return map;
        }

        public static void addAction(this Map m, Vector2 position, TileAction action, string args)
        {
            m.GetLayer("Buildings").PickTile(new Location((int)position.X * Game1.tileSize, (int)position.Y * Game1.tileSize), Game1.viewport.Size).Properties.AddOrReplace("Action", action.trigger + " " + args);
        }

        public static void addAction(this Map m, Vector2 position, string trigger, string args)
        {
            m.GetLayer("Buildings").PickTile(new Location((int)position.X * Game1.tileSize, (int)position.Y * Game1.tileSize), Game1.viewport.Size).Properties.AddOrReplace("Action", trigger + " " + args);
        }

        public static void addTouchAction(this Map m, Vector2 position, TileAction action, string args)
        {
            m.GetLayer("Back").PickTile(new Location((int)position.X * Game1.tileSize, (int)position.Y * Game1.tileSize), Game1.viewport.Size).Properties.AddOrReplace("TouchAction", action.trigger + " " + args);
        }

        public static void addTouchAction(this Map m, Vector2 position, string trigger, string args)
        {
            m.GetLayer("Back").PickTile(new Location((int)position.X * Game1.tileSize, (int)position.Y * Game1.tileSize), Game1.viewport.Size).Properties.AddOrReplace("TouchAction", trigger + " " + args);
        }
    }
}
