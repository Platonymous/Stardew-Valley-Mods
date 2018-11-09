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
            Sound.IsLooped = loop;
            Play();
        }

        public void SetVolume(float volume)
        {
            if (Sound is SoundEffectInstance instance && !instance.IsDisposed)
                instance.Volume = Math.Max(0,Math.Min(volume,1));
        }

        public void Stop()
        {
            IsPlaying = false;
            Sound?.Stop();
        }

        public void Play()
        {
            UpdateThread = new Thread(Update);
            UpdateThread.Start();
        }

        public void Dispose()
        {
            Stop();
            Sound.Dispose();
        }

        public void Update()
        {
            Sound?.Play();
            IsPlaying = true;

            while (!Sound.IsDisposed && Sound.State == SoundState.Playing && IsPlaying && LinkedCue is Cue c)
            {
                float vol = c.GetVariable("Volume");
                SetVolume(vol != 0 ? vol : (Ambient ? Game1.musicPlayerVolume : Game1.ambientPlayerVolume));
            }

            IsPlaying = false;
        }
    }
}
