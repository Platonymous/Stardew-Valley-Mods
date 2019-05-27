using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PyTK.Extensions;
using Microsoft.Xna.Framework;

namespace PyTK.Types
{
    public class AnimatedTexture2D : ScaledTexture2D
    {
        private List<Texture2D> Frames = new List<Texture2D>();
        public int CurrentFrame = 0;
        private int SkipFrame = 0;
        private int Counter = 0;
        public bool Paused { get; set; } = false;
        private bool Loop = true;

        public override Texture2D STexture {
            get {
                if (Paused)
                    return Frames[CurrentFrame];

                Counter++;
                Counter = Counter > SkipFrame ? 0 : Counter;
                if (Counter % SkipFrame == 0)
                {
                    CurrentFrame++;
                    Counter = 0;
                }
                CurrentFrame = CurrentFrame >= Frames.Count ? Loop ? 0 : Frames.Count - 1 : CurrentFrame;
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
            : base(spriteSheet,tileWidth,tileHeight)
        {
            Paused = startPaused;
            Loop = loop;
            Scale = scale;
            SetSpeed(fps);
            
            int tiles = (spriteSheet.Width / tileWidth) * (spriteSheet.Height / tileHeight);
            for (int t = 0; t < tiles; t++)
                Frames.Add(spriteSheet.getTile(t, tileWidth, tileHeight));

            Color[] data = new Color[(int)((int)(tileWidth/scale) * (int)(tileHeight/scale))];
            PyUtils.getRectangle((int)(tileWidth / scale), (int)(tileHeight / scale),Color.White).GetData(data);
            SetData(data);
        }
    }
}
