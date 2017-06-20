using StardewModdingAPI;
using StardewModdingAPI.Events;

using Microsoft.Xna.Framework;

using System.IO;
using System.Collections.Generic;

using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace CustomFurniture
{
    public class CustomFurnitureMod : Mod
    {
        internal static IModHelper helper;
        internal static Dictionary<string,CustomFurniture> furniture;

        public override void Entry(IModHelper helper)
        {
            CustomFurnitureMod.helper = helper;
            furniture = new Dictionary<string, CustomFurniture>();
            loadPacks();
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;
        }

        private void SaveEvents_AfterReturnToTitle(object sender, System.EventArgs e)
        {
            MenuEvents.MenuChanged -= MenuEvents_MenuChanged;
        }

        private void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {  
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
        }

        private void loadPacks()
        {
            int countPacks = 0;
            int countObjects = 0;

            string[] files = parseDir(Path.Combine(Helper.DirectoryPath, "Furniture"), "*.json");

            countPacks = files.Length;

            foreach (string file in files)
            {
                CustomFurniturePack pack = Helper.ReadJsonFile<CustomFurniturePack>(file);

                foreach (CustomFurnitureData data in pack.furniture)
                {
                    countObjects++;
                    data.folderName = Path.GetDirectoryName(file);
                    string objectID = data.folderName + "." + Path.GetFileName(file) + "." + data.id;
                    
                    CustomFurniture f = new CustomFurniture(data, objectID, Vector2.Zero);

                    furniture.Add(objectID, f);
                }
            }

            Monitor.Log(countPacks + " Content Packs with " + countObjects + " Objects found.");
        }

        private string[] parseDir(string path, string extension)
        {
            return Directory.GetFiles(path, extension, SearchOption.AllDirectories);
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (Game1.activeClickableMenu is ShopMenu)
            {
                ShopMenu shop = (ShopMenu)Game1.activeClickableMenu;
                Dictionary<Item, int[]> items = Helper.Reflection.GetPrivateValue<Dictionary<Item, int[]>>(shop, "itemPriceAndStock");
                List<Item> selling = Helper.Reflection.GetPrivateValue<List<Item>>(shop, "forSale");
                int currency = Helper.Reflection.GetPrivateValue<int>(shop, "currency");
                bool isCatalogue = (currency == 0 && selling[0] is Furniture);

                if ((shop.portraitPerson != null && shop.portraitPerson.name == "Robin") || isCatalogue)
                {
                    Dictionary<Item, int> newItemsToSell = new Dictionary<Item, int>();

                    foreach (CustomFurniture f in furniture.Values)
                    {
                        newItemsToSell.Add(f, isCatalogue ? 0 : f.price);
                    }

                    foreach (Item item in newItemsToSell.Keys)
                    {
                        items.Add(item, new int[] { newItemsToSell[item], int.MaxValue });
                        selling.Add(item);
                    }
                }

            }
        }
    }
}
