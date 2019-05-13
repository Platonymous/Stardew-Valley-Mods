using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PyTK.Tiled
{
    public class TiledImageLayer : XmlObject, IXmlFormatable
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public float Horizontal { get; set; }
        public float Vertical { get; set; }
        public float Transparency { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public bool Hidden { get; set; }

        public List<TiledProperty> Properties { get; set; }

        public TiledImageLayer()
          : base(null)
        {
        }

        public TiledImageLayer(XElement elem)
          : base(elem)
        {
            Name = elem.Value<string>("@name");
            Horizontal = elem.Value<float?>("@offsetx") ?? 0;
            Vertical = elem.Value<float?>("@offsety") ?? 0;
            Transparency = elem.Value<float?>("@opacity") ?? 1;
            Hidden = (elem.Value<int?>("@visible") ?? 1) == 0;
            XElement xImage = elem.Element("image");
            Source = xImage.Value<string>("@source");
            ImageWidth = xImage.Value<int>("@width");
            ImageHeight = xImage.Value<int>("@height");

            if (elem.Element("properties") is XElement xelement)
                Properties = xelement.Elements("property").Select(prop => new TiledProperty(prop)).ToList();
            else
                Properties = null;
        }

        public XElement ToXml()
        {
            return new XElement("layer", new object[7]
            {
         new XAttribute( "name",  Name),
         new XAttribute( "offsetx",  Horizontal),
         new XAttribute( "offsety",  Vertical),
         XmlUtils.If(Transparency != 1, new XAttribute( "opacity",  Transparency)),
         new XElement("image", new object[3]
         {
             new XAttribute( "source",  Source),
             new XAttribute( "width",  ImageWidth),
             new XAttribute("height", ImageHeight)
         }),
         XmlUtils.If(Hidden,  new XAttribute( "visible",  0)),
         XmlUtils.If(Properties.Any(),  new XElement( "properties",  Properties.Select( prop => prop.ToXml())))
            });
        }
    }
}
