using Microsoft.Xna.Framework.Graphics;
using System.IO;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Portraiture
{
    class TextureLoader
    {
        private static string contentFolder;
        private static int activeFolder;
        private static List<string> folders;
        static Dictionary<string, Texture2D> pTextures;
        
        public static void loadTextures()
        {
            activeFolder = 0;
            contentFolder = Path.Combine(PortraitureMod.helper.DirectoryPath, "Portraits");
            folders = new List<string>();
            folders.Add("Vanilla");
            pTextures = new Dictionary<string, Texture2D>();
            pTextures.Add("empty", PortraitureMod.helper.Content.Load<Texture2D>("empty.png", ContentSource.ModFolder));
            loadAllPortraits();

            string loadConfig = loadConfigFile().Split('?')[0];
            if (loadConfig == "")
            {
                if (folders.Count > 1)
                {
                    loadConfig = folders[1];
                }
                else
                {
                    loadConfig = folders[0];
                }
            }
            activeFolder = folders.FindIndex(f => f == loadConfig);

            if(activeFolder < 0)
            {
                activeFolder = folders.Count - 1;
                saveConfig();
            }

        }


        public static Texture2D getEmptyPortrait()
        {
            return pTextures["empty"];
        }

        public static Texture2D getPortrait(string name)
        {
            activeFolder = Math.Max(activeFolder, 0);

            if (pTextures.ContainsKey(folders[activeFolder] + ">" + name + "_" + Game1.currentLocation.name))
            {
                return pTextures[folders[activeFolder] + ">" + name + "_" + Game1.currentLocation.name];
            }
            else if (pTextures.ContainsKey(folders[activeFolder] + ">" +name))
            {
                return pTextures[folders[activeFolder] + ">" + name];
            }
            else
            {
                return Game1.getCharacterFromName(name).Portrait;
            }
            
        }

        private static void loadAllPortraits()
        {
            foreach (string dir in Directory.EnumerateDirectories(contentFolder))
            {
                string folderName = new DirectoryInfo(dir).Name;
            
                folders.Add(folderName);
                foreach (string file in Directory.EnumerateFiles(dir,"*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".png") || s.EndsWith(".xnb"))){
                    
                    string fileName = Path.GetFileName(file);
                    string name = Path.GetFileNameWithoutExtension(file);
                    string extention = Path.GetExtension(file).ToLower();
                    if (extention == "xnb")
                    {
                        fileName = name;
                    }
                    pTextures.Add(folderName + ">" + name, PortraitureMod.helper.Content.Load<Texture2D>(Path.Combine("Portraits",folderName,fileName)));

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
            {
                activeFolder = 0;
            }

            saveConfig();
        }


        private static string loadConfigFile()
        {
            if (!(doesConfigExist()))
            {
                return "";
            }
            string filename = "Portraiture" + "_" + Game1.player.name + "_" + Game1.uniqueIDForThisGame + ".sav";

            FileInfo fi = ensureFolderStructureExists(Game1.player.name, Game1.uniqueIDForThisGame, filename);

            using (StreamReader sr = fi.OpenText())
            {
                return sr.ReadToEnd();

            }
        }

        private static void saveConfig()
        {
            string filename = "Portraiture" + "_" + Game1.player.name + "_" + Game1.uniqueIDForThisGame + ".sav";
            FileInfo fi = ensureFolderStructureExists(Game1.player.name, Game1.uniqueIDForThisGame, filename);

            string directoryName = folders[activeFolder];
            string savstring = directoryName + "?";

            using (StreamWriter sw = fi.CreateText())
            {
                sw.WriteLine(savstring);
            }
        }


        private static FileInfo ensureFolderStructureExists(string PN, ulong GID, string tmpString)
        {
            string str = PN;
            foreach (char c in str)
            {
                if (!char.IsLetterOrDigit(c))
                    str = str.Replace(c.ToString() ?? "", "");
            }
            string path2 = Path.Combine(str + "_" + (object)GID, tmpString);
            FileInfo fileInfo1 = new FileInfo(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"), path2));
            if (!fileInfo1.Directory.Exists)
                fileInfo1.Directory.Create();

            return fileInfo1;
        }

        private static bool doesConfigExist()
        {
            string filename = "Portraiture" + "_" + Game1.player.name + "_" + Game1.uniqueIDForThisGame + ".sav";
            string str = Game1.player.name;
            foreach (char c in str)
            {
                if (!char.IsLetterOrDigit(c))
                    str = str.Replace(c.ToString() ?? "", "");
            }
            string path2 = Path.Combine(str + "_" + (object)Game1.uniqueIDForThisGame, filename);
            FileInfo fileInfo1 = new FileInfo(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"), path2));
            if (!fileInfo1.Exists)
            {
                return false;
            }
            else
            {
                return true;
            }

        }


    }


}
