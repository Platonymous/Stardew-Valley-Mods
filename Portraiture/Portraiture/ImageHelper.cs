using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Drawing;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System;

namespace Portraiture
{
    class ImageHelper
    {
        public static IModHelper helper;
        public static IMonitor monitor;
        public static List<string> folders;
        public static int activeFolder;
        public static SerializableDictionary<string, Texture2D> pTextures;
        public static float displayAlpha;


        public static void loadTextureFolders()
        {
            folders = new List<string>();
            pTextures = new SerializableDictionary<string, Texture2D>();
            string path = Path.Combine(helper.DirectoryPath, "Portraits");
            folders.Add(path);
            foreach (string dir in Directory.EnumerateDirectories(path))
            {
                folders.Add(Path.Combine(path, dir));
            }

            activeFolder = 0;

            if (folders.Count > 1)
            {
                activeFolder = 1;
            }

            string loadConfig = loadConfigFile().Split('?')[0];

     

            if(loadConfig != "")
            {
                
                for(int i=0; i < folders.Count; i++)
                {
                    string fName = new DirectoryInfo(folders[i]).Name;
                    
                    if (fName == loadConfig)
                    {
                        activeFolder = i;
                    }
                }

                
                if (activeFolder == -1)
                {
                    activeFolder = 1;
                }
            }
           

            anounceFolderName();

        }

        public static void nextFolder()
        {
            
            activeFolder++;
            if (folders.Count <= activeFolder)
            {
                activeFolder = 0;
            }
            pTextures = new SerializableDictionary<string, Texture2D>();
            PortraitureDialogueBoxNew.animationFinished = false;
            displayAlpha = 3;
            anounceFolderName();
        }

        public static void anounceFolderName()
        {
            
            string foldername = "Vanilla";

            if (activeFolder > 0)
            {
                DirectoryInfo folderInfo = new DirectoryInfo(folders[activeFolder]);
                foldername = folderInfo.Name;

            }

            string message = "Portraits set to: " + foldername;
            monitor.Log(message);
            saveConfig();
            
        }

        public static string getFolderName()
        {
            string foldername = "Vanilla";

            if (activeFolder > 0)
            {
                DirectoryInfo folderInfo = new DirectoryInfo(folders[activeFolder]);
                foldername = folderInfo.Name;

            }

            return foldername;

        }

        public static string loadConfigFile()
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

        public static void saveConfig()
        {
            string filename = "Portraiture" + "_" + Game1.player.name + "_" + Game1.uniqueIDForThisGame + ".sav";
            FileInfo fi = ensureFolderStructureExists(Game1.player.name, Game1.uniqueIDForThisGame, filename);

            string directoryName = new DirectoryInfo(folders[activeFolder]).Name;
            string savstring = directoryName+"?"; 

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

        public static bool doesConfigExist()
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

        public static bool doesImageFileExist(string name)
        {
            string folder = folders[activeFolder];
            string path = Path.Combine(folder, name);
        
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
      
                return false;
            }
            else
            {
              
                return true;
            }



        }

        public static Texture2D loadTextureFromModFolder(string filename)
        {
            string folder = folders[activeFolder];
            string tilesheetFile = Path.Combine(folder, filename);
            Bitmap tilesheetImage = new Bitmap(Image.FromFile(tilesheetFile));
            
            return Bitmap2Texture(tilesheetImage);

        }

        public static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
   
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public static Texture2D loadXNBFromModFolder(string filename)
        {
            string folder = folders[activeFolder];
            string tilesheetFile = Path.Combine(folder, filename);
            string filepath = tilesheetFile;
            string folderpath = Path.Combine(StardewModdingAPI.Constants.ExecutionPath,"Content");
            string relPath = GetRelativePath(filepath, folderpath);
          
            return Game1.content.Load<Texture2D>(relPath);

        }

        public static Texture2D Bitmap2Texture(Bitmap bmp)
        {

            MemoryStream s = new MemoryStream();

            bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
            s.Seek(0, SeekOrigin.Begin);
            Texture2D tx = Texture2D.FromStream(StardewValley.Game1.graphics.GraphicsDevice, s);

            return tx;

        }
    }
}
