using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace PyTK.Types
{
    public class MappedTexture2D : Texture2D
    {
        protected virtual Dictionary<Rectangle?, Texture2D> Map { get; set; } = new Dictionary<Rectangle?, Texture2D>();

        public MappedTexture2D(Texture2D texture, Dictionary<Rectangle?, Texture2D> map)
            : base(texture.GraphicsDevice, texture.Width, texture.Height)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            SetData<Color>(data);
            Map = map;
        }

        public virtual void Set(Rectangle? key, Texture2D value)
        {
            if(key.HasValue)
                Map.AddOrReplace(key, value);
        }

        public virtual void Remove(Rectangle? key)
        {
            if (key.HasValue)
                Map.Remove(key);
        }

        public virtual void Clear()
        {
            Map.Clear();
        }

        public virtual Rectangle? GetSourceRectangle(Rectangle? key)
        {
            if (key.HasValue && Map.Keys.FirstOrDefault(k => k.HasValue && k.Value.Contains(key.Value)) is Rectangle r)
                return new Rectangle(key.Value.X - r.X, key.Value.Y - r.Y, key.Value.Width, key.Value.Height);
            else
                return null;
        }

        public virtual Texture2D Get(Rectangle? key)
        {
            if (key.HasValue && Map.Keys.FirstOrDefault(k => k.HasValue && k.Value.Contains(key.Value)) is Rectangle r)
                return Map[r];
            else
                return null;
        }

        public virtual KeyValuePair<Rectangle?, Texture2D> GetPair(Rectangle? key)
        {
            if (key.HasValue && Map.FirstOrDefault(k => k.Key.HasValue && k.Key.Value.Contains(key.Value)) is KeyValuePair<Rectangle?, Texture2D> r && r.Key.HasValue)
                return new KeyValuePair<Rectangle?, Texture2D>(new Rectangle(key.Value.X - r.Key.Value.X, key.Value.Y - r.Key.Value.Y, key.Value.Width, key.Value.Height),r.Value);
            else
                return new KeyValuePair<Rectangle?, Texture2D>(null,null);
        }


        public virtual IEnumerable<KeyValuePair<Rectangle?, Texture2D>> Each(){

            foreach (var m in Map)
                yield return new KeyValuePair<Rectangle?, Texture2D>(m.Key, Get(m.Key));

        }
    }
}
