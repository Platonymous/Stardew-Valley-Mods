using System.Xml.Linq;

namespace PyTK.Tiled
{
    public class TiledLayerData : XmlObject, IXmlFormatable
    {
        public string EncodingType { get; set; }
        public string Data { get; set; }

        public TiledLayerData()
          : base(null)
        {
        }

        public TiledLayerData(XElement elem)
          : base(elem)
        {
            EncodingType = elem.Value<string>("@encoding");
            Data = elem.Value;
        }

        public XElement ToXml()
        {
            return new XElement("data", new object[2]
            {
         new XAttribute( "encoding",  EncodingType),
         Data
            });
        }
    }
}
