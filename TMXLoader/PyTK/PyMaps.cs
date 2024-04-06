using System.Collections.Generic;
using System.Linq;
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
using xTile.Dimensions;
using System;
using Netcode;
using xTile.ObjectModel;
using Microsoft.Xna.Framework.Graphics;
using TMXTile;
using System.Reflection;

namespace TMXLoader
{
    public static class PyMaps
    {
        internal static IModHelper Helper { get; } = TMXLoaderMod.helper;
        internal static IMonitor Monitor { get; } = TMXLoaderMod.monitor;

        /// <summary>Looks for an object of the requested type on this map position.</summary>
        /// <returns>Return the object or null.</returns>
        public static T sObjectOnMap<T>(this Vector2 t) where T : SObject
        {
            if (Game1.currentLocation is GameLocation location)
            {
                if (location.netObjects.FieldDict.TryGetValue(t, out NetRef<SObject> netRaw) && netRaw.Value is T netValue)
                    return netValue;
                if (location.overlayObjects.TryGetValue(t, out SObject overlayRaw) && overlayRaw is T overlayValue)
                    return overlayValue;
            }
            return null;
        }

        /// <summary>Looks for an object of the requested type on this map position.</summary>
        /// <returns>Return the object or null.</returns>
        public static T terrainOnMap<T>(this Vector2 t) where T : TerrainFeature
        {
            if (Game1.currentLocation is GameLocation location)
            {
                if (location.terrainFeatures.FieldDict.TryGetValue(t, out NetRef<TerrainFeature> raw) && raw.Value is T value)
                    return value;
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
                return ((T) furniture.Find(f => f.boundingBox.Value.Intersects(new Microsoft.Xna.Framework.Rectangle((int) t.X * Game1.tileSize, (int) t.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize))));
            }
            return null;
        }

        /// <summary>Looks for an object of the requested type on this map position.</summary>
        /// <returns>Return the object or null.</returns>
        public static SObject sObjectOnMap(this Vector2 t)
        {
            if (Game1.currentLocation is GameLocation location)
            {
                if (location.netObjects.FieldDict.TryGetValue(t, out NetRef<SObject> netObj))
                    return netObj.Value;
                if (location.overlayObjects.TryGetValue(t, out SObject overlayObj))
                    return overlayObj;
            }
            return null;
        }

        public static bool setMapProperty(this Map map, string property, string value)
        {
            map.Properties[property] = value;
            return true;
        }

        public static string getMapProperty(this Map map, string property)
        {
            PropertyValue p = "";
            if (map.Properties.TryGetValue(property, out p))
            {
                return p.ToString();
            }
            return "";
        }

        public static bool hasTileSheet(this Map map, TileSheet tilesheet)
        {
            foreach (TileSheet ts in map.TileSheets)
                if (tilesheet.ImageSource.EndsWith(new FileInfo(ts.ImageSource).Name) || tilesheet.Id == ts.Id)
                    return true;

            return false;
        }

        public static Dictionary<Layer, List<LayerEventHandler>> LayerHandlerList = new Dictionary<Layer, List<LayerEventHandler>>();

