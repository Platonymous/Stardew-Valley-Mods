using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;

namespace PyTK.Types
{
    public class TextureInterceptor : IInterceptor<Texture2D>
    {
        public IManifest Mod { get;}

        public Func<Texture2D, object, string, Texture2D> Handler { get; protected set; }

        public Type DataType { get;}

        public TextureInterceptor(IManifest mod, Func<Texture2D, object, string, Texture2D> handler, Type dataType)
        {
            Mod = mod;
            Handler = handler;
            DataType = dataType;
        }
    }

    public class TextureInterceptor<TData> : TextureInterceptor
    {
        public TextureInterceptor(IManifest mod, Func<Texture2D, TData, string, Texture2D> handler)
            : base(mod,(t,o,s) => handler(t,(TData) o, s), typeof(TData))
        {

        }
    }
}
