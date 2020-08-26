using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace CustomMusic
{
    public interface IGMCMAPI
    {
        void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);

        void RegisterLabel(IManifest mod, string labelName, string labelDesc);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet);

        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max, int interval);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max, float interval);

        void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices);

        void RegisterComplexOption(IManifest mod, string optionName, string optionDesc,
                                   Func<Vector2, object, object> widgetUpdate,
                                   Func<SpriteBatch, Vector2, object, object> widgetDraw,
                                   Action<object> onSave);

        void SubscribeToChange(IManifest mod, Action<string, bool> changeHandler);
        void SubscribeToChange(IManifest mod, Action<string, int> changeHandler);
        void SubscribeToChange(IManifest mod, Action<string, float> changeHandler);
        void SubscribeToChange(IManifest mod, Action<string, string> changeHandler);

        void OpenModMenu(IManifest mod);
    }

    public class GMCMConfig
    {
        public List<GMCMOption> Options = new List<GMCMOption>();
        public static IGMCMAPI Api = null;
        public IManifest Manifest = null;
        public Action<string, string> SaveHandler;
        public static bool Patched = false;
        public GMCMOption Label = null;

        internal static Dictionary<object, int> currentIndex = new Dictionary<object, int>();
        internal const int maxValues = 5;
        public static SoundEffectInstance activeSound = null;

        public GMCMConfig(IManifest manifest, Action<string, string> saveHandler, List<GMCMOption> options, GMCMLabel label = null)
        {
            Manifest = manifest;
            Options = options;
            SaveHandler = saveHandler;
            Label = label;
        }
       
            public static IGMCMAPI GetAPI(IModHelper helper)
        {
            if (Api is IGMCMAPI)
                return Api;

            if (!helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
                return null;

            Api = helper.ModRegistry.GetApi<IGMCMAPI>("spacechase0.GenericModConfigMenu");
            return Api;
        }

        public bool register(IModHelper helper)
        {
            if (GetAPI(helper) == null)
                return false;

            Api.RegisterModConfig(Manifest, () =>
            {
                foreach(var option in Options)
                    option.ActiveIndex = option.DefaultIndex;

                activeSound?.Stop(true);
            }, () => SaveHandler.Invoke("save","file"));

            Api.RegisterClampedOption(Manifest, "MusicVolume", "", () => CustomMusicMod.config.MusicVolume, (f) => CustomMusicMod.config.MusicVolume = f, 0f, 1f);
            Api.RegisterClampedOption(Manifest, "SoundVolume", "", () => CustomMusicMod.config.SoundVolume, (f) => CustomMusicMod.config.SoundVolume = f, 0f, 1f);

            if (Label != null)
                Api.RegisterLabel(Manifest, Label.Name, Label.Description);

            foreach (var option in Options)
            Api.RegisterChoiceOption(Manifest, option.Name, option.Description, () =>
            {
                activeSound?.Stop(true);
                return option.Choices[option.ActiveIndex];
            }, (s) =>
            {
                activeSound?.Stop(true);
                option.ActiveIndex = option.Choices.IndexOf(s);
                SaveHandler(option.Name, s);
            }, option.Choices.ToArray());

            Api.RegisterLabel(Manifest, "", "");
            Api.RegisterLabel(Manifest, "", "");
            Api.RegisterLabel(Manifest, "", "");
            Api.RegisterLabel(Manifest, "", "");
            Api.RegisterLabel(Manifest, "", "");
            Api.RegisterLabel(Manifest, "", "");

            Api.SubscribeToChange(Manifest, HandleChange);
            
            return true;
        }

        public void HandleChange(string key, string value)
        {
            Game1.stopMusicTrack(Game1.MusicContext.Default);
            if(CustomMusicMod.Music.FirstOrDefault(m => Path.GetFileNameWithoutExtension(m.Path) == value) is StoredMusic sm)
            {
                activeSound?.Stop();
                Game1.stopMusicTrack(Game1.MusicContext.Default);
                activeSound = sm.Sound.CreateInstance();
                activeSound.Play();
            }
        }
    }
}