        public static Map enableMoreMapLayers(this Map map)
        {
            foreach (Layer layer in map.Layers)
            {
                if (layer.Properties.ContainsKey("OffestXReset"))
                    layer.SetOffset(new Location(layer.Properties["OffestXReset"], layer.Properties["OffestYReset"]));

                if (LayerHandlerList.ContainsKey(layer))
                    continue;

                LayerHandlerList.Add(layer, new List<LayerEventHandler>());

                if (layer.Properties.ContainsKey("Draw") && map.GetLayer(layer.Properties["Draw"]) is Layer maplayer)
                {
                    LayerEventHandler l = (s, e) => drawLayer(layer, layer.GetOffset(), layer.Properties.ContainsKey("WrapAround"));
                    maplayer.AfterDraw += l;
                    LayerHandlerList[layer].Add(l);
                }
                else if (layer.Properties.ContainsKey("DrawAbove") && map.GetLayer(layer.Properties["DrawAbove"]) is Layer maplayerAbove)
                {
                    LayerEventHandler l = (s, e) => drawLayer(layer, layer.GetOffset(), layer.Properties.ContainsKey("WrapAround"));
                    maplayerAbove.AfterDraw += l;
                    LayerHandlerList[layer].Add(l);
                }
                else if (layer.Properties.ContainsKey("DrawBefore") && map.GetLayer(layer.Properties["DrawBefore"]) is Layer maplayerBefore)
                {
                    LayerEventHandler l = (s, e) => drawLayer(layer, layer.GetOffset(), layer.Properties.ContainsKey("WrapAround"));
                    maplayerBefore.BeforeDraw += l;
                    LayerHandlerList[layer].Add(l); ;
                }
            }
            return map;
        }

        public static void drawLayer(Layer layer, Location offset, bool wrap = false)
        {
            drawLayer(layer, offset, Game1.viewport, wrap);
        }

        public static void drawLayer(Layer layer, Location offset, xTile.Dimensions.Rectangle viewport, bool wrap = false)
        {
            if (Game1.currentLocation is GameLocation location && location.map is Map map && !map.Layers.Contains(layer))
                return;

            drawLayer(layer, Game1.mapDisplayDevice, viewport, Game1.pixelZoom, offset, wrap);
        }


        public static void drawLayer(Layer layer, xTile.Display.IDisplayDevice device, xTile.Dimensions.Rectangle viewport, int pixelZoom, Location offset, bool wrap = false)
        {
            if (layer.Properties.ContainsKey("DrawConditions") && !layer.Properties.ContainsKey("DrawConditionsResult") && Game1.currentLocation is GameLocation gl && gl.Map is Map m)
                PyUtils.checkDrawConditions(m);

            if (layer.Properties.ContainsKey("DrawConditions") && (!layer.Properties.ContainsKey("DrawConditionsResult") || layer.Properties["DrawConditionsResult"] != "T"))
                return;

            
                if (!layer.Properties.ContainsKey("OffestXReset"))
                {
                    layer.Properties["OffestXReset"] = offset.X;
                    layer.Properties["OffestYReset"] = offset.Y;
                }

            if (!layer.Properties.ContainsKey("StartX"))
            {
                Vector2 local = Game1.GlobalToLocal(new Vector2(offset.X, offset.Y));
                layer.Properties["StartX"] = local.X;
                layer.Properties["StartY"] = local.Y;
            }

            if (layer.Properties.ContainsKey("AutoScrollX"))
            {
                string[] ax = layer.Properties["AutoScrollX"].ToString().Split(',');
                int cx = int.Parse(ax[0]);
                int mx = 1;
                if (ax.Length > 1)
                    mx = int.Parse(ax[1]);

                if (cx < 0)
                    mx *= -1;

                if (Game1.currentGameTime.TotalGameTime.Ticks % cx == 0)
                    offset.X += mx;
            }

            if (layer.Properties.ContainsKey("AutoScrollY"))
            {
                string[] ay = layer.Properties["AutoScrollY"].ToString().Split(',');
                int cy = int.Parse(ay[0]);
                int my = 1;
                if (ay.Length > 1)
                    my = int.Parse(ay[1]);

                if (cy < 0)
                    my *= -1;

                if (Game1.currentGameTime.TotalGameTime.Ticks % cy == 0)
                    offset.Y += my;
            }

            layer.SetOffset(offset);

            if (layer.Properties.ContainsKey("tempOffsetx") && layer.Properties.ContainsKey("tempOffsety"))
                offset = new Location(int.Parse(layer.Properties["tempOffsetx"]), int.Parse(layer.Properties["tempOffsety"]));

            if (layer.IsImageLayer())
                drawImageLayer(layer, offset, wrap);
            else
                layer.Draw(device, viewport, offset, wrap, pixelZoom);

        }

