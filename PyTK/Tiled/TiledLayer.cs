using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PyTK.Tiled
{
    internal class TiledLayer : XmlObject, IXmlFormatable
    {
        public string Name { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool Hidden { get; set; }

        public List<TiledProperty> Properties { get; set; }

        public TiledLayerData Data { get; set; }

        public TiledLayer()
          : base((XElement)null)
        {
        }

        public TiledLayer(XElement elem)
          : base(elem)
        {
            this.Name = elem.Value<string>("@name");
            this.Width = elem.Value<int>("@width");
            this.Height = elem.Value<int>("@height");
            this.Hidden = (elem.Value<int?>("@visible") ?? 1) == 0;
            XElement xelement;
            this.Properties = (xelement = elem.Element((XName)"properties")) != null ? xelement.Elements((XName)"property").Select<XElement, TiledProperty>((Func<XElement, TiledProperty>)(prop => new TiledProperty(prop))).ToList<TiledProperty>() : (List<TiledProperty>)null;
            this.Data = new TiledLayerData(elem.Element((XName)"data"));
        }

        public XElement ToXml()
        {
            return new XElement((XName)"layer", new object[6]
            {
        (object) new XAttribute((XName) "name", (object) this.Name),
        (object) new XAttribute((XName) "width", (object) this.Width),
        (object) new XAttribute((XName) "height", (object) this.Height),
        (object) XmlUtils.If(this.Hidden, (XObject) new XAttribute((XName) "visible", (object) 0)),
        (object) XmlUtils.If(this.Properties.Any<TiledProperty>(), (XObject) new XElement((XName) "properties", (object) this.Properties.Select<TiledProperty, XElement>((Func<TiledProperty, XElement>) (prop => prop.ToXml())))),
        (object) this.Data.ToXml()
            });
        }
    }
}
