using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyTK.Types
{
    public interface IInterceptor
    {
        IManifest Mod { get; }
        Type DataType { get; }

    }


    public interface IInterceptor<T> : IInterceptor
    {
        Func<T, object,string, T> Handler { get; }
    }

}
