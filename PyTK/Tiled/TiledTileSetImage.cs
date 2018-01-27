using System.Xml.Linq;

namespace PyTK.Tiled
{
    internal class TiledTileSetImage : XmlObject, IXmlFormatable
    {
        public string Source { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public TiledTileSetImage()
          : base((XElement)null)
        {
        }

        public TiledTileSetImage(XElement elem)
          : base(elem)
        {
            this.Source = elem.Value<string>("@source");
            this.Width = elem.Value<int>("@width");
            this.Height = elem.Value<int>("@height");
        }

        public XElement ToXml()
        {
            return new XElement((XName)"image", new object[3]
            {
        (object) new XAttribute((XName) "source", (object) this.Source),
        (object) new XAttribute((XName) "width", (object) this.Width),
        (object) new XAttribute((XName) "height", (object) this.Height)
            });
        }
    }
}
