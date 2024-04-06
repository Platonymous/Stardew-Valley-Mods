using Microsoft.Xna.Framework.Graphics;
using System.IO;
using StardewValley;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.Xna.Framework;

using StardewModdingAPI;
using Newtonsoft.Json;
using System.Buffers;
using Portraiture.HDP;
using System.Diagnostics.Tracing;

namespace Portraiture
{
    class TextureLoader
    {
        private static string contentFolder;
        internal static int activeFolder = 0;
        internal static List<string> folders = new List<string>();
        internal static Dictionary<string, Texture2D> pTextures = new Dictionary<string, Texture2D>();
        internal static PresetCollection presets = new PresetCollection();

        public static void loadTextures()
        {
            activeFolder = 0;
            contentFolder = Path.Combine(PortraitureMod.helper.DirectoryPath, "Portraits");
            folders = new List<string>();
            folders.Add("Vanilla");
            pTextures = new Dictionary<string, Texture2D>();
            loadAllPortraits();

            string loadConfig = PortraitureMod.config.active;

            if (loadConfig == "none")
                    activeFolder = 0;
            else
                activeFolder = folders.Contains(loadConfig) ? folders.FindIndex(f => f == loadConfig) : (folders.Count > 1 ? 1 : 0);

                saveConfig();
            
        }

        internal static void setPreset(string name, string folder)
        {
            presets.Presets.RemoveAll(p => p.Character == name);
            if (!string.IsNullOrEmpty(folder))
                presets.Presets.Add(new Preset() { Character = name, Portraits = folder });

            PortraitureMod.config.presets = presets;
            saveConfig();
        }

        internal static void loadPreset(IMonitor monitor)
        {
            if (PortraitureMod.config.active is string d && !string.IsNullOrEmpty(d) && folders.Contains(d))
            {
                monitor.Log("Loaded Active Portraits: " + d, LogLevel.Info);
                activeFolder = folders.FindIndex(f => f == d);
            }

            if (PortraitureMod.config.presets is PresetCollection p)
            {
                monitor.Log("Loaded Active Presets for " + string.Join(',',p.Presets.Select(pr => pr.Character + ":" + pr.Portraits)),LogLevel.Info);

                presets = p;
            }
           
        }

        internal static Rectangle getSoureRectangle(Texture2D texture, int index = 0)
        {
            int textureSize = Math.Max(texture.Width / 2, 64);
            return Game1.getSourceRectForStandardTileSheet(texture, index, textureSize, textureSize);
        }

        public static Texture2D getPortrait(NPC npc, Texture2D tex)
        {
            var name = npc.Name;

            if (!Context.IsWorldReady || folders.Count == 0)
                return null;

            activeFolder = Math.Max(activeFolder, 0);

            if(presets.Presets.FirstOrDefault(pr => pr.Character == name) is Preset pre)
                activeFolder = Math.Max(folders.IndexOf(pre.Portraits), 0);

            var folder = folders[activeFolder];

            if (activeFolder == 0 || folders.Count <= activeFolder || folder == "none" || (true && folder == "HDP" && PortraitureMod.helper.ModRegistry.IsLoaded("tlitookilakin.HDPortraits")))
                return null;

            if (folder == "HDP" && !PortraitureMod.helper.ModRegistry.IsLoaded("tlitookilakin.HDPortraits"))
            {
                try
                {
                    var portraits = PortraitureMod.helper.GameContent.Load<MetadataModel>("Mods/HDPortraits/" + name);
                    if (portraits == null)
                        return null;


                    if (portraits is MetadataModel model && model.TryGetTexture(out Texture2D texture))
                    {

                        if (model.Animation != null && (model.Animation.VFrames != 1 || model.Animation.HFrames != 1))
                        {
                            model.Animation.Reset();
                            return new AnimatedTexture2D(texture, texture.Width / model.Animation.VFrames, texture.Height / model.Animation.HFrames, 6, true, model.Size / 64f);
                        }
                        else
                            return ScaledTexture2D.FromTexture(tex, texture, model.Size / 64f, null);
                    }
                    else return null;
                }
                catch
                {
                    return null;
                }
            }


                string season = Game1.currentSeason?.ToLower() ?? "spring";


                if (presets.Presets.FirstOrDefault(p => p.Character == name) is Preset preset && folders.Contains(preset.Portraits))
                    folder = preset.Portraits;


                if (Game1.currentLocation is GameLocation gl && gl.Name is string locname)
                {
                    if (pTextures.ContainsKey(folder + ">" + name + "_" + gl.Name + "_" + season))
                        return pTextures[folder + ">" + name + "_" + gl.Name + "_" + season];
                    else if (pTextures.ContainsKey(folders[activeFolder] + ">" + name + "_" + gl.Name))
                        return pTextures[folder + ">" + name + "_" + gl.Name];
                }

                if (pTextures.ContainsKey(folder + ">" + name + "_" + season))
                    return pTextures[folder + ">" + name + "_" + season];

                if (pTextures.ContainsKey(folder + ">" + name))
                    return pTextures[folder + ">" + name];
           
            return null;
        }

