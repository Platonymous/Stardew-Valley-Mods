using System.Xml.Linq;

namespace PyTK.Tiled
{
    internal class TiledProperty : XmlObject, IXmlFormatable
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public TiledProperty(string name, string value)
          : base((XElement)null)
        {
            this.Name = name;
            this.Value = value;
        }

        public TiledProperty(XElement elem)
          : base(elem)
        {
            this.Name = elem.Value<string>("@name");
            this.Value = elem.Value<string>("@value");
        }

        public XElement ToXml()
        {
            return new XElement((XName)"property", new object[2]
            {
        (object) new XAttribute((XName) "name", (object) this.Name),
        (object) new XAttribute((XName) "value", (object) this.Value)
            });
        }
    }
}