        public static void drawImageLayer(Layer layer, Location offset, bool wrap = false)
        {
           drawImageLayer(layer, Game1.mapDisplayDevice, Game1.viewport, Game1.pixelZoom, offset, wrap);
        }

        private static void drawImageLayer(Layer layer, xTile.Display.IDisplayDevice device, xTile.Dimensions.Rectangle viewport, int pixelZoom, Location offset, bool wrap = false)
        {

            
            Vector2 pos = Game1.GlobalToLocal(new Vector2(offset.X, offset.Y));

            if (layer.Properties.ContainsKey("ParallaxX") || layer.Properties.ContainsKey("ParallaxY"))
            {
                Vector2 end = pos;
                if (layer.Properties.ContainsKey("OffestXReset"))
                {
                    end.X = layer.Properties["OffestXReset"];
                    end.Y = layer.Properties["OffestYReset"];
                }
                end = Game1.GlobalToLocal(end);

                Vector2 start = new Vector2(layer.Properties["StartX"], layer.Properties["StartY"]);

                Vector2 dif = start - end;

                if (layer.Properties.ContainsKey("ParallaxX"))
                    pos.X += ((float.Parse(layer.Properties["ParallaxX"]) * dif.X) / 100f) - dif.X;

                if (layer.Properties.ContainsKey("ParallaxY"))
                    pos.Y += ((float.Parse(layer.Properties["ParallaxY"]) * dif.Y) / 100f) - dif.Y;

            }


            if (!wrap)
            {
                layer.Draw(device,viewport,offset,wrap,pixelZoom);
                return;
            }

            if (layer.GetTileSheetForImageLayer() is TileSheet ts
                && PyDisplayDevice.Instance is PyDisplayDevice sDevice
                && sDevice.GetTexture(ts) is Texture2D texture
                && layer.GetOpacity() is float opacity)
            {
                Color color = Color.White;

                if (layer.GetColor() is TMXColor c)
                    color = new Color(c.R, c.G, c.B, c.A);

                var vp = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height);
                Microsoft.Xna.Framework.Rectangle dest = new Microsoft.Xna.Framework.Rectangle((int)pos.X, (int)pos.Y, texture.Width * Game1.pixelZoom, texture.Height * Game1.pixelZoom);

                Vector2 s = pos;

                while (s.X > (vp.X - (dest.Width * 2)) || s.Y > (vp.Y - (dest.Height * 2)))
                {
                    s.X -= dest.Width;
                    s.Y -= dest.Height;
                }

                Vector2 e = new Vector2(vp.X + vp.Width + (dest.Width * 2), vp.Height + vp.Y + (dest.Height * 2));

                for (float x = s.X; x <= e.X; x += dest.Width)
                    for (Microsoft.Xna.Framework.Rectangle n = new Microsoft.Xna.Framework.Rectangle((int)x, (int)s.Y, dest.Width, dest.Height); n.Y <= e.Y; n.Y += dest.Height)
                        if ((layer.Properties["WrapAround"] != "Y" || n.X == dest.X) && (layer.Properties["WrapAround"] != "X" || n.Y == dest.Y))
                            Game1.spriteBatch.Draw(texture, n, color * opacity);
            }
        }

