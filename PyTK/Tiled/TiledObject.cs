using System;
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
          : base((XElement)null)
        {
        }

        public TiledObject(XElement elem)
          : base(elem)
        {
            this.ObjectId = elem.Value<int>("@id");
            this.Name = elem.Value<string>("@name");
            this.XPos = elem.Value<int>("@x");
            this.YPos = elem.Value<int>("@y");
            this.Width = elem.Value<int>("@width");
            this.Height = elem.Value<int>("@height");
            XElement xelement;
            this.Properties = (xelement = elem.Element((XName)"properties")) != null ? xelement.Elements((XName)"property").Select<XElement, TiledProperty>((Func<XElement, TiledProperty>)(prop => new TiledProperty(prop))).ToList<TiledProperty>() : (List<TiledProperty>)null;
        }

        public XElement ToXml()
        {
            return new XElement((XName)"object", new object[7]
            {
        (object) new XAttribute((XName) "id", (object) this.ObjectId),
        (object) new XAttribute((XName) "name", (object) this.Name),
        (object) new XAttribute((XName) "x", (object) this.XPos),
        (object) new XAttribute((XName) "y", (object) this.YPos),
        (object) new XAttribute((XName) "width", (object) this.Width),
        (object) new XAttribute((XName) "height", (object) this.Height),
        (object) XmlUtils.If(this.Properties.Any<TiledProperty>(), (XObject) new XElement((XName) "properties", (object) this.Properties.Select<TiledProperty, XElement>((Func<TiledProperty, XElement>) (prop => prop.ToXml()))))
            });
        }
    }
}
