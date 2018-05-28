using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PyTK.Tiled
{
    public class TiledLayer : XmlObject, IXmlFormatable
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Hidden { get; set; }

        public List<TiledProperty> Properties { get; set; }

        public TiledLayerData Data { get; set; }

        public TiledLayer()
          : base(null)
        {
        }

        public TiledLayer(XElement elem)
          : base(elem)
        {
            Name = elem.Value<string>("@name");
            Width = elem.Value<int>("@width");
            Height = elem.Value<int>("@height");
            Hidden = (elem.Value<int?>("@visible") ?? 1) == 0;
            XElement xelement;
            Properties = (xelement = elem.Element("properties")) != null ? xelement.Elements("property").Select(prop => new TiledProperty(prop)).ToList() : null;
            Data = new TiledLayerData(elem.Element("data"));
        }

        public XElement ToXml()
        {
            return new XElement("layer", new object[6]
            {
         new XAttribute( "name",  Name),
         new XAttribute( "width",  Width),
         new XAttribute( "height",  Height),
         XmlUtils.If(Hidden,  new XAttribute( "visible",  0)),
         XmlUtils.If(Properties.Any(),  new XElement( "properties",  Properties.Select( prop => prop.ToXml()))),
         Data.ToXml()
            });
        }
    }
}
