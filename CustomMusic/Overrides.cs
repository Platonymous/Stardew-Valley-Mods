using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
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
            if (name.StartsWith("cm_"))
            {
                string[] n = name.Split('_');
                string next = n.Length > 2 ? n[2] : "MainTheme";
                __result = __instance.GetCue(next);
                nextCue = new KeyValuePair<string, string>(next, name);
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

        public static bool Play(ref Cue __instance)
        {
            string name = __instance.Name;
            bool custom = false;
            if(nextCue.Key == name)
            {
                custom = true;
                name = nextCue.Value;
                nextCue = new KeyValuePair<string, string>("none", "none");
            }
            CustomMusicMod.SMonitor.Log("Playing: " + name + (CustomMusicMod.Music.ContainsKey(name) ? (custom ? " (custom)" : " (Changed)") : ""), StardewModdingAPI.LogLevel.Trace);

            if (CustomMusicMod.Music.ContainsKey(name) && CustomMusicMod.Music[name] is StoredMusic music)
                CustomMusicMod.Active.Add(new ActiveMusic(__instance.Name, music.Sound.CreateInstance(), ref __instance, music.Ambient, music.Loop));

            return !CustomMusicMod.Music.ContainsKey(name);
        }

        public static void SetVariable(Cue __instance, string name, ref float value)
        {
            if (CustomMusicMod.Active.ToList().Find(a => a.Id == __instance.Name) is ActiveMusic am)
                am.SetVolume(value);
        }
    }
}
