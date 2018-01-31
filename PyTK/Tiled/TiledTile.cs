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
          : base(null)
        {
        }

        public TiledTile(XElement elem)
          : base(elem)
        {
            TileId = elem.Value<int>("@id");
            XElement xelement1;
            Properties = (xelement1 = elem.Element("properties")) != null ? xelement1.Elements("property").Select(prop => new TiledProperty(prop)).ToList() : null;
            XElement xelement2;
            Animation = (xelement2 = elem.Element("animation")) != null ? xelement2.Elements("frame").Select(frame => new TiledAnimationFrame(frame)).ToList() : null;
        }

        public XElement ToXml()
        {
            XName name1 = "tile";
            object[] objArray = new object[3]
            {
         new XAttribute( "id",  TileId),
        null,
        null
            };
            int index1 = 1;
            int num1 = Properties != null ? 1 : 0;
            XName name2 = "properties";
            List<TiledProperty> properties = Properties;
            IEnumerable<XElement> xelements1 = properties != null ? properties.Select(prop => prop.ToXml()) : null;
            XElement xelement1 = new XElement(name2, xelements1);
            XObject xobject1 = XmlUtils.If(num1 != 0, xelement1);
            objArray[index1] = xobject1;
            int index2 = 2;
            int num2 = Animation != null ? 1 : 0;
            XName name3 = "animation";
            List<TiledAnimationFrame> animation = Animation;
            IEnumerable<XElement> xelements2 = animation != null ? animation.Select(prop => prop.ToXml()) : null;
            XElement xelement2 = new XElement(name3, xelements2);
            XObject xobject2 = XmlUtils.If(num2 != 0, xelement2);
            objArray[index2] = xobject2;
            return new XElement(name1, objArray);
        }
    }
}
