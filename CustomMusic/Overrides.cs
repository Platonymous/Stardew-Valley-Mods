using Microsoft.Xna.Framework.Audio;
using StardewValley;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomMusic
{
    public class Overrides
    {
        public static bool skip = false;
        public static KeyValuePair<string, string> nextCue = new KeyValuePair<string, string>("none","none");

        public static void Stop(Cue __instance)
        {
            foreach (ActiveMusic a in CustomMusicMod.Active.Where(m =>m.IsPlaying && m.Id == __instance.Name))
                a.Stop();
        }

        public static bool IsPlaying(ref Cue __instance, ref bool __result)
        {
            foreach (ActiveMusic a in CustomMusicMod.Active)
                if (a.Id == __instance.Name)
                {
                    __result = a.IsPlaying;
                    return false;
                }
            return true;
        }

        public static bool GetCue(SoundBank __instance, string name, ref Cue __result)
        {
            if (name.StartsWith("cm:"))
            {
                string[] n = name.Split(':');
                string next = n.Length > 2 ? n[2] : "MainTheme";
                __result = __instance.GetCue(next);
                nextCue = new KeyValuePair<string, string>(next, n[1]);
                return false;
            }

            return true;
        }

        public static void Dispose(Cue __instance)
        {
            foreach (ActiveMusic a in CustomMusicMod.Active.Where(m => m.Id == __instance.Name))
                a.Dispose();

            CustomMusicMod.Active.RemoveWhere(m => m.Id == __instance.Name);
        }

        public static bool PlayCue(SoundBank __instance, string name)
        {
            return Play3(name, __instance);
        }

        public static bool Play(ref Cue __instance)
        {
            string name = __instance.Name;
            return Play2(name, ref __instance);
        }
        public static bool Play3(string name, SoundBank soundbank)
        {
            bool custom = false;
            if (nextCue.Key == name)
            {
                custom = true;
                name = nextCue.Value;
                nextCue = new KeyValuePair<string, string>("none", "none");
            }
            bool ret = true;
            try
            {
                string preset = "Default";

                if (CustomMusicMod.config.Presets.ContainsKey(name))
                    preset = CustomMusicMod.config.Presets[name];

                if (preset == "Vanilla")
                    return ret;

                List<StoredMusic> songs = null;

                if (preset != "Default" && preset != "Random" && preset != "Any")
                    songs = CustomMusicMod.Music.Where(m => Path.GetFileNameWithoutExtension(m.Path) == preset).ToList();

                if (preset == "Any")
                    songs = CustomMusicMod.Music.ToList();

                if (songs == null || songs.Count() == 0)
                    songs = CustomMusicMod.Music.Where(m => m.Id == name && CustomMusicMod.checkConditions(m.Conditions)).ToList();

                if (preset == "Random" || preset == "Any")
                    songs.Add(new StoredMusic() { Id = "Vanilla" });

                if (songs.Count > 0 && songs.First() is StoredMusic music)
                {
                    if (songs.Count > 1)
                        music = songs[Game1.random.Next(songs.Count())];

                    if (music.Id == "Vanilla")
                        return ret;

                    music.Sound.Play(CustomMusicMod.config.SoundVolume, 0f,0f);

                    if (CustomMusicMod.config.Debug)
                        CustomMusicMod.SMonitor.Log("Playing: " + name + (custom ? " (custom)" : " (Changed)"), StardewModdingAPI.LogLevel.Trace);
                    ret = false;
                }
                else if (CustomMusicMod.config.Debug)
                    CustomMusicMod.SMonitor.Log("Playing: " + name, StardewModdingAPI.LogLevel.Trace);
            }
            catch
            {

            }

            return ret;
        }


        public static bool Play2(string name, ref Cue __instance)
        {
            foreach (ActiveMusic a in CustomMusicMod.Active.Where(m => m.Id == name))
                a.Dispose();

            CustomMusicMod.Active.RemoveWhere(m => m.Id == name);

            bool custom = false;
            if (nextCue.Key == name)
            {
                custom = true;
                name = nextCue.Value;
                nextCue = new KeyValuePair<string, string>("none", "none");
            }
            bool ret = true;
            try
            {
                string preset = "Default";

                if (CustomMusicMod.config.Presets.ContainsKey(name))
                    preset = CustomMusicMod.config.Presets[name];

                if (preset == "Vanilla")
                    return ret;

                List<StoredMusic> songs = null;

                if (preset != "Default" && preset != "Random" && preset != "Any")
                    songs = CustomMusicMod.Music.Where(m => Path.GetFileNameWithoutExtension(m.Path) == preset).ToList();

                if (preset == "Any")
                    songs = CustomMusicMod.Music.ToList();

                if (songs == null || songs.Count() == 0)
                    songs = CustomMusicMod.Music.Where(m => m.Id == name && CustomMusicMod.checkConditions(m.Conditions)).ToList();

                if (preset == "Random" || preset == "Any")
                    songs.Add(new StoredMusic() { Id = "Vanilla" });

                if (songs.Count > 0 && songs.First() is StoredMusic music)
                {
                    if (songs.Count > 1)
                        music = songs[Game1.random.Next(songs.Count())];

                    if (music.Id == "Vanilla")
                        return ret;

                    ActiveMusic active = new ActiveMusic(__instance.Name, music.Sound.CreateInstance(), ref __instance, music.Ambient, music.Loop);
                    CustomMusicMod.Active.Add(active);
                    if (CustomMusicMod.config.Debug)
                        CustomMusicMod.SMonitor.Log("Playing: " + name + (custom ? " (custom)" : " (Changed)"), StardewModdingAPI.LogLevel.Trace);
                    ret = false;
                }
                else if (CustomMusicMod.config.Debug)
                    CustomMusicMod.SMonitor.Log("Playing: " + name, StardewModdingAPI.LogLevel.Trace);
            }
            catch
            {

            }

            return ret;
        }

        public static void SetVariable(Cue __instance, string name, ref float value)
        {
            if (CustomMusicMod.Active.ToList().Find(a => a.Id == __instance.Name) is ActiveMusic am)
                am.SetVolume(value * CustomMusicMod.config.MusicVolume);
        }

       
    }
}
