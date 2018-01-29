using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PyTK.Tiled
{
    internal class TiledObject : XmlObject, IXmlFormatable
    {
        public int ObjectId { get; set; }
        public string Name { get; set; }
        public int XPos { get; set; }
        public int YPos { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<TiledProperty> Properties { get; set; }

        public TiledObject()
          : base(null)
        {
        }

        public TiledObject(XElement elem)
          : base(elem)
        {
            ObjectId = elem.Value<int>("@id");
            Name = elem.Value<string>("@name");
            XPos = elem.Value<int>("@x");
            YPos = elem.Value<int>("@y");
            Width = elem.Value<int>("@width");
            Height = elem.Value<int>("@height");
            XElement xelement;
            Properties = (xelement = elem.Element("properties")) != null ? xelement.Elements("property").Select(prop => new TiledProperty(prop)).ToList() : null;
        }

        public XElement ToXml()
        {
            return new XElement("object", new object[7]
            {
         new XAttribute( "id",  ObjectId),
         new XAttribute( "name",  Name),
         new XAttribute( "x",  XPos),
         new XAttribute( "y",  YPos),
         new XAttribute( "width",  Width),
         new XAttribute( "height",  Height),
         XmlUtils.If(Properties.Any(),  new XElement( "properties",  Properties.Select( prop => prop.ToXml())))
            });
        }
    }
}
