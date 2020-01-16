using StardewModdingAPI;
using PyTK.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Collections.Generic;
using PyTK.CustomElementHandler;
using PyTK.Types;

namespace SDVGameBoy
{
    public class SDVGameBoyMod : Mod
    {

        private CustomObjectData gbData;
        private List<CustomObjectData> cData;
        private Texture2D cartTexture;
        private string romfolder;
        internal static IMonitor _monitor;

        public override void Entry(IModHelper helper)
        {
            _monitor = Monitor;
            cData = new List<CustomObjectData>();
            cartTexture = Helper.Content.Load<Texture2D>(@"assets/cartridge.png");
            romfolder = Path.Combine(helper.DirectoryPath, "roms");
            GBCartridge.roms = new Dictionary<string, byte[]>();
            GameBoy.attTexture = helper.Content.Load<Texture2D>(@"assets/slot.png");
            gbData = new CustomObjectData("GameBoy", "GameBoy/0/-300/Crafting -9/A classic GameBoy,but it looks damaged./GameBoy", helper.Content.Load<Texture2D>(@"assets/gameboy.png"), Color.White, type: typeof(GameBoy));
            loadRoms();

            helper.Events.GameLoop.SaveLoaded += (s, e) =>
            {
                cData.ForEach(c => new InventoryItem(c.getObject(), 500, 1).addToNPCShop("Gus"));
                new InventoryItem(gbData.getObject(), 2000, 1).addToNPCShop("Gus");
            };
        }

        public void loadRoms()
        {
            string[] files = Directory.GetFiles(romfolder,"*.gb", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                string fileName = new FileInfo(file).Name;
                string cartFile = fileName.Replace(".gb", ".png");
                string name = fileName.Replace(".gb", "").Replace("_", " ");
                loadRom(name, file);
                Texture2D texture;
                if (File.Exists(Path.Combine(romfolder, cartFile)))
                    texture = Helper.Content.Load<Texture2D>(@"Roms/"+ cartFile);
                else
                    texture = cartTexture;
                cData.Add(new CustomObjectData(name, name + "/0/-300/Crafting -9/A Game for your GameBoy./" + name, texture, Color.White, type: typeof(GBCartridge)));
            }
        }

        public byte[] loadRom(string name, string path)
        {
            if (GBCartridge.roms.ContainsKey(name))
                return GBCartridge.roms[name];

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    byte[] rom = new byte[fs.Length];
                    for (int i = 0; i < fs.Length; i++)
                        rom[i] = br.ReadByte();
                    GBCartridge.roms.AddOrReplace(name, rom);
                    return rom;
                }
            }
        }

    }

    
}
