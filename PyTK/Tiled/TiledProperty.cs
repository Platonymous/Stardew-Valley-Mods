using System.Xml.Linq;

namespace PyTK.Tiled
{
    public class TiledProperty : XmlObject, IXmlFormatable
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public TiledProperty(string name, string value)
          : base(null)
        {
            Name = name;
            Value = value;
        }

        public TiledProperty(XElement elem)
          : base(elem)
        {
            Name = elem.Value<string>("@name");
            Value = elem.Value<string>("@value");
        }

        public XElement ToXml()
        {
            return new XElement("property", new object[2]
            {
         new XAttribute( "name",  Name),
         new XAttribute( "value",  Value)
            });
        }
    }
}
