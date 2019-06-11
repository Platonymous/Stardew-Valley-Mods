using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Extensions;
using System.Collections.Generic;

namespace PyTK.Types
{
    public class MappedTexture2D : Texture2D
    {
        protected virtual Dictionary<Vector2, Texture2D> Map { get; set; } = new Dictionary<Vector2, Texture2D>();

        public MappedTexture2D(Texture2D texture, Dictionary<Vector2, Texture2D> map)
            : base(texture.GraphicsDevice, texture.Width, texture.Height)
        {
            Map = map;
        }

        public virtual void Set(Vector2 key, Texture2D value)
        {
            Map.AddOrReplace(key, value);
        }

        public virtual void Remove(Vector2 key)
        {
            Map.Remove(key);
        }

        public virtual void Clear()
        {
            Map.Clear();
        }

        public virtual Texture2D Get(float x, float y)
        {
            return Get(new Vector2(x, y));
        }

        public virtual Texture2D Get(int x, int y)
        {
            return Get(new Vector2(x, y));
        }

        public virtual Texture2D Get(Vector2 key)
        {
            if (Map.ContainsKey(key))
                return Map[key];
            else
                return null;
        }


        public virtual IEnumerable<KeyValuePair<Vector2, Texture2D>> Each(){

            foreach (var m in Map)
                yield return new KeyValuePair<Vector2, Texture2D>(m.Key, Get(m.Key));

        }
    }
}
