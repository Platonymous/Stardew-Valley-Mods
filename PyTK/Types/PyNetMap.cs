using xTile;
using Netcode;
using System.IO;
using PyTK.Tiled;
using System.Xml;

namespace PyTK.Types
{
    /*
    public class PyNetMap : NetField<Map,PyNetMap>
    {
        private NewTiledTmxFormat format = new NewTiledTmxFormat();

        public PyNetMap()
        {
        }

        public PyNetMap(Map value) : base(value)
        {
        }

        protected override void ReadDelta(BinaryReader reader, NetVersion version)
        {
            string str = (string)null;
            if (reader.ReadBoolean())
                str = reader.ReadString();
            if (!version.IsPriorityOver(this.ChangeVersion))
                return;

            if (str == null)
            {
                CleanSet(null, true);
                return;
            }

            using (StringReader sreader = new StringReader(PyNet.DecompressString(str)))
            {
                Map map = format.Load(XmlReader.Create(sreader));
                CleanSet(map, true);
            }
        }

        protected override void WriteDelta(BinaryWriter writer)
        {
            writer.Write(this.value != null);
            if (this.value == null)
                return;
            writer.Write(PyNet.CompressString(format.AsString(value)));
        }
    }
    */
}
