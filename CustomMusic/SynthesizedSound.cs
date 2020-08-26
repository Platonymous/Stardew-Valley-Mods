using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomMusic
{
    public enum WaveForm
    {
        Sine,
        Square,
        Saw,
        Triangle,
        Noise
    }

    public class SynthesizedSound
    {
        private readonly SoundEffect Sound;
        private readonly Random random;
        public SynthesizedSound(int sampleRate = 44100, int length = 1, float frequenzy = 440f, WaveForm form = WaveForm.Sine)
        {
            random = new Random((int)frequenzy);
            int s = sizeof(short);
            byte[] waveByteData = new byte[sampleRate * s * length];

            for (int i = 0; i < sampleRate * length; i++)
            {
                byte[] sampleBytes;
                switch (form)
                {
                    case WaveForm.Sine: sampleBytes = GetSineWave(frequenzy, sampleRate, i); break;
                    case WaveForm.Square: sampleBytes = GetSquareWave(frequenzy, sampleRate, i);break;
                    case WaveForm.Noise: sampleBytes = GetNoiseWave(frequenzy, sampleRate, i); break;
                    default: sampleBytes = GetSineWave(frequenzy, sampleRate, i); break;
                }
                waveByteData[i * s] = sampleBytes[0];
                waveByteData[i * s + 1] = sampleBytes[1];
            }

            Sound = new SoundEffect(waveByteData, sampleRate, AudioChannels.Mono);
        }

        private byte[] GetSineWave(float frequenzy,int sampleRate, int i)
        {
            return BitConverter
                .GetBytes(
                Convert.ToInt16(short.MaxValue * Math.Sin(((Math.PI * 2 * frequenzy) / sampleRate) * i)));
        }

        private byte[] GetSquareWave(float frequenzy, int sampleRate, int i)
        {
            return BitConverter
                .GetBytes(
                Convert.ToInt16(short.MaxValue * Math.Sign(Math.Sin((Math.PI * 2 * frequenzy) / sampleRate * i))));
        }

        private byte[] GetNoiseWave(float frequenzy, int sampleRate, int i)
        {
            return BitConverter
                .GetBytes(
                Convert.ToInt16(random.Next(-short.MaxValue,short.MaxValue))
                );
        }

        public void Play(float volume = 1f, float pitch = 1f, float pan = 0f)
        {
            Sound.Play(volume,pitch,pan);
        }
    }
}