        private static void loadAllPortraits()
        {
            foreach (string dir in Directory.EnumerateDirectories(contentFolder))
            {
                string folderName = new DirectoryInfo(dir).Name;

                folders.Add(folderName);
                foreach (string file in Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".png") || s.EndsWith(".xnb")))
                {
                    string fileName = Path.GetFileName(file);
                    string name = Path.GetFileNameWithoutExtension(file).Split(new[] { "_anim_" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    string extention = Path.GetExtension(file).ToLower();

                    if (extention == "xnb")
                        fileName = name;

                    Texture2D texture = PortraitureMod.helper.ModContent.Load<Texture2D>($"Portraits/{folderName}/{fileName}");

                    Texture2D frame = texture;
                    int fps = 12;
                    int frames = 1;
                    bool loop = false;
                    if (fileName.Contains("_anim_"))
                    {
                        string[] fdata = fileName.Split(new[] { "_anim_" }, StringSplitOptions.RemoveEmptyEntries);
                        if (fdata.Length > 1)
                            frames = int.Parse(fdata[1]);

                        if (fdata.Length > 2)
                            fps = int.Parse(fdata[2]);

                        if (fdata.Length > 3)
                            loop = fdata[3] == "loop";

                        if (frames < 1)
                            frames = 1;

                        if (fps < 1)
                            fps = 12;
                    }

                    double tileWith = Convert.ToDouble(Math.Max(texture.Width / 2, 64)) / frames;
                    float scale = (float)(tileWith / 64);
                    Texture2D scaled = texture;
                    if (frames == 1)
                        scaled = new ScaledTexture2D(texture, scale);
                    else if (frames > 1)
                        scaled = new AnimatedTexture2D(texture, texture.Width / frames, texture.Height, fps, loop, scale);

                    if (!pTextures.ContainsKey(folderName + ">" + name))
                        pTextures.Add(folderName + ">" + name, scaled);
                    else
                        pTextures[folderName + ">" + name] = scaled;
                }
            }

            var contentPacks = PortraitureMod.helper.ContentPacks.GetOwned();

            foreach (StardewModdingAPI.IContentPack pack in contentPacks)
            {
                string folderName = pack.Manifest.UniqueID;

                folders.Add(folderName);
                foreach (string file in Directory.EnumerateFiles(pack.DirectoryPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".png") || s.EndsWith(".xnb")))
                {
                    string fileName = Path.GetFileName(file);
                    string name = Path.GetFileNameWithoutExtension(file);
                    string extention = Path.GetExtension(file).ToLower();

                    if (extention == "xnb")
                        fileName = name;
                    Texture2D texture = pack.ModContent.Load<Texture2D>(fileName);
                    int tileWith = Math.Max(texture.Width / 2, 64);
                    float scale = tileWith / 64;


                    ScaledTexture2D scaled;
                    
                        scaled = ScaledTexture2D.FromTexture(Game1.getCharacterFromName(name).Portrait, texture, scale);
                   
                    if (!pTextures.ContainsKey(folderName + ">" + name))
                        pTextures.Add(folderName + ">" + name, scaled);
                    else
                        pTextures[folderName + ">" + name] = scaled;
                }
            }
            
            if(PortraitureMod.config.HPDOption)
                folders.Add("HDP");

