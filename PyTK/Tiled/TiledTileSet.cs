using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PyTK.Tiled
{
    public class TiledTileSet : XmlObject, IXmlFormatable
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
                return FirstGid + TileCount - 1;
            }
        }

        public TiledTileSet()
          : base(null)
        {
        }

        public TiledTileSet(XElement elem)
          : base(elem)
        {
            FirstGid = elem.Value<int>("@firstgid");
            SheetName = elem.Value<string>("@name");
            TileWidth = elem.Value<int>("@tilewidth");
            TileHeight = elem.Value<int>("@tileheight");
            TileCount = elem.Value<int>("@tilecount");
            Columns = elem.Value<int>("@columns");
            int? nullable = elem.Value<int?>("@spacing");
            Spacing = nullable ?? 0;
            nullable = elem.Value<int?>("@margin");
            Margin = nullable ?? 0;
            XElement elem1;
            Image = (elem1 = elem.Element("image")) != null ? new TiledTileSetImage(elem1) : null;
            Tiles = elem.Elements("tile").Select(tile => new TiledTile(tile)).ToList();
        }

        public XElement ToXml()
        {
            return new XElement("tileset", new object[8]
            {
         new XAttribute( "firstgid",  FirstGid),
         new XAttribute( "name",  SheetName),
         new XAttribute( "tilewidth",  TileWidth),
         new XAttribute( "tileheight",  TileHeight),
         new XAttribute( "tilecount",  TileCount),
         new XAttribute( "columns",  Columns),
         Image.ToXml(),
         Tiles.Select( tile => tile.ToXml())
            });
        }
    }
}
