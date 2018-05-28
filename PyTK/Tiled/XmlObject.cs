using System.Xml.Linq;

namespace PyTK.Tiled
{
    public class XmlObject
    {
        public XElement XElement { get; }

        public XmlObject(XElement elem)
        {
            XElement = elem;
        }
    }
}
