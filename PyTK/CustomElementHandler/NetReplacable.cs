using Netcode;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SObject = StardewValley.Object;

namespace PyTK.CustomElementHandler
{
    public class NetReplacable : NetRef<INetObject<INetSerializable>> 
    {
        public NetReplacable(INetObject<INetSerializable> item)
            : base(item)
        {

        }

        public override INetObject<INetSerializable> Get()
        {
            INetObject<INetSerializable> obj = base.Get();
            if (SaveHandler.inSync && obj is ISaveElement ise)
            {
                base.Set((INetObject<INetSerializable>)ise.getReplacement());
                return base.Get();
            }
            else
            {
                if (!SaveHandler.inSync && !(obj is ISaveElement))
                {
                    base.Set((INetObject<INetSerializable>)SaveHandler.RebuildObject(obj));
                    return base.Get();
                }
                else
                    return obj;
            }
        }

    }
}
