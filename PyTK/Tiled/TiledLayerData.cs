using System.Xml.Linq;

namespace PyTK.Tiled
{
    internal class TiledLayerData : XmlObject, IXmlFormatable
    {
        public string EncodingType { get; set; }

        public string Data { get; set; }

        public TiledLayerData()
          : base((XElement)null)
        {
        }

        public TiledLayerData(XElement elem)
          : base(elem)
        {
            this.EncodingType = elem.Value<string>("@encoding");
            this.Data = elem.Value;
        }

        public XElement ToXml()
        {
            return new XElement((XName)"data", new object[2]
            {
        (object) new XAttribute((XName) "encoding", (object) this.EncodingType),
        (object) this.Data
            });
        }
    }
}
