using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace ContentPack
{
    public class ContentPackMod : Mod, IDisposable
    {
        private string contentFolder;
        private string contentSource;
  

        private ContentConfig config;

        public void Dispose()
        {
            

            foreach (string dirPath in Directory.GetDirectories(contentSource, "*",
                SearchOption.AllDirectories))
            {
                string del = dirPath.Replace(contentSource, contentFolder);


                foreach (string newPath in Directory.GetFiles(del, "*.*",
                SearchOption.AllDirectories))
                {
                    Monitor.Log("Delete File:" + newPath);
                    File.Delete(newPath);
                }
                Monitor.Log("Delete Folder:" + del);
                Directory.Delete(del);
            }
                


        }

        public override void Entry(IModHelper helper)
        {

            config = Helper.ReadConfig<ContentConfig>();
            contentFolder = Path.Combine(Environment.CurrentDirectory, "Mods", config.targetMod, config.contentFolder);
            
            if (!Directory.Exists(contentFolder))
            {
                Directory.CreateDirectory(contentFolder);
            }

            contentSource = Path.Combine(Helper.DirectoryPath, "Machines");

            if (contentSource != null)
            {
                copyDirectories();
            }


        }

        private void copyDirectories()
        {
            Monitor.Log("Adding Content Packs");

            foreach (string dirPath in Directory.GetDirectories(contentSource, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(contentSource, contentFolder));

 
            foreach (string newPath in Directory.GetFiles(contentSource, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(contentSource, contentFolder), true);
        }

       
    }
}
