using Microsoft.Xna.Framework.Graphics;
using System.IO;
using StardewValley;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

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

            if (pTextures.ContainsKey(folders[activeFolder] + ">" + name + "_" + Game1.currentLocation.name))
                return pTextures[folders[activeFolder] + ">" + name + "_" + Game1.currentLocation.name];
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

                    if (!pTextures.ContainsKey(folderName + ">" + name))
                        pTextures.Add(folderName + ">" + name, PortraitureMod.helper.Content.Load<Texture2D>($"Portraits/{folderName}/{fileName}"));
                    else
                        pTextures[folderName + ">" + name] = PortraitureMod.helper.Content.Load<Texture2D>($"Portraits/{folderName}/{fileName}");
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
            string directoryName = folders[activeFolder];
            string savstring = directoryName;
            PortraitureMod.config.active = savstring;
            PortraitureMod.helper.WriteConfig(PortraitureMod.config);
        }


    }


}
