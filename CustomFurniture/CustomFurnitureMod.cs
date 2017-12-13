using StardewModdingAPI;
using StardewModdingAPI.Events;

using Microsoft.Xna.Framework;

using System.IO;
using System.Collections.Generic;

using StardewValley;
using StardewValley.Menus;
using System.Linq;
using StardewValley.Objects;
using System;
using Microsoft.Xna.Framework.Graphics;
using Harmony;
using System.Reflection;

namespace CustomFurniture
{
    public class CustomFurnitureMod : Mod
    {
        internal static IModHelper helper;
        internal static Dictionary<string,CustomFurniture> furniture = new Dictionary<string, CustomFurniture>();
        private static Dictionary<string, CustomFurniture> furniturePile = new Dictionary<string, CustomFurniture>();
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            CustomFurnitureMod.helper = helper;
            harmonyFix();
            loadPacks();
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;
            helper.ConsoleCommands.Add("replace_custom_furniture", "Triggers Custom Furniture Replacement", replaceCustomFurniture);
        }

        public void harmonyFix()
        {
            var instance = HarmonyInstance.Create("Platonymous.CustomFurniture");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void replaceCustomFurniture(string action, string[] param)
        {
            if (param[0] == "itemMenu" && Game1.activeClickableMenu is ItemGrabMenu itemMenu)
            {
                List<Item> remove = new List<Item>();
                List<Item> additions = new List<Item>();

                foreach (Item item in itemMenu.ItemsToGrabMenu.actualInventory)
                    if (item is Chest chest && furniturePile.Keys.Where(f => f.Equals(item.Name)).Any())
                    {
                        Item cf = furniturePile[furniturePile.Keys.Where(f => f.Equals(item.Name)).FirstOrDefault()];
                        additions.Add(cf);
                        remove.Add(item);
                    }

                foreach (Item addition in additions)
                    itemMenu.ItemsToGrabMenu.actualInventory.Add(addition);

                foreach (Item j in remove)
                    itemMenu.ItemsToGrabMenu.actualInventory.Remove(j);
            }

            if (param[0] == "shop" && Game1.activeClickableMenu is ShopMenu shop)
            {
                Dictionary<Item, int[]> items = Helper.Reflection.GetPrivateValue<Dictionary<Item, int[]>>(shop, "itemPriceAndStock");
                List<Item> selling = Helper.Reflection.GetPrivateValue<List<Item>>(shop, "forSale");
                List<Item> remove = new List<Item>();
                List<Item> additions = new List<Item>();

                foreach (Item i in selling)
                    if (i is Chest chest && furniturePile.Keys.Where(f => f.Equals(i.Name)).Any())
                    {
                        Item cf = furniturePile[furniturePile.Keys.Where(f => f.Equals(i.Name)).FirstOrDefault()];
                        items.Add(cf, new int[] { chest.preservedParentSheetIndex, int.MaxValue });
                        additions.Add(cf);
                        remove.Add(i);
                    }

                foreach (Item addition in additions)
                    selling.Add(addition);

                foreach (Item j in remove)
                {
                    items.Remove(j);
                    selling.Remove(j);
                }
            }
        }

        public static void log(string text)
        {
            instance.Monitor.Log(text);
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
                string author = pack.author == "none" ? "" : " by " + pack.author;
                Monitor.Log(pack.name + " " + pack.version + author, LogLevel.Info);
                foreach (CustomFurnitureData data in pack.furniture)
                {
                    countObjects++;
                    data.folderName = Path.GetDirectoryName(file);
                    string objectID = data.folderName + "." + Path.GetFileName(file) + "." + data.id;
                    string pileID = new DirectoryInfo(data.folderName).Name + "." + new FileInfo(file).Name+ "." + data.id;
                    CustomFurniture f = new CustomFurniture(data, objectID, Vector2.Zero);
                    furniturePile.Add(pileID, f);
                    furniture.Add(objectID, f);
                }
            }

            Monitor.Log(countPacks + " Content Packs with " + countObjects + " Objects found.");
        }

        private string[] parseDir(string path, string extension)
        {
            return Directory.GetFiles(path, extension, SearchOption.AllDirectories);
        }

        private bool meetsConditions(string conditions)
        {
            return (Helper.Reflection.GetPrivateMethod(Game1.currentLocation, "checkEventPrecondition").Invoke<int>(new object[] { "9999984/" + conditions }) != -1);
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (Game1.activeClickableMenu is ShopMenu)
            {
                ShopMenu shop = (ShopMenu)Game1.activeClickableMenu;
                Dictionary<Item, int[]> items = Helper.Reflection.GetPrivateValue<Dictionary<Item, int[]>>(shop, "itemPriceAndStock");
                List<Item> selling = Helper.Reflection.GetPrivateValue<List<Item>>(shop, "forSale");
                int currency = Helper.Reflection.GetPrivateValue<int>(shop, "currency");
                bool isCatalogue = (currency == 0 && selling.Count > 0 && selling[0] is Furniture);
                string shopkeeper = "Robin";


                if (shop.portraitPerson != null || isCatalogue)
                {
                    Dictionary<Item, int> newItemsToSell = new Dictionary<Item, int>();

                    foreach (CustomFurniture f in furniture.Values)
                    {
                        if (!f.data.sellAtShop || (f.data.conditions != "none" && !meetsConditions(f.data.conditions)))
                            continue;

                        if (Game1.getCharacterFromName(f.data.shopkeeper) is NPC sk && !sk.isInvisible)
                            shopkeeper = f.data.shopkeeper;
                        else
                            shopkeeper = "Robin";

                        if (f.data.instantGift != "none")
                        {
                            Game1.player.addItemByMenuIfNecessary(f);
                            Game1.player.mailReceived.Remove(f.data.instantGift);
                            continue;
                        }

                        if ((shop.portraitPerson is NPC shopk && shopk.name == shopkeeper) || isCatalogue)
                                newItemsToSell.Add(f, isCatalogue ? 0 : f.price);
                    }

                    foreach (Item item in newItemsToSell.Keys)
                        if (!items.ContainsKey(item))
                        {
                            items.Add(item, new int[] { newItemsToSell[item], int.MaxValue });
                            selling.Add(item);
                        }
                }

            }
        }

        
    }
}
