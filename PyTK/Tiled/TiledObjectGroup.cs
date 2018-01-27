using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PyTK.Tiled
{
  internal class TiledObjectGroup : XmlObject, IXmlFormatable
  {
    public string Name { get; set; }

    public List<TiledObject> Objects { get; set; }

    public TiledObjectGroup()
      : base((XElement) null)
    {
    }

    public TiledObjectGroup(XElement elem)
      : base(elem)
    {
      this.Name = elem.Value<string>("@name");
      this.Objects = elem.Elements((XName) "object").Select<XElement, TiledObject>((Func<XElement, TiledObject>) (obj => new TiledObject(obj))).ToList<TiledObject>();
    }

    public XElement ToXml()
    {
      return new XElement((XName) "objectgroup", new object[2]
      {
        (object) new XAttribute((XName) "name", (object) this.Name),
        (object) this.Objects.Select<TiledObject, XElement>((Func<TiledObject, XElement>) (obj => obj.ToXml()))
      });
    }
  }
}
