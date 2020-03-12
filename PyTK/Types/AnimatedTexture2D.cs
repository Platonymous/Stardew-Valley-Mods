using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using PyTK.Extensions;
using Microsoft.Xna.Framework;

namespace PyTK.Types
{
    public class AnimatedTexture2D : ScaledTexture2D
    {
        public static uint ticked = 0;
        public List<Texture2D> Frames = new List<Texture2D>();
        public int CurrentFrame = 0;
        private int SkipFrame = 0;
        public bool Paused { get; set; } = false;
        private bool Loop = true;
        private uint lastTick = 0;

        public void Tick()
        {
            if (CurrentFrame == Frames.Count - 1 && !Loop)
                Paused = true;

            if (Paused)
                return;

            if (lastTick == ticked)
                return;
            else
                lastTick = ticked;

            if (ticked % SkipFrame == 0)
                CurrentFrame++;

            CurrentFrame = CurrentFrame >= Frames.Count ? 0 : CurrentFrame;
        }

        public override Texture2D STexture {
            get {
                return Frames[CurrentFrame];
            }
            set => base.STexture = value;
        }

        public void SetSpeed(int fps)
        {
            SkipFrame = 60 / fps;
        }

        public AnimatedTexture2D(Texture2D spriteSheet, int tileWidth, int tileHeight, int fps, bool loop = true, float scale = 1)
            :this(spriteSheet,tileWidth,tileHeight,fps,false,loop,scale)
        {

        }

        public AnimatedTexture2D(Texture2D spriteSheet, int tileWidth, int tileHeight, int fps, bool startPaused, bool loop = true, float scale = 1)
            : base(spriteSheet,(int)(tileWidth/scale), (int)(tileHeight /scale))
        {
            Paused = startPaused;
            Loop = loop;
            Scale = scale;
            SetSpeed(fps);
            
            int tiles = (spriteSheet.Width / tileWidth) * (spriteSheet.Height / tileHeight);
            for (int t = 0; t < tiles; t++)
                Frames.Add(spriteSheet.getTile(t, tileWidth, tileHeight));

            Color[] data = new Color[(int)((int)(tileWidth/scale) * (int)(tileHeight/scale))];
            spriteSheet.getArea(new Rectangle(0,0,tileWidth,tileHeight)).ScaleUpTexture(1f / scale, false).GetData(data);
            SetData(data);
        }
    }
}
