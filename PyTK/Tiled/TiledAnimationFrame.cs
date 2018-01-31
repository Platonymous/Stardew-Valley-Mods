using System.Xml.Linq;

namespace PyTK.Tiled
{
    internal class TiledAnimationFrame : XmlObject, IXmlFormatable
    {
        public int TileId { get; set; }
        public int Duration { get; set; }

        public TiledAnimationFrame()
          : base(null)
        {
        }

        public TiledAnimationFrame(XElement elem)
          : base(elem)
        {
            TileId = elem.Value<int>("@tileid");
            Duration = elem.Value<int>("@duration");
        }

        public XElement ToXml()
        {
            return new XElement("frame", new object[2]
            {
         new XAttribute( "tileid",  TileId),
         new XAttribute( "duration",  Duration)
            });
        }
    }
}
