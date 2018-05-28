using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PyTK.Tiled
{
    public class TiledObjectGroup : XmlObject, IXmlFormatable
    {
        public string Name { get; set; }
        public List<TiledObject> Objects { get; set; }

        public TiledObjectGroup()
          : base(null)
        {
        }

        public TiledObjectGroup(XElement elem)
          : base(elem)
        {
            Name = elem.Value<string>("@name");
            Objects = elem.Elements("object").Select<XElement, TiledObject>(obj => new TiledObject(obj)).ToList<TiledObject>();
        }

        public XElement ToXml()
        {
            return new XElement("objectgroup", new object[2]
            {
         new XAttribute( "name",  Name),
         Objects.Select<TiledObject, XElement>( obj => obj.ToXml())
            });
        }
    }
}
