using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PyTK.Tiled
{
    internal class TiledTileSet : XmlObject, IXmlFormatable
    {
        public int FirstGid { get; set; }
        public string SheetName { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int TileCount { get; set; }
        public int Columns { get; set; }
        public int Spacing { get; set; }
        public int Margin { get; set; }

        public TiledTileSetImage Image { get; set; }

        public List<TiledTile> Tiles { get; set; }

        public int LastGid
        {
            get
            {
                return this.FirstGid + this.TileCount - 1;
            }
        }

        public TiledTileSet()
          : base((XElement)null)
        {
        }

        public TiledTileSet(XElement elem)
          : base(elem)
        {
            this.FirstGid = elem.Value<int>("@firstgid");
            this.SheetName = elem.Value<string>("@name");
            this.TileWidth = elem.Value<int>("@tilewidth");
            this.TileHeight = elem.Value<int>("@tileheight");
            this.TileCount = elem.Value<int>("@tilecount");
            this.Columns = elem.Value<int>("@columns");
            int? nullable = elem.Value<int?>("@spacing");
            this.Spacing = nullable ?? 0;
            nullable = elem.Value<int?>("@margin");
            this.Margin = nullable ?? 0;
            XElement elem1;
            this.Image = (elem1 = elem.Element((XName)"image")) != null ? new TiledTileSetImage(elem1) : (TiledTileSetImage)null;
            this.Tiles = elem.Elements((XName)"tile").Select<XElement, TiledTile>((Func<XElement, TiledTile>)(tile => new TiledTile(tile))).ToList<TiledTile>();
        }

        public XElement ToXml()
        {
            return new XElement((XName)"tileset", new object[8]
            {
        (object) new XAttribute((XName) "firstgid", (object) this.FirstGid),
        (object) new XAttribute((XName) "name", (object) this.SheetName),
        (object) new XAttribute((XName) "tilewidth", (object) this.TileWidth),
        (object) new XAttribute((XName) "tileheight", (object) this.TileHeight),
        (object) new XAttribute((XName) "tilecount", (object) this.TileCount),
        (object) new XAttribute((XName) "columns", (object) this.Columns),
        (object) this.Image.ToXml(),
        (object) this.Tiles.Select<TiledTile, XElement>((Func<TiledTile, XElement>) (tile => tile.ToXml()))
            });
        }
    }
}
