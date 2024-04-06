using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portraiture.HDP
{
    public class AnimationModel
    {
        //https://github.com/tlitookilakin/HDPortraits/blob/master/HDPortraits/Models/AnimationModel.cs
        public int HFrames { set; get; } = 1;
        public int VFrames { set; get; } = 1;
        public int Speed { get; set; } = 100;
        public List<int> Delays { get; set; } = null;

        private int currentFrame = 0;
        private int timeSinceLast = 0;

        public void Animate(int millis)
        {
            timeSinceLast += millis;
            int delay = Speed;

            if (Delays != null && Delays.Count > currentFrame && Delays[currentFrame] >= 0)
                if (Delays[currentFrame] == 0)
                    return;
                else
                    delay = Delays[currentFrame];

            if (timeSinceLast >= delay)
            {
                timeSinceLast -= delay;
                currentFrame = (currentFrame + 1) % (HFrames * VFrames);
            }
        }
        public void Reset()
        {
            timeSinceLast = 0;
            currentFrame = 0;
        }
        public Rectangle GetSourceRegion(Texture2D texture, int size, int index, int millis = -1)
        {
            int hamt = texture.Width / (size * HFrames);
            if (hamt == 0)
                return new(0, 0, size, size);

            Point pos = new(
                (index % hamt) * size * HFrames + size * (currentFrame % HFrames),
                (index / hamt) * size * VFrames + size * (currentFrame / HFrames)
            );

            if (millis > 0)
                Animate(millis);

            if (pos.Y >= texture.Height || pos.X >= texture.Width)
                pos = new(
                    size * (currentFrame % HFrames),
                    size * (currentFrame / HFrames)
                );

            return new(pos, new(size, size));
        }
    }
}