        public static void drawImageLayer(Layer layer, xTile.Dimensions.Rectangle viewport)
        {
            if (layer.GetTileSheetForImageLayer() is TileSheet ts
                && PyDisplayDevice.Instance is PyDisplayDevice device
                && device.GetTexture(ts) is Texture2D texture
                && layer.GetOffset() is Location posGlobal
                && layer.GetOpacity() is float opacity)
            {
                Color color = Color.White;
                if (layer.GetColor() is TMXColor c)
                    color = new Color(c.R, c.G, c.B, c.A);

                Vector2 pos = Game1.GlobalToLocal(new Vector2(posGlobal.X, posGlobal.Y));
                Microsoft.Xna.Framework.Rectangle dest = new Microsoft.Xna.Framework.Rectangle((int)pos.X, (int)pos.Y, texture.Width * Game1.pixelZoom, texture.Height * Game1.pixelZoom);

                Game1.spriteBatch.Draw(texture, dest, color * opacity);
            }
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
                    l.largeTerrainFeatures.Remove(new List<LargeTerrainFeature>(l.largeTerrainFeatures).Find(p => p.Tile == new Vector2(x,y)));
                    l.terrainFeatures.Remove(new Vector2(x, y));
                }

            return l;
        }

        public static Map mergeInto(this Map t, Map map, Vector2 position, Microsoft.Xna.Framework.Rectangle? sourceArea = null, bool includeEmpty = true, bool properties = true)
        {
            Microsoft.Xna.Framework.Rectangle sourceRectangle = sourceArea.HasValue ? sourceArea.Value : new Microsoft.Xna.Framework.Rectangle(0, 0, t.DisplayWidth / Game1.tileSize, t.DisplayHeight / Game1.tileSize);

            //tilesheet normalizing taken with permission from Content Patcher 
            // https://github.com/Pathoschild/StardewMods/blob/stable/ContentPatcher/Framework/Patches/EditMapPatch.cs
            foreach (TileSheet tilesheet in t.TileSheets)
            {
                TileSheet mapSheet = map.GetTileSheet(tilesheet.Id);

                if (mapSheet == null || mapSheet.ImageSource != tilesheet.ImageSource)
                {
                    // change ID if needed so new tilesheets are added after vanilla ones (to avoid errors in hardcoded game logic)
                    string id = tilesheet.Id;
                    if (!id.StartsWith("z_", StringComparison.InvariantCultureIgnoreCase))
                        id = $"z_{id}";

                    // change ID if it conflicts with an existing tilesheet
                    if (map.GetTileSheet(id) != null)
                    {
                        int disambiguator = Enumerable.Range(2, int.MaxValue - 1).First(p => map.GetTileSheet($"{id}_{p}") == null);
                        id = $"{id}_{disambiguator}";
                    }
                    //add tilesheet
                    
                    if(!map.TileSheets.ToList().Exists(ts => ts.Id == tilesheet.Id))
                        map.AddTileSheet(new TileSheet(tilesheet.Id, map, tilesheet.ImageSource, tilesheet.SheetSize, tilesheet.TileSize));
                }
            }

            if (properties)
                foreach (KeyValuePair<string, PropertyValue> p in t.Properties)
                    if (map.Properties.ContainsKey(p.Key))
                        if (p.Key == "EntryAction")
                            map.Properties[p.Key] = map.Properties[p.Key] + ";" + p.Value;
                        else
                            map.Properties[p.Key] = p.Value;
                    else
                        map.Properties.Add(p);

            /*
            int w = (int)position.X + sourceRectangle.Width;
            int h = (int)position.Y + sourceRectangle.Height;

            foreach (Layer l in map.Layers.Where(ly => ly.LayerWidth < w || ly.LayerHeight < h))
            {
                Monitor.Log("Expanding Map " + l.LayerWidth + "x" + l.LayerHeight + "->" + w + "x" + h, LogLevel.Warn);

                if (l.LayerWidth < w)
                    l.LayerWidth = w;

                if (l.LayerHeight < h)
                    l.LayerHeight = h;

            }
            */
            for (int x = 0; x < sourceRectangle.Width; x++)
                for (int y = 0; y < sourceRectangle.Height; y++)
                    foreach (Layer layer in t.Layers)
                    {
                        int px = (int)position.X + x;
                        int py = (int)position.Y + y;

                        int sx = (int)sourceRectangle.X + x;
                        int sy = (int)sourceRectangle.Y + y;

                        Tile sourceTile = layer.Tiles[(int)sx, (int)sy];
                        Layer mapLayer = map.GetLayer(layer.Id);

                        if (mapLayer == null)
                        {
                            map.InsertLayer(new Layer(layer.Id, map, map.Layers[0].LayerSize, map.Layers[0].TileSize), map.Layers.Count);
                            mapLayer = map.GetLayer(layer.Id);
                        }

                        if (mapLayer.IsImageLayer())
                            mapLayer.SetupImageLayer();

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
                                    mapLayer.Tiles[(int)px, (int)py] = null;
                                }
                                catch { }
                            }
                            continue;
                        }

                        TileSheet tilesheet = map.GetTileSheet(sourceTile.TileSheet.Id);
                        Tile newTile = new StaticTile(mapLayer, tilesheet, BlendMode.Additive, sourceTile.TileIndex);

                        try
                        {
                            if (sourceTile.Properties.ContainsKey("NoTileMerge"))
                                newTile = mapLayer.Tiles[(int)px, (int)py];
                        }
                        catch
                        {

                        }

                        if (sourceTile is AnimatedTile aniTile)
                        {
                            List<StaticTile> staticTiles = new List<StaticTile>();

                            foreach (StaticTile frame in aniTile.TileFrames)
                                staticTiles.Add(new StaticTile(mapLayer, tilesheet, BlendMode.Additive, frame.TileIndex));

                            newTile = new AnimatedTile(mapLayer, staticTiles.ToArray(), aniTile.FrameInterval);
                        }

                        if (properties)
                        {
                            foreach (var prop in sourceTile.Properties)
                                if (newTile.Properties.ContainsKey(prop.Key))
                                    newTile.Properties[prop.Key] = prop.Value;
                                else
                                    newTile.Properties.Add(prop);

                            foreach (var prop in sourceTile.TileIndexProperties)
                                if (newTile.TileIndexProperties.ContainsKey(prop.Key))
                                    newTile.TileIndexProperties[prop.Key] = prop.Value;
                                else
                                    newTile.TileIndexProperties.Add(prop);
                        }
                        try
                        {
                            mapLayer.Tiles[(int)px, (int)py] = newTile;
                        }
                        catch (Exception e)
                        {
                            Monitor.Log($"{e.Message} ({map.DisplayWidth} -> {layer.Id} -> {px}:{py})");
                        }
                    }

            return map;
        }

        public static void addAction(this Map m, Vector2 position, TileAction action, string args)
        {
            var props = m.GetLayer("Buildings").PickTile(new Location((int)position.X * Game1.tileSize, (int)position.Y * Game1.tileSize), Game1.viewport.Size).Properties;
            props.Remove("Action");
            props.Add("Action", action.trigger + " " + args);
        }

        public static void addAction(this Map m, Vector2 position, string trigger, string args)
        {
            var props = m.GetLayer("Buildings").PickTile(new Location((int)position.X * Game1.tileSize, (int)position.Y * Game1.tileSize), Game1.viewport.Size).Properties;
            props.Remove("Action");
            props.Add("Action", trigger + " " + args);
        }

        public static void addTouchAction(this Map m, Vector2 position, TileAction action, string args)
        {
            var props = m.GetLayer("Back").PickTile(new Location((int)position.X * Game1.tileSize, (int)position.Y * Game1.tileSize), Game1.viewport.Size).Properties;
            props.Remove("TouchAction");
            props.Add("TouchAction", action.trigger + " " + args);
        }

        public static void addTouchAction(this Map m, Vector2 position, string trigger, string args)
        {
            var props = m.GetLayer("Back").PickTile(new Location((int)position.X * Game1.tileSize, (int)position.Y * Game1.tileSize), Game1.viewport.Size).Properties;
            props.Remove("TouchAction");
            props.Add("TouchAction", trigger + " " + args);
        }
    }
}
