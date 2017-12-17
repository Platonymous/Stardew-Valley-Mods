using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using System;

namespace Visualize
{
    public class VisualizeMod : Mod
    {
        public static IMonitor _monitor;
        public static Profile _activeProfile;
        public static Config _config;
        public static IModHelper _helper;
        public static List<Color> palette = new List<Color>();
        public static Dictionary<Texture2D, List<Color>> paletteCache = new Dictionary<Texture2D, List<Color>>();
        public static List<Profile> profiles = new List<Profile>();

        public override void Entry(IModHelper helper)
        {
            _monitor = Monitor;
            _config = Helper.ReadConfig<Config>();
            _helper = Helper;
            loadProfiles();
            setActiveProfile();
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            GameEvents.EighthUpdateTick += GameEvents_EighthUpdateTick;
            harmonyFix();
        }

        private void GameEvents_EighthUpdateTick(object sender, System.EventArgs e)
        {
            if (_activeProfile != null && _activeProfile.noAmbientLight)
                Game1.ambientLight = Color.White;

            if (_activeProfile.noLightsources && Game1.currentLocation is GameLocation location)
                location.lightGlows = new List<Vector2>();
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (e.KeyPressed == _config.next)
                switchProfile(1);

            if (e.KeyPressed == _config.previous)
                switchProfile(-1);
        }

        public void setActiveProfile()
        {
            if (_activeProfile == null)
                _activeProfile = profiles.Find(p => p.id == _config.activeProfile);

            if (_activeProfile == null && profiles.Count > 0)
                _activeProfile = profiles[0];

            palette = new List<Color>();

            if (_activeProfile.palette != "none")
            {
                Texture2D paletteImage = Helper.Content.Load<Texture2D>($"Profiles/{_activeProfile.palette}", ContentSource.ModFolder);

                if (paletteCache.ContainsKey(paletteImage))
                    palette = paletteCache[paletteImage];
                else
                {
                    palette = Effects.loadPalette(paletteImage);
                    paletteCache.Add(paletteImage, palette);
                }
                    
            }

        }

        public void harmonyFix()
        {
            var instance = HarmonyInstance.Create("Platonymous.Visualize");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void loadProfiles()
        {
            profiles = new List<Profile>();

            string[] files = parseDir(Path.Combine(Helper.DirectoryPath, "Profiles"), "*.json");

            foreach (string file in files)
            {
                Profile profile = Helper.ReadJsonFile<Profile>(file);

                if (profile.id == "auto")
                    profile.id = profile.author + "." + profile.name;

                profiles.Add(profile);
                string author = profile.author == "none" ? "" : " by " + profile.author;
                Monitor.Log(profile.name + " " + profile.version + author, LogLevel.Info); 
            }

            Monitor.Log(files.Length + " Profiles found.");
        }

        private string[] parseDir(string path, string extension)
        {
            return Directory.GetFiles(path, extension, SearchOption.TopDirectoryOnly);
        }

        private void switchProfile(int i)
        {
            int cIndex = profiles.FindIndex(p => p == _activeProfile);
            int nIndex = cIndex + i;

            if (nIndex < 0)
                nIndex = profiles.Count - 1;

            if (nIndex >= profiles.Count)
                nIndex = 0;

            _activeProfile = profiles[nIndex];

            Effects.colorCache = new Dictionary<Color, Color>();
            Effects.textureCache = new Dictionary<Texture2D, Texture2D>();

            setActiveProfile();

            Game1.playSound("coin");
            string author = _activeProfile.author == "none" ? "" : " by " + _activeProfile.author;
            Game1.hudMessages.Clear();
            Game1.addHUDMessage(new HUDMessage("Profile: " + _activeProfile.name + author, 1));
            _config.activeProfile = _activeProfile.id;
            Helper.WriteConfig<Config>(_config);
        }

    }
}
