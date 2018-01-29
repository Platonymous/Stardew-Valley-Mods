using System.Xml.Linq;

namespace PyTK.Tiled
{
    internal class TiledTileSetImage : XmlObject, IXmlFormatable
    {
        public string Source { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public TiledTileSetImage()
          : base(null)
        {
        }

        public TiledTileSetImage(XElement elem)
          : base(elem)
        {
            Source = elem.Value<string>("@source");
            Width = elem.Value<int>("@width");
            Height = elem.Value<int>("@height");
        }

        public XElement ToXml()
        {
            return new XElement("image", new object[3]
            {
         new XAttribute( "source",  Source),
         new XAttribute( "width",  Width),
         new XAttribute( "height",  Height)
            });
        }
    }
}
