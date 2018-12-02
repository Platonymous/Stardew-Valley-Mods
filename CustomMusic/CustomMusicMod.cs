using StardewModdingAPI;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Ogg2XNA;
using Harmony;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using StardewValley;
using System;

namespace CustomMusic
{
    public class CustomMusicMod : Mod
    {
        internal static HashSet<StoredMusic> Music = new HashSet<StoredMusic>();
        internal static HashSet<ActiveMusic> Active = new HashSet<ActiveMusic>();
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;

        public override void Entry(IModHelper helper)
        {
            SMonitor = Monitor;
            SHelper = Helper;
            loadContentPacks();
            var harmony = HarmonyInstance.Create("Platonymous.CustomMusic");
            harmony.Patch(typeof(AudioCategory).GetMethod("SetVolume"), new HarmonyMethod(typeof(Overrides), "SetVolume"));
            harmony.Patch(typeof(Cue).GetMethod("Stop"), new HarmonyMethod(typeof(Overrides), "Stop"));
            harmony.Patch(typeof(Cue).GetMethod("Play"), new HarmonyMethod(typeof(Overrides), "Play"));
            harmony.Patch(typeof(Cue).GetMethod("Dispose"), new HarmonyMethod(typeof(Overrides), "Dispose"));
            harmony.Patch(typeof(Cue).GetMethod("SetVariable"), new HarmonyMethod(typeof(Overrides), "SetVariable"));
            harmony.Patch(typeof(Cue).GetProperty("IsPlaying").GetGetMethod(false), new HarmonyMethod(typeof(Overrides), "IsPlaying"));
            harmony.Patch(typeof(SoundBank).GetMethod("GetCue"), new HarmonyMethod(typeof(Overrides), "GetCue"));
        }

        public static Type getTypeSDV(string type)
        {
            string prefix = "StardewValley.";
            Type defaulSDV = Type.GetType(prefix + type + ", Stardew Valley");

            if (defaulSDV != null)
                return defaulSDV;
            else
                return Type.GetType(prefix + type + ", StardewValley");

        }

        private void loadContentPacks()
        {
            foreach (IContentPack pack in Helper.GetContentPacks())
            {
                MusicContent content = pack.ReadJsonFile<MusicContent>("content.json");

                foreach (MusicItem music in content.Music)
                {
                    string path = Path.Combine(pack.DirectoryPath, music.File);
   
                    if (!music.Preload)
                        Task.Run(() =>
                        {
                            addMusic(path, music);
                        });
                    else
                        addMusic(path, music);
                }
            }
        }

        private void addMusic(string path, MusicItem music)
        {
            Monitor.Log("Loading " + music.File + " ...", LogLevel.Info);
            SoundEffect soundEffect = OggLoader.Load(path);
            Monitor.Log(music.File + " Loaded", LogLevel.Trace);
            string[] ids = music.Id.Split(',').Select(p => p.Trim()).ToArray();
            foreach(string id in ids)
                Music.Add(new StoredMusic(id, soundEffect, music.Ambient, music.Loop, music.Conditions));
        }

        public static bool checkConditions(string condition)
        {
            if (condition == null || condition == "")
                return true;

            GameLocation location = Game1.currentLocation;
            if (location == null)
                location = Game1.getFarm();

            if (location == null)
                return false;
            else
                return (SHelper.Reflection.GetMethod(location, "checkEventPrecondition").Invoke<int>("9999999/" + condition) != -1);
        }
    }
}

