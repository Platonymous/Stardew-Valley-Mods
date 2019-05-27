using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using xTile;
using xTile.Dimensions;
using xTile.Format;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace PyTK.Tiled
{
    public class NewTiledTmxFormat : IMapFormat
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        public string Name
        {
            get
            {
                return "Tiled XML Format [Updated]";
            }
        }

        public string FileExtensionDescriptor
        {
            get
            {
                return "Tiled XML Map Files (*.tmx) **";
            }
        }

        public string FileExtension
        {
            get
            {
                return "tmx";
            }
        }

        internal TiledMap TiledMap { get; set; }

        public CompatibilityReport DetermineCompatibility(Map map)
        {
            List<CompatibilityNote> compatibilityNoteList = new List<CompatibilityNote>();
            foreach (TileSheet tileSheet in map.TileSheets)
            {
                Size size = tileSheet.Margin;
                if (!size.Square)
                    compatibilityNoteList.Add(new CompatibilityNote(CompatibilityLevel.None, string.Format("Tilesheet {0}: Margin values ({1}) are not equal.", tileSheet.Id, tileSheet.Margin)));
                size = tileSheet.Spacing;
                if (!size.Square)
                    compatibilityNoteList.Add(new CompatibilityNote(CompatibilityLevel.None, string.Format("Tilesheet {0}: Spacing values ({1}) are not equal.", tileSheet.Id, tileSheet.Spacing)));
            }
            if (map.Layers.Count > 0)
            {
                Layer layer1 = map.Layers[0];
                bool flag1 = false;
                bool flag2 = false;
                bool flag3 = false;
                bool flag4 = false;
                foreach (Layer layer2 in map.Layers)
                {
                    if (layer2 != layer1)
                    {
                        if (layer2.LayerWidth != layer1.LayerWidth)
                            flag1 = true;
                        if (layer2.LayerHeight != layer1.LayerHeight)
                            flag2 = true;
                        if (layer2.TileWidth != layer1.TileWidth)
                            flag3 = true;
                        if (layer2.TileHeight != layer1.TileHeight)
                            flag4 = true;
                    }
                }
                if (flag1)
                    compatibilityNoteList.Add(new CompatibilityNote(CompatibilityLevel.None, "Layer widths do not match across all layers."));
                if (flag2)
                    compatibilityNoteList.Add(new CompatibilityNote(CompatibilityLevel.None, "Layer heights do not match across all layers."));
                if (flag3)
                    compatibilityNoteList.Add(new CompatibilityNote(CompatibilityLevel.None, "Tile widths do not match across all layers."));
                if (flag4)
                    compatibilityNoteList.Add(new CompatibilityNote(CompatibilityLevel.None, "Tile heights do not match across all layers."));
            }
            return new CompatibilityReport(compatibilityNoteList);
        }

        public Map Load(XmlReader xmlReader)
        {
            TiledMap = new TiledMap(XElement.Load(xmlReader));
            return Load(TiledMap);
        }

        public Map Load(Stream stream)
        {
            TiledMap = new TiledMap(XElement.Load(stream));
            return Load(TiledMap);
        }

        public Map Load(TiledMap TiledMap)
        {
            Map map = new Map();
            if (TiledMap.Orientation != "orthogonal")
                throw new Exception("Only orthogonal Tiled maps are supported.");
            List<TiledProperty> properties = TiledMap.Properties;
            if (properties != null)
            {
                Action<TiledProperty> action = prop =>
               {
                   if (prop.Name == "@Description")
                       map.Description = prop.Value;
                   else
                       map.Properties[prop.Name] = prop.Value;
               };
                properties.ForEach(action);
            }
            LoadTileSets(map);
            LoadLayers(map);
            LoadImageLayers(map);
            LoadObjects(map);
            return map;
        }

        public string AsString(Map map)
        {
            return Store(map).ToString();
        }

        public void Store(Map map, Stream stream)
        {
            Store(map).Save(stream);
        }

        public XElement Store(Map map)
        {
            TiledMap tiledMap1 = new TiledMap();
            tiledMap1.Version = "1.0";
            tiledMap1.Orientation = "orthogonal";
            int layerWidth = map.GetLayer("Back").LayerWidth;
            tiledMap1.Width = layerWidth;
            int layerHeight = map.GetLayer("Back").LayerHeight;
            tiledMap1.Height = layerHeight;
            int tileWidth = 16; // map.GetLayer("Back").TileWidth;
            tiledMap1.TileWidth = tileWidth;
            int tileHeight = 16; //map.GetLayer("Back").TileHeight;
            tiledMap1.TileHeight = tileHeight;
            List<TiledProperty> tiledPropertyList = new List<TiledProperty>();
            tiledMap1.Properties = tiledPropertyList;
            List<TiledTileSet> tiledTileSetList = new List<TiledTileSet>();
            tiledMap1.TileSets = tiledTileSetList;
            List<TiledLayer> tiledLayerList = new List<TiledLayer>();
            tiledMap1.Layers = tiledLayerList;
            List<TiledObjectGroup> tiledObjectGroupList = new List<TiledObjectGroup>();
            tiledMap1.ObjectGroups = tiledObjectGroupList;
            TiledMap tiledMap2 = tiledMap1;
            if (map.Description.Length > 0)
                map.Properties["@Description"] = map.Description;
            foreach (KeyValuePair<string, PropertyValue> property in map.Properties)
                tiledMap2.Properties.Add(new TiledProperty(property.Key, property.Value));
            StoreTileSets(map, tiledMap2);
            StoreLayers(map, tiledMap2);
            StoreImageLayers(map, tiledMap2);
            StoreObjects(map, tiledMap2);
            return tiledMap2.ToXml();
        }

        public void StoreImageLayers(Map map, TiledMap tiled)
        {
            
        }

        public void LoadTileSets(Map map)
        {
            List<TiledTileSet> tileSets = TiledMap.TileSets;
            if (tileSets == null)
                return;
            
            Action<TiledTileSet> action1 = tileSet =>
           {

               Size sheetSize = new Size();
               try
               {
                   sheetSize.Width = (tileSet.Image.Width + tileSet.Spacing - tileSet.Margin) / (tileSet.TileWidth + tileSet.Spacing);
                   sheetSize.Height = (tileSet.Image.Height + tileSet.Spacing - tileSet.Margin) / (tileSet.TileHeight + tileSet.Spacing);
               }
               catch (Exception ex)
               {
                   throw new Exception("Unable to determine sheet size", ex);
               }
               tileSet.TileWidth = 64;
               tileSet.TileHeight = 64;
               TileSheet tileSheet = new TileSheet(tileSet.SheetName, map, tileSet.Image.Source, sheetSize, new Size(tileSet.TileWidth, tileSet.TileHeight))
               {
                   Spacing = new Size(tileSet.Spacing),
                   Margin = new Size(tileSet.Margin)
               };
               tileSheet.Properties["@FirstGid"] = tileSet.FirstGid;
               tileSheet.Properties["@LastGid"] = tileSet.LastGid;
               List<TiledTile> tiles = tileSet.Tiles;
               if (tiles != null)
               {
                   Action<TiledTile> action2 = tile =>
             {
                 List<TiledProperty> properties = tile.Properties;
                 if (properties == null)
                     return;
                 Action<TiledProperty> action3 = prop => tileSheet.Properties[string.Format("@TileIndex@{0}@{1}", tile.TileId, prop.Name)] = prop.Value;
                 properties.ForEach(action3);
             };
                   tiles.ForEach(action2);
               }
               map.AddTileSheet(tileSheet);
           };
            tileSets.ForEach(action1);
        }

        public void LoadImageLayers(Map map)
        {
            List<TiledImageLayer> layers = TiledMap.ImageLayers;

            if (layers == null || layers.Count == 0)
                return;

            foreach (TiledImageLayer layer in layers)
            {
                Size imageSize = new Size(layer.ImageWidth, layer.ImageHeight);
                TileSheet imagesheet = new TileSheet("zImageSheet_" + layer.Name, map, layer.Source, new Size(1,1), imageSize);
                map.AddTileSheet(imagesheet);
                Layer imageLayer = new Layer(layer.Name, map, map.Layers[0].LayerSize, map.Layers[0].TileSize);

                List<TiledProperty> properties = layer.Properties;
                if (properties != null)
                {
                    Action<TiledProperty> action2 = prop =>
                    {
                        if (prop.Name == "@Description")
                            imageLayer.Description = prop.Value;
                        else
                            imageLayer.Properties[prop.Name] = prop.Value;
                    };
                    properties.ForEach(action2);
                }

                imageLayer.Visible = !layer.Hidden;
                imageLayer.Properties["offsetx"] = layer.Horizontal * Game1.pixelZoom;
                imageLayer.Properties["offsety"] = layer.Vertical * Game1.pixelZoom;
                imageLayer.Properties["opacity"] = layer.Transparency;
                imageLayer.Properties["isImageLayer"] = true;
                
                map.AddLayer(imageLayer);
            }
        }

        public void LoadLayers(Map map)
        {
            if (map.TileSheets.Count == 0)
                throw new Exception("Must load at least one tileset to determine layer tile size");
            List<TiledLayer> layers = TiledMap.Layers;
            if (layers == null)
                return;
            Action<TiledLayer> action1 = layer =>
           {
               Layer layer1 = new Layer(layer.Name, map, new Size(layer.Width, layer.Height), map.TileSheets[0].TileSize);
               int num = !layer.Hidden ? 1 : 0;
               layer1.Visible = num != 0;
               Layer mapLayer = layer1;
               if (layer.Properties == null)
                   layer.Properties = new List<TiledProperty>();
               List<TiledProperty> properties = layer.Properties;
               if (properties != null)
               {
                   Action<TiledProperty> action2 = prop =>
             {
                 if (prop.Name == "@Description")
                     mapLayer.Description = prop.Value;
                 else
                     mapLayer.Properties[prop.Name] = prop.Value;
             };
                   properties.ForEach(action2);
               }
               if (!(layer.Data.EncodingType == "csv"))
               {
                   Monitor.Log("Error: Change your encoding setting from " + layer.Data.EncodingType + " to CSV");
                   throw new Exception(string.Format("Unknown encoding setting ({0})", layer.Data.EncodingType));
               }
               LoadLayerDataCsv(mapLayer, layer);
               mapLayer.Properties["offsetx"] = (int) Math.Floor(layer.Horizontal * Game1.pixelZoom);
               mapLayer.Properties["offsety"] = (int)Math.Floor(layer.Vertical * Game1.pixelZoom);
               map.AddLayer(mapLayer);
           };
            layers.ForEach(action1);
        }

        internal void LoadLayerDataCsv(Layer mapLayer, TiledLayer tiledLayer)
        {
            string[] strArray = tiledLayer.Data.Data.Split(new char[4]
            {
        ',',
        '\r',
        '\n',
        '\t'
            }, StringSplitOptions.RemoveEmptyEntries);
            Location origin = Location.Origin;
            foreach (string s in strArray)
            {
                int gid = int.Parse(s);
                mapLayer.Tiles[origin] = LoadTile(mapLayer, gid);
                ++origin.X;
                if (origin.X >= mapLayer.LayerWidth)
                {
                    origin.X = 0;
                    ++origin.Y;
                }
            }
        }

        internal Tile LoadTile(Layer layer, int gid)
        {
            if (gid == 0)
                return null;
            TileSheet selectedTileSheet = null;
            int tileIndex = -1;
            foreach (TileSheet tileSheet in layer.Map.TileSheets)
            {
                int property1 = tileSheet.Properties["@FirstGid"];
                int property2 = tileSheet.Properties["@LastGid"];
                if (gid >= property1 && gid <= property2)
                {
                    selectedTileSheet = tileSheet;
                    tileIndex = gid - property1;
                    break;
                }
            }
            if (selectedTileSheet == null)
                throw new Exception(string.Format("Invalid tile gid: {0}", gid));
            TiledTileSet tiledTileSet;
            if ((tiledTileSet = TiledMap.TileSets.FirstOrDefault(tileSheet => tileSheet.SheetName == selectedTileSheet.Id)) == null)
                return new StaticTile(layer, selectedTileSheet, BlendMode.Alpha, tileIndex);
            TiledTile tiledTile1 = tiledTileSet.Tiles.FirstOrDefault(tiledTile =>
           {
               if (tiledTile.TileId == tileIndex)
                   return tiledTile.Animation != null;
               return false;
           });
            if (tiledTile1 == null || tiledTile1.Animation.Count <= 0)
                return new StaticTile(layer, selectedTileSheet, BlendMode.Alpha, tileIndex);
            StaticTile[] array = tiledTile1.Animation.Select(frame => new StaticTile(layer, selectedTileSheet, BlendMode.Alpha, frame.TileId)).ToArray();
            return new AnimatedTile(layer, array, tiledTile1.Animation[0].Duration);
        }

        internal void LoadObjects(Map map)
        {
            List<TiledObjectGroup> objectGroups = TiledMap.ObjectGroups;
            if (objectGroups == null)
                return;
            Action<TiledObjectGroup> action1 = objectGroup =>
           {
               Layer layer = map.GetLayer(objectGroup.Name);
               foreach (TiledObject tiledObject in objectGroup.Objects)
               {
                   if (tiledObject.Name == "TileData")
                   {
                       int tileX = tiledObject.XPos / 16;
                       int tileWidth = tiledObject.Width / 16;
                       int tileY = tiledObject.YPos / 16;
                       int tileHeight = tiledObject.Height / 16;

                       for (int x = tileX; x < tileX + tileWidth; x++)
                           for (int y = tileY; y < tileY + tileHeight; y++)
                           {
                               Tile tile = layer.Tiles[x, y];
                               List<TiledProperty> properties = tiledObject.Properties;
                               if (properties != null && tile != null)
                               {
                                   Action<TiledProperty> action2 = prop =>
                                   {
                                       if (!tile.Properties.ContainsKey(prop.Name))
                                           tile.Properties.Add(prop.Name, prop.Value);
                                       else
                                           tile.Properties[prop.Name] = prop.Value;
                                   };
                                   properties.ForEach(action2);
                               }
                           }
                   }
               }
           };
            objectGroups.ForEach(action1);
        }

        internal void StoreTileSets(Map map, TiledMap tiledMap)
        {
            int num = 1;
            foreach (TileSheet tileSheet in map.TileSheets)
            {
                if (tileSheet.Id.StartsWith("zImageSheet_"))
                    continue;

                TiledTileSet tiledTileSet1 = new TiledTileSet();
                tiledTileSet1.FirstGid = num;
                string id = tileSheet.Id;
                tiledTileSet1.SheetName = id;
                int tileWidth = tileSheet.TileWidth;
                tiledTileSet1.TileWidth = tileWidth;
                int tileHeight = tileSheet.TileHeight;
                tiledTileSet1.TileHeight = tileHeight;
                int spacingWidth = tileSheet.SpacingWidth;
                tiledTileSet1.Spacing = spacingWidth;
                int marginWidth = tileSheet.MarginWidth;
                tiledTileSet1.Margin = marginWidth;
                int tileCount = tileSheet.TileCount;
                tiledTileSet1.TileCount = tileCount;
                int sheetWidth = tileSheet.SheetWidth;
                tiledTileSet1.Columns = sheetWidth;
                tiledTileSet1.Image = new TiledTileSetImage()
                {
                    Source = tileSheet.ImageSource,
                    Width = tileSheet.SheetWidth * tileSheet.TileWidth,
                    Height = tileSheet.SheetHeight * tileSheet.TileHeight
                };
                List<TiledTile> tiledTileList = new List<TiledTile>();
                tiledTileSet1.Tiles = tiledTileList;
                TiledTileSet tiledTileSet2 = tiledTileSet1;
                foreach (KeyValuePair<string, PropertyValue> property in tileSheet.Properties)
                {
                    if (property.Key.StartsWith("@Tile@") || property.Key.StartsWith("@TileIndex@"))
                    {
                        string[] strArray = property.Key.Split(new char[1]
                        {
              '@'
                        }, StringSplitOptions.RemoveEmptyEntries);
                        int tileIndex = int.Parse(strArray[1]);
                        string name = strArray[2];
                        TiledTile tiledTile1;
                        if ((tiledTile1 = tiledTileSet2.Tiles.FirstOrDefault(tiledTile => tiledTile.TileId == tileIndex)) != null)
                            tiledTile1.Properties.Add(new TiledProperty(name, tileSheet.Properties[property.Key]));
                        else
                            tiledTileSet2.Tiles.Add(new TiledTile()
                            {
                                TileId = tileIndex,
                                Properties = new List<TiledProperty>()
                {
                  new TiledProperty(name,  tileSheet.Properties[property.Key])
                }
                            });
                    }
                }
                tiledMap.TileSets.Add(tiledTileSet2);
                num += tileSheet.TileCount;
            }
        }

        internal void StoreLayers(Map map, TiledMap tiledMap)
        {
            foreach (Layer layer in map.Layers)
            {
                if (layer.Properties.Keys.Contains("isImageLayer") && layer.Properties["isImageLayer"] == true)
                    continue;

                TiledLayer tiledLayer1 = new TiledLayer();
                tiledLayer1.Name = layer.Id;
                tiledLayer1.Width = layer.LayerWidth;
                tiledLayer1.Height = layer.LayerHeight;
                int num1 = !layer.Visible ? 1 : 0;
                tiledLayer1.Hidden = num1 != 0;
                tiledLayer1.Data = new TiledLayerData()
                {
                    EncodingType = "csv"
                };
                List<TiledProperty> tiledPropertyList = new List<TiledProperty>();
                tiledLayer1.Properties = tiledPropertyList;
                TiledLayer tiledLayer2 = tiledLayer1;
                List<int> intList = new List<int>();
                for (int index1 = 0; index1 < layer.LayerHeight; ++index1)
                {
                    for (int index2 = 0; index2 < layer.LayerWidth; ++index2)
                    {
                        Tile tile = layer.Tiles[index2, index1];
                        if (tile is AnimatedTile animatedTile)
                        {
                            foreach (TiledTileSet tileSet in tiledMap.TileSets.Where(t => t.SheetName == animatedTile.TileSheet.Id))
                            {
                                TiledTile tiledTile1 = tileSet.Tiles.FirstOrDefault(tiledTile => tiledTile.TileId == tile.TileIndex);
                                if (tiledTile1 == null)
                                    tileSet.Tiles.Add(new TiledTile()
                                    {
                                        TileId = tile.TileIndex,
                                        Animation = ((IEnumerable<StaticTile>)animatedTile.TileFrames).Select(frame => new TiledAnimationFrame()
                                        {
                                            TileId = frame.TileIndex,
                                            Duration = (int)animatedTile.FrameInterval
                                        }).ToList()
                                    });
                                else if (tiledTile1.Animation == null)
                                    tiledTile1.Animation = ((IEnumerable<StaticTile>)animatedTile.TileFrames).Select(frame => new TiledAnimationFrame()
                                    {
                                        TileId = frame.TileIndex,
                                        Duration = (int)animatedTile.FrameInterval
                                    }).ToList();
                            }
                        }
                        int num2 = 0;
                        if (tile != null)
                        {
                            int tileIndex = tile.TileIndex;
                            TiledTileSet tiledTileSet = tiledMap.TileSets.FirstOrDefault(tileSet => tileSet.SheetName == tile.TileSheet.Id);
                            int num3 = tiledTileSet != null ? tiledTileSet.FirstGid : 1;
                            num2 = tileIndex + num3;
                        }
                        intList.Add(num2);
                    }
                }
                tiledLayer2.Data.Data = string.Join(",", intList);
                if (layer.Description.Length > 0)
                    tiledLayer2.Properties.Add(new TiledProperty("@Description", layer.Description));
                tiledMap.Layers.Add(tiledLayer2);
            }
        }

        internal void StoreObjects(Map map, TiledMap tiledMap)
        {
            foreach (Layer layer in map.Layers)
            {
                TiledObjectGroup tiledObjectGroup = new TiledObjectGroup()
                {
                    Name = layer.Id,
                    Objects = new List<TiledObject>()
                };
                for (int index1 = 0; index1 < layer.LayerHeight; ++index1)
                {
                    for (int index2 = 0; index2 < layer.LayerWidth; ++index2)
                    {
                        Tile tile = layer.Tiles[index2, index1];
                        if ((tile != null ? tile.Properties : null) != null && tile.Properties.Any<KeyValuePair<string, PropertyValue>>())
                        {
                            TiledObject tiledObject = new TiledObject()
                            {
                                ObjectId = tiledMap.NextObjectId,
                                Name = "TileData",
                                XPos = index2 * 16,
                                YPos = index1 * 16,
                                Width = 16,
                                Height = 16,
                                Properties = new List<TiledProperty>()
                            };
                            foreach (KeyValuePair<string, PropertyValue> property in tile.Properties)
                                tiledObject.Properties.Add(new TiledProperty(property.Key, property.Value));
                            tiledObjectGroup.Objects.Add(tiledObject);
                            ++tiledMap.NextObjectId;
                        }
                    }
                }
                tiledMap.ObjectGroups.Add(tiledObjectGroup);
            }
        }
    }
}
