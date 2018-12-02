using Microsoft.Xna.Framework.Audio;
using StardewValley;
using System;
using System.Threading;

namespace CustomMusic
{
    public class ActiveMusic : IDisposable
    {
        public string Id { get; set; }
        public SoundEffectInstance Sound { get; set; } = null;
        public bool Ambient { get; set; } = false;
        public bool IsPlaying { get; set; } = false;
        public bool Loop { get; set; } = false;
        public Cue LinkedCue { get; set; } = null;
        public Thread UpdateThread { get; set; }

        public ActiveMusic()
        {

        }

        public ActiveMusic(string id, SoundEffectInstance sound, ref Cue linkedCue, bool ambient, bool loop)
        {
            this.Id = id;
            this.Sound = sound;
            this.LinkedCue = linkedCue;
            this.Ambient = ambient;
            this.Loop = loop;
            Sound.IsLooped = loop;
            Play();
        }

        public void SetVolume(float volume)
        {
            try
            {
                if (Sound is SoundEffectInstance instance && !instance.IsDisposed)
                    instance.Volume = Math.Max(0, Math.Min(volume, 1));
            }
            catch
            {

            }
        }

        public void Stop()
        {
            try
            {
                IsPlaying = false;
                Sound?.Stop();
            }
            catch
            {
            }
        }

        public void Play()
        {
            UpdateThread = new Thread(Update);
            UpdateThread.Start();
        }

        public void Dispose()
        {
            Stop();
        }

        public void Update()
        {
            Sound?.Play();
            IsPlaying = true;
            try
            {
                while (!Sound.IsDisposed && IsPlaying && LinkedCue is Cue c)
                {
                    float vol = c.GetVariable("Volume");
                    SetVolume(vol != 0 ? vol : (Ambient ? Game1.musicPlayerVolume : Game1.ambientPlayerVolume));
                }
            }
            catch
            {

            }

            IsPlaying = false;
        }
    }
}
