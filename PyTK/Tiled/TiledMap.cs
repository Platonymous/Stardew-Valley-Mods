using System;
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
          : base((XElement)null)
        {
        }

        public TiledMap(XElement elem)
          : base(elem)
        {
            this.Version = elem.Value<string>("@version");
            this.Orientation = elem.Value<string>("@orientation");
            this.RenderOrder = elem.Value<string>("@renderorder");
            this.Width = elem.Value<int>("@width");
            this.Height = elem.Value<int>("@height");
            this.TileWidth = elem.Value<int>("@tilewidth");
            this.TileHeight = elem.Value<int>("@tileheight");
            this.NextObjectId = elem.Value<int>("@nextobjectid");
            XElement xelement;
            this.Properties = (xelement = elem.Element((XName)"properties")) != null ? xelement.Elements((XName)"property").Select<XElement, TiledProperty>((Func<XElement, TiledProperty>)(prop => new TiledProperty(prop))).ToList<TiledProperty>() : (List<TiledProperty>)null;
            this.TileSets = elem.Elements((XName)"tileset").Select<XElement, TiledTileSet>((Func<XElement, TiledTileSet>)(tileSet => new TiledTileSet(tileSet))).ToList<TiledTileSet>();
            this.Layers = elem.Elements((XName)"layer").Select<XElement, TiledLayer>((Func<XElement, TiledLayer>)(layer => new TiledLayer(layer))).ToList<TiledLayer>();
            this.ObjectGroups = elem.Elements((XName)"objectgroup").Select<XElement, TiledObjectGroup>((Func<XElement, TiledObjectGroup>)(objectGroup => new TiledObjectGroup(objectGroup))).ToList<TiledObjectGroup>();
        }

        public XElement ToXml()
        {
            return new XElement((XName)"map", new object[12]
            {
        (object) new XAttribute((XName) "version", (object) this.Version),
        (object) new XAttribute((XName) "orientation", (object) this.Orientation),
        (object) new XAttribute((XName) "renderorder", (object) this.RenderOrder),
        (object) new XAttribute((XName) "width", (object) this.Width),
        (object) new XAttribute((XName) "height", (object) this.Height),
        (object) new XAttribute((XName) "tilewidth", (object) this.TileWidth),
        (object) new XAttribute((XName) "tileheight", (object) this.TileHeight),
        (object) new XAttribute((XName) "nextobjectid", (object) this.NextObjectId),
        (object) XmlUtils.If(this.Properties.Any<TiledProperty>(), (XObject) new XElement((XName) "properties", (object) this.Properties.Select<TiledProperty, XElement>((Func<TiledProperty, XElement>) (prop => prop.ToXml())))),
        (object) this.TileSets.Select<TiledTileSet, XElement>((Func<TiledTileSet, XElement>) (tileSet => tileSet.ToXml())),
        (object) this.Layers.Select<TiledLayer, XElement>((Func<TiledLayer, XElement>) (layer => layer.ToXml())),
        (object) this.ObjectGroups.Select<TiledObjectGroup, XElement>((Func<TiledObjectGroup, XElement>) (objectGroup => objectGroup.ToXml()))
            });
        }
    }
}
