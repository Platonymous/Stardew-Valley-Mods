using Microsoft.Xna.Framework.Graphics;
using System.IO;
using StardewValley;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using PyTK.Types;

namespace Portraiture
{
    class TextureLoader
    {
        private static string contentFolder;
        internal static int activeFolder;
        private static List<string> folders;
        static Dictionary<string, Texture2D> pTextures;

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
                if (folders.Count > 1)
                    loadConfig = folders[1];
                else
                    loadConfig = folders[0];
            else
                activeFolder = folders.FindIndex(f => f == loadConfig);

            saveConfig();
        }

        internal static Rectangle getSoureRectangle(Texture2D texture, int index = 0)
        {
            int textureSize = Math.Max(texture.Width / 2, 64);
            return Game1.getSourceRectForStandardTileSheet(texture, index, textureSize, textureSize);
        }

        public static Texture2D getPortrait(string name)
        {
            activeFolder = Math.Max(activeFolder, 0);

            if (pTextures.ContainsKey(folders[activeFolder] + ">" + name + "_" + Game1.currentLocation.Name))
                return pTextures[folders[activeFolder] + ">" + name + "_" + Game1.currentLocation.Name];
            else if (pTextures.ContainsKey(folders[activeFolder] + ">" + name))
                return pTextures[folders[activeFolder] + ">" + name];
            
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

                    Texture2D texture = PortraitureMod.helper.Content.Load<Texture2D>($"Portraits/{folderName}/{fileName}");

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
                    if (frames == 1 && scale != 1)
                        scaled = new ScaledTexture2D(texture, scale);
                    else if (frames > 1)
                        scaled = new AnimatedTexture2D(texture, texture.Width / frames, texture.Height,fps,loop,scale);

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

                foreach (string file in Directory.EnumerateFiles(pack.DirectoryPath, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".png") || s.EndsWith(".xnb")))
                {
                    string fileName = Path.GetFileName(file);
                    string name = Path.GetFileNameWithoutExtension(file);
                    string extention = Path.GetExtension(file).ToLower();

                    if (extention == "xnb")
                        fileName = name;
                    Texture2D texture = pack.LoadAsset<Texture2D>($"{fileName}");
                    int tileWith = Math.Max(texture.Width / 2, 64);
                    float scale = tileWith / 64;
                    ScaledTexture2D scaled;
                    try
                    {
                        scaled = ScaledTexture2D.FromTexture(Game1.getCharacterFromName(name).Portrait, texture, scale);
                    }
                    catch
                    {
                        scaled = ScaledTexture2D.FromTexture(Game1.getCharacterFromName("Pierre").Portrait, texture, scale);
                    }
                    if (!pTextures.ContainsKey(folderName + ">" + name))
                        pTextures.Add(folderName + ">" + name, scaled);
                    else
                        pTextures[folderName + ">" + name] = scaled;
                }
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
                string directoryName = folders[activeFolder];
                string savstring = directoryName;
                PortraitureMod.config.active = savstring;
                PortraitureMod.helper.WriteConfig(PortraitureMod.config);
            }
        }


    }


}
