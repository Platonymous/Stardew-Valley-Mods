using System.Xml.Linq;

namespace PyTK.Tiled
{
    internal interface IXmlFormatable
    {
        XElement ToXml();
    }
}