                if ((PortraitureMod.config.SideLoadHDPWhenInstalled && PortraitureMod.helper.ModRegistry.IsLoaded("tlitookilakin.HDPortraits")) ||  (PortraitureMod.config.SideLoadHDPWhenNotInstalled && !PortraitureMod.helper.ModRegistry.IsLoaded("tlitookilakin.HDPortraits")))
                foreach (var file in Directory.GetParent(PortraitureMod.helper.DirectoryPath).EnumerateFiles("manifest.json", SearchOption.AllDirectories))
                {
                    try
                    {
                        if (JsonConvert.DeserializeObject<SmapiManifest>(File.ReadAllText(file.FullName)) is SmapiManifest manifest)
                        {
                            if (manifest.ContentPackFor?.UniqueID == "Pathoschild.ContentPatcher" && manifest.Dependencies.Any(d => d.UniqueID == "tlitookilakin.HDPortraits"))
                            {
                                try
                                {
                                    string folderName = manifest.UniqueID;
                                    folders.Add(folderName);
                                    Dictionary<float, int> scales = new Dictionary<float, int>();
                                    List<ScaledTexture2D> rescale = new List<ScaledTexture2D>();

                                    foreach (string f in Directory.EnumerateFiles(file.Directory.FullName, "*.png", SearchOption.AllDirectories))
                                    {
                                        string fileName = f;
                                        string name = Path.GetFileNameWithoutExtension(f);

                                        Texture2D texture = Texture2D.FromFile(Game1.graphics.GraphicsDevice, fileName);
                                        PremultiplyTransparency(texture);
                                        int tileWith = Math.Max(texture.Width / 2, 64);
                                        float scale = tileWith / 64f;
                                        ScaledTexture2D scaled;
                                        try
                                        {
                                                if(Game1.getCharacterFromName(name) is NPC ch && ch.Portrait is Texture2D ptex)
                                            scaled = ScaledTexture2D.FromTexture(ptex, texture, scale);
                                                else
                                                    scaled = ScaledTexture2D.FromTexture(Game1.getCharacterFromName("Pierre").Portrait, texture, scale);
                                            }
                                            catch
                                            {
                                                scaled = ScaledTexture2D.FromTexture(Game1.getCharacterFromName("Pierre").Portrait, texture, scale);

                                            }

                                            if (!pTextures.ContainsKey(folderName + ">" + name))
                                            pTextures.Add(folderName + ">" + name, scaled);
                                        else
                                            pTextures[folderName + ">" + name] = scaled;

                                        if (scales.ContainsKey(scale))
                                            scales[scale]++;
                                        else
                                            scales.Add(scale, 1);

                                        var maxScale = scales.ToList().OrderByDescending(s => s.Value).FirstOrDefault().Key;

                                        if (scale != maxScale && pTextures[folderName + ">" + name] is ScaledTexture2D st)
                                            rescale.Add(st);
                                    }

                                    rescale.ForEach(s => s.Scale = scales.ToList().OrderByDescending(s => s.Value).FirstOrDefault().Key);

                                    PortraitureMod.log("Added HD Portraits Pack: " + manifest.UniqueID);
                                }
                                catch
                                {

                                }
                            }

                        }
                    }
                    catch
                    {

                    }

                }
        }

        private static void PremultiplyTransparency(Texture2D texture)
        {
            int count = texture.Width * texture.Height;
            Color[] data = ArrayPool<Color>.Shared.Rent(count);
            try
            {
                texture.GetData(data, 0, count);

                bool changed = false;
                for (int i = 0; i < count; i++)
                {
                    ref Color pixel = ref data[i];
                    if (pixel.A is (byte.MinValue or byte.MaxValue))
                        continue;

                    data[i] = new Color(pixel.R * pixel.A / byte.MaxValue, pixel.G * pixel.A / byte.MaxValue, pixel.B * pixel.A / byte.MaxValue, pixel.A);
                    changed = true;
                }

                if (changed)
                    texture.SetData(data, 0, count);
            }
            finally
            {
                ArrayPool<Color>.Shared.Return(data);
            }
        }

        public static string getFolderName()
        {
            return folders[activeFolder];
        }

        public static void nextFolder()
        {
            activeFolder++;
            if (folders.Count <= activeFolder)
                activeFolder = 0;

                saveConfig();
        }

        private static void saveConfig()
        {
            if (folders.Count > activeFolder && activeFolder >= 0)
            {
                PortraitureMod.config.active = folders[activeFolder];
            }
            else
            {
                PortraitureMod.config.active = "none";
            }
            PortraitureMod.helper.WriteConfig(PortraitureMod.config);
        }


    }


    public class SmapiManifest
    {
        public string Name { get; set; } = "";
        public string UniqueID { get; set; } = "";

        public SmapiManifest ContentPackFor { get; set; } = null;

        public List<SmapiManifest> Dependencies { get; set; } = new List<SmapiManifest>();
    }

}