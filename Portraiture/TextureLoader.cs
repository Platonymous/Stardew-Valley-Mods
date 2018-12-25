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
        static Dictionary<string, ScaledTexture2D> pTextures;

        public static void loadTextures()
        {
            activeFolder = 0;
            contentFolder = Path.Combine(PortraitureMod.helper.DirectoryPath, "Portraits");
            folders = new List<string>();
            folders.Add("Vanilla");
            pTextures = new Dictionary<string, ScaledTexture2D>();
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

        public static ScaledTexture2D getPortrait(string name)
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
                    string name = Path.GetFileNameWithoutExtension(file);
                    string extention = Path.GetExtension(file).ToLower();

                    if (extention == "xnb")
                        fileName = name;
                    Texture2D texture = PortraitureMod.helper.Content.Load<Texture2D>($"Portraits/{folderName}/{fileName}");
                    double tileWith = Convert.ToDouble(Math.Max(texture.Width / 2, 64));
                    float scale = (float)(tileWith / 64);

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
