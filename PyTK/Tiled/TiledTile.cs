using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PyTK.Tiled
{
    internal class TiledTile : XmlObject, IXmlFormatable
    {
        public int TileId { get; set; }

        public List<TiledProperty> Properties { get; set; }

        public List<TiledAnimationFrame> Animation { get; set; }

        public TiledTile()
          : base((XElement)null)
        {
        }

        public TiledTile(XElement elem)
          : base(elem)
        {
            this.TileId = elem.Value<int>("@id");
            XElement xelement1;
            this.Properties = (xelement1 = elem.Element((XName)"properties")) != null ? xelement1.Elements((XName)"property").Select<XElement, TiledProperty>((Func<XElement, TiledProperty>)(prop => new TiledProperty(prop))).ToList<TiledProperty>() : (List<TiledProperty>)null;
            XElement xelement2;
            this.Animation = (xelement2 = elem.Element((XName)"animation")) != null ? xelement2.Elements((XName)"frame").Select<XElement, TiledAnimationFrame>((Func<XElement, TiledAnimationFrame>)(frame => new TiledAnimationFrame(frame))).ToList<TiledAnimationFrame>() : (List<TiledAnimationFrame>)null;
        }

        public XElement ToXml()
        {
            XName name1 = (XName)"tile";
            object[] objArray = new object[3]
            {
        (object) new XAttribute((XName) "id", (object) this.TileId),
        null,
        null
            };
            int index1 = 1;
            int num1 = this.Properties != null ? 1 : 0;
            XName name2 = (XName)"properties";
            List<TiledProperty> properties = this.Properties;
            IEnumerable<XElement> xelements1 = properties != null ? properties.Select<TiledProperty, XElement>((Func<TiledProperty, XElement>)(prop => prop.ToXml())) : (IEnumerable<XElement>)null;
            XElement xelement1 = new XElement(name2, (object)xelements1);
            XObject xobject1 = XmlUtils.If(num1 != 0, (XObject)xelement1);
            objArray[index1] = (object)xobject1;
            int index2 = 2;
            int num2 = this.Animation != null ? 1 : 0;
            XName name3 = (XName)"animation";
            List<TiledAnimationFrame> animation = this.Animation;
            IEnumerable<XElement> xelements2 = animation != null ? animation.Select<TiledAnimationFrame, XElement>((Func<TiledAnimationFrame, XElement>)(prop => prop.ToXml())) : (IEnumerable<XElement>)null;
            XElement xelement2 = new XElement(name3, (object)xelements2);
            XObject xobject2 = XmlUtils.If(num2 != 0, (XObject)xelement2);
            objArray[index2] = (object)xobject2;
            return new XElement(name1, objArray);
        }
    }
}
