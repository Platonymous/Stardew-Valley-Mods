using System.Xml.Linq;

namespace PyTK.Tiled
{
    internal class TiledAnimationFrame : XmlObject, IXmlFormatable
    {
        public int TileId { get; set; }

        public int Duration { get; set; }

        public TiledAnimationFrame()
          : base((XElement)null)
        {
        }

        public TiledAnimationFrame(XElement elem)
          : base(elem)
        {
            this.TileId = elem.Value<int>("@tileid");
            this.Duration = elem.Value<int>("@duration");
        }

        public XElement ToXml()
        {
            return new XElement((XName)"frame", new object[2]
            {
        (object) new XAttribute((XName) "tileid", (object) this.TileId),
        (object) new XAttribute((XName) "duration", (object) this.Duration)
            });
        }
    }
}
