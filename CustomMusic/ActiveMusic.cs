using Microsoft.Xna.Framework;
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
        public Thread UpdateThread { get; set; }

        public AudioEmitter Emitter { get; set; } = null;
        public AudioListener Listener { get; set; } = null;

        public bool IsEmitter { get; set; } = false;

        public float Distance { get; set; } = 1;
        public float Volume { get; set; } = 1;

        public int MaxDistance { get; set; } = 100;

        public Vector2 EmitterTile { get; set; } = Vector2.Zero;

        public ActiveMusic()
        {

        }

        public ActiveMusic(string id, SoundEffectInstance sound, bool ambient, bool loop)
        {
            this.Id = id;
            this.Sound = sound;
            this.Ambient = ambient;
            this.Loop = loop;
            Sound.IsLooped = loop;
            Play();
        }

        public ActiveMusic(string id, SoundEffectInstance sound, bool ambient, bool loop, Vector2 position, float distance, float volume, int maxDistance)
        {
            this.Id = id;
            this.Sound = sound;
            this.Ambient = ambient;
            this.Loop = loop;
            Sound.IsLooped = loop;
            Emitter = new AudioEmitter();
            Distance = distance;
            EmitterTile = position;
            var ePos = (position * new Vector2(Game1.tileSize, Game1.tileSize)) + new Vector2(Game1.tileSize / 2f, Game1.tileSize / 2f);
            Emitter.Position = new Vector3(0, ePos.X * distance, ePos.Y * distance); ;
            Listener = new AudioListener();
            IsEmitter = true;
            Volume = volume;
            MaxDistance = maxDistance * maxDistance;
            Play();
        }

        public void SetVolume(float volume)
        {
            try
            {


                if (Sound is SoundEffectInstance instance && !instance.IsDisposed)
                {
                    instance.Volume = Math.Max(0, Math.Min(volume, 1));
                    if (IsEmitter)
                        instance.Volume = Math.Min(instance.Volume * Volume, 1);
                }
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
            if (IsEmitter)
            {
                var position = Game1.player.Position;
                Listener.Position = new Vector3(0,position.X * Distance, position.Y * Distance);
                Sound?.Apply3D(new AudioListener[] { Listener }, Emitter);
            }

            Sound?.Play();
            IsPlaying = true;

            UpdateThread = new Thread(Update);
            UpdateThread.Start();
        }

        public void Dispose()
        {
            Stop();
        }

        public void Update()
        {
            
            try
            {
                while (!Sound.IsDisposed && IsPlaying && Sound.State == SoundState.Playing)
                {
                    if (IsEmitter)
                    {
                        var position = Game1.player.Position;
                        var t = new Vector3(0,position.X * Distance, position.Y * Distance);
                        if (t != Listener.Position)
                        {
                            Listener.Position = t;
                            Sound?.Apply3D(Listener, Emitter);
                        }
                    }

                        float mainvol = (Ambient ? Game1.ambientPlayerVolume : Game1.musicPlayerVolume);
                        float optionsvol = (Ambient ? Game1.options.ambientVolumeLevel : Game1.options.musicVolumeLevel);

                    if (IsEmitter && MaxDistance < GetSquaredDistance(Game1.player.getTileLocation(), EmitterTile))
                        optionsvol = 0f;
                        
                        SetVolume(Math.Min(optionsvol, mainvol));

                    Thread.Sleep(1);
                }
            }
            catch(Exception e)
            {
                CustomMusicMod.SMonitor.Log(e.Message + ";" + e.StackTrace, StardewModdingAPI.LogLevel.Error);
            }

            IsPlaying = false;
        }

        public static float GetSquaredDistance(Vector2 point1, Vector2 point2)
        {
            float a = (point1.X - point2.X);
            float b = (point1.Y - point2.Y);
            return (a * a) + (b * b);
        }
    }
}
