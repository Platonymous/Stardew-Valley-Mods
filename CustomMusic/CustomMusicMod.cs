using StardewModdingAPI;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Ogg2XNA;
using Harmony;
using System.Collections.Generic;

namespace CustomMusic
{
    public class CustomMusicMod : Mod
    {
        internal static Dictionary<string, StoredMusic> Music = new Dictionary<string, StoredMusic>();
        internal static HashSet<ActiveMusic> Active = new HashSet<ActiveMusic>();
        internal static IMonitor SMonitor;

        public override void Entry(IModHelper helper)
        {
            SMonitor = Monitor;
            loadContentPacks();
            var harmony = HarmonyInstance.Create("Platonymous.CustomMusic");
            harmony.Patch(typeof(AudioCategory).GetMethod("SetVolume"), new HarmonyMethod(typeof(Overrides), "SetVolume"));
            harmony.Patch(typeof(Cue).GetMethod("Stop"), new HarmonyMethod(typeof(Overrides), "Stop"));
            harmony.Patch(typeof(Cue).GetMethod("Play"), new HarmonyMethod(typeof(Overrides), "Play"));
            harmony.Patch(typeof(Cue).GetMethod("Dispose"), new HarmonyMethod(typeof(Overrides), "Dispose"));
            harmony.Patch(typeof(Cue).GetMethod("SetVariable"), new HarmonyMethod(typeof(Overrides), "SetVariable"));
            harmony.Patch(typeof(Cue).GetProperty("IsPlaying").GetGetMethod(false), new HarmonyMethod(typeof(Overrides), "IsPlaying"));

        }

        private void loadContentPacks()
        {
            foreach (IContentPack pack in Helper.GetContentPacks())
            {
                MusicContent content = Helper.ReadJsonFile<MusicContent>(Path.Combine(pack.DirectoryPath, "content.json"));

                foreach (MusicItem music in content.Music)
                {
                    string path = Path.Combine(pack.DirectoryPath, music.File);
                    addMusic(path, music);
                }
            }
        }

        private void addMusic(string path, MusicItem music)
        {
            if (Music.ContainsKey(music.Id))
                Music.Remove(music.Id);

            SoundEffect soundEffect = OggLoader.Load(path);
            Music.Add(music.Id, new StoredMusic(music.Id, soundEffect, music.Ambient, music.Loop));
        }
    }

}

