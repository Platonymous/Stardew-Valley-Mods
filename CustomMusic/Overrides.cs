using Microsoft.Xna.Framework.Audio;
using System.Linq;

namespace CustomMusic
{
    public class Overrides
    {
        public static bool skip = false;

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

        public static void Dispose(Cue __instance)
        {
            foreach (ActiveMusic a in CustomMusicMod.Active.Where(m => m.Id == __instance.Name))
                a.Dispose();

            CustomMusicMod.Active.RemoveWhere(m => m.Id == __instance.Name);
        }

        public static bool Play(ref Cue __instance)
        {
            CustomMusicMod.SMonitor.Log("Playing: " + __instance.Name + (CustomMusicMod.Music.ContainsKey(__instance.Name) ? " (Changed)" : ""), StardewModdingAPI.LogLevel.Trace);

            if (CustomMusicMod.Music.ContainsKey(__instance.Name) && CustomMusicMod.Music[__instance.Name] is StoredMusic music)
                CustomMusicMod.Active.Add(new ActiveMusic(__instance.Name, music.Sound.CreateInstance(), ref __instance, music.Ambient, music.Loop));

            return !CustomMusicMod.Music.ContainsKey(__instance.Name);
        }

        public static void SetVariable(Cue __instance, string name, ref float value)
        {
            if (CustomMusicMod.Active.ToList().Find(a => a.Id == __instance.Name) is ActiveMusic am)
                am.SetVolume(value);
        }
    }
}
