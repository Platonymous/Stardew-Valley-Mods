using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using PyTK.ContentSync;
using Netcode;
using xTile;
using PyTK.Tiled;
using System.Xml;

namespace PyTK.Types
{
    /*
    public class PyNetField<T,TSelf> : NetField<T, TSelf> where TSelf : PyNetField<T,TSelf>
    {
        
        private NewTiledTmxFormat format = new NewTiledTmxFormat();

        public PyNetField()
        {
        }

        public PyNetField(T value) : base(value)
        {
        }

        protected override void ReadDelta(BinaryReader reader, NetVersion version)
        {
            if (value is Texture2D || typeof(T) == typeof(Texture2D))
            {

                string str = (string)null;


                if (reader.ReadBoolean())
                    str = reader.ReadString();

                if (!version.IsPriorityOver(this.ChangeVersion))
                    return;

                if (str == null)
                {
                    CleanSet((T)(object)null, false);
                    return;
                }

                Texture2D tex = JsonConvert.DeserializeObject<SerializationTexture2D>(PyNet.DecompressString(str)).getTexture();
                CleanSet((T)(object)tex, false);
            }else if (value is Map || typeof(T) == typeof(Map))
            {
                string str = (string)null;
                if (reader.ReadBoolean())
                    str = reader.ReadString();
                if (!version.IsPriorityOver(this.ChangeVersion))
                    return;

                if (str == null)
                {
                    CleanSet((T)(object)null, false);
                    return;
                }

                using (StringReader sreader = new StringReader(PyNet.DecompressString(str)))
                {
                    Map map = format.Load(XmlReader.Create(sreader));
                    CleanSet((T)(object)map, true);
                }
            }
        }

        protected override void WriteDelta(BinaryWriter writer)
        {
            if (value is Texture2D)
            {
                writer.Write(this.value != null);
                if (this.value == null)
                    return;
                writer.Write(PyNet.CompressString(JsonConvert.SerializeObject(new SerializationTexture2D((Texture2D) (object) value))));
            }else if(value is Map)
            {
                writer.Write(this.value != null);
                if (this.value == null)
                    return;
                writer.Write(PyNet.CompressString(format.AsString((Map)(object)value)));
            }
        }
    }
    */
}
