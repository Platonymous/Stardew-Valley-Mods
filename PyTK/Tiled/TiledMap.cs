using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PyTK.Tiled
{
    internal class TiledMap : XmlObject, IXmlFormatable
    {
        public string Version { get; set; } = "1.0";
        public string Orientation { get; set; } = "orthogonal";
        public string RenderOrder { get; set; } = "right-down";
        public int Width { get; set; }
        public int Height { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int NextObjectId { get; set; }
        public List<TiledProperty> Properties { get; set; }
        public List<TiledTileSet> TileSets { get; set; }
        public List<TiledLayer> Layers { get; set; }
        public List<TiledObjectGroup> ObjectGroups { get; set; }

        public TiledMap()
          : base(null)
        {
        }

        public TiledMap(XElement elem)
          : base(elem)
        {
            Version = elem.Value<string>("@version");
            Orientation = elem.Value<string>("@orientation");
            RenderOrder = elem.Value<string>("@renderorder");
            Width = elem.Value<int>("@width");
            Height = elem.Value<int>("@height");
            TileWidth = elem.Value<int>("@tilewidth");
            TileHeight = elem.Value<int>("@tileheight");
            NextObjectId = elem.Value<int>("@nextobjectid");
            XElement xelement;
            Properties = (xelement = elem.Element("properties")) != null ? xelement.Elements("property").Select(prop => new TiledProperty(prop)).ToList() : null;
            TileSets = elem.Elements("tileset").Select(tileSet => new TiledTileSet(tileSet)).ToList();
            Layers = elem.Elements("layer").Select(layer => new TiledLayer(layer)).ToList();
            ObjectGroups = elem.Elements("objectgroup").Select(objectGroup => new TiledObjectGroup(objectGroup)).ToList();
        }

        public XElement ToXml()
        {
            return new XElement("map", new object[12]
            {
         new XAttribute( "version",  Version),
         new XAttribute( "orientation",  Orientation),
         new XAttribute( "renderorder",  RenderOrder),
         new XAttribute( "width",  Width),
         new XAttribute( "height",  Height),
         new XAttribute( "tilewidth",  TileWidth),
         new XAttribute( "tileheight",  TileHeight),
         new XAttribute( "nextobjectid",  NextObjectId),
         XmlUtils.If(Properties.Any(),  new XElement( "properties",  Properties.Select( prop => prop.ToXml()))),
         TileSets.Select( tileSet => tileSet.ToXml()),
         Layers.Select( layer => layer.ToXml()),
         ObjectGroups.Select( objectGroup => objectGroup.ToXml())
            });
        }
    }
}
