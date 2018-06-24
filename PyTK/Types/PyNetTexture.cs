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

namespace PyTK.Types
{
    /*
    public class PyNetTexture : NetField<Texture2D, PyNetTexture>
    {
        
        public PyNetTexture()
        {
        }

        public PyNetTexture(Texture2D value) : base(value)
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
                CleanSet(null, false);
                return;
            }

            Texture2D tex = JsonConvert.DeserializeObject<SerializationTexture2D>(PyNet.DecompressString(str)).getTexture();
            CleanSet(tex, false);
        }

        protected override void WriteDelta(BinaryWriter writer)
        {
            writer.Write(this.value != null);
            if (this.value == null)
                return;
            writer.Write(PyNet.CompressString(JsonConvert.SerializeObject(new SerializationTexture2D(value))));
        }
        
    }
    */
}
