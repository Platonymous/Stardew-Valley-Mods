﻿using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using System.IO;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;
using System.Linq;
using StardewValley.Objects;
using System;
using Harmony;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Extensions;

namespace CustomFurniture
{
    public class CustomFurnitureMod : Mod
    {
        internal static IModHelper helper;
        internal static Dictionary<string,CustomFurniture> furniture = new Dictionary<string, CustomFurniture>();
        internal static Dictionary<string, CustomFurniture> furniturePile = new Dictionary<string, CustomFurniture>();
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            CustomFurnitureMod.helper = helper;
            try
            {
                harmonyFix();
            }
            catch (Exception e)
            {
                Monitor.Log("Harmony Error: Custom deco won't work on tables." + e.StackTrace, LogLevel.Error);
            }
            loadPacks();
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.ConsoleCommands.Add("replace_custom_furniture", "Triggers Custom Furniture Replacement", replaceCustomFurniture);
        }

        public void harmonyFix()
        {
            var instance = HarmonyInstance.Create("Platonymous.CustomFurniture");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void harmonyDraw(Texture2D texture, Vector2 location, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects spriteeffects, float layerDepth)
        {
            Game1.spriteBatch.Draw(texture, location, sourceRectangle, color, rotation, origin, scale, spriteeffects, layerDepth);
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
                Dictionary<Item, int[]> items = Helper.Reflection.GetField<Dictionary<Item, int[]>>(shop, "itemPriceAndStock").GetValue();
                List<Item> selling = Helper.Reflection.GetField<List<Item>>(shop, "forSale").GetValue();
                List<Item> remove = new List<Item>();
                List<Item> additions = new List<Item>();

                foreach (Item i in selling)
                    if (i is Chest chest && furniturePile.Keys.Any(f => f.Equals(i.Name)))
                    {
                        Item cf = furniturePile[furniturePile.Keys.FirstOrDefault(f => f.Equals(i.Name))];
                        items.Add(cf, new int[] { chest.preservedParentSheetIndex.Value, int.MaxValue });
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
            instance.Monitor.Log(text,LogLevel.Trace);
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            Helper.Events.Display.MenuChanged -= OnMenuChanged;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Helper.Events.Display.MenuChanged += OnMenuChanged;
        }

        private void loadPacks()
        {
            int countPacks = 0;
            int countObjects = 0;

            var contentPacks = Helper.ContentPacks.GetOwned();

            foreach (IContentPack cpack in contentPacks)
            {
                string[] cfiles = parseDir(cpack.DirectoryPath, "*.json");

                countPacks += (cfiles.Length - 1);

                foreach (string file in cfiles)
                {
                    if (file.ToLower().Contains("manifest.json"))
                        continue;

                    CustomFurniturePack pack = cpack.ReadJsonFile<CustomFurniturePack>(Path.GetFileName(file));

                    pack.author = cpack.Manifest.Author;
                    pack.version = cpack.Manifest.Version.ToString();
                    string author = pack.author == "none" ? "" : " by " + pack.author;
                    Monitor.Log(pack.name + " " + pack.version + author, LogLevel.Info);
                    foreach (CustomFurnitureData data in pack.furniture)
                    {
                        countObjects++;
                        data.folderName = pack.useid == "none" ? cpack.Manifest.UniqueID : pack.useid;
                        string pileID = data.folderName + "." + Path.GetFileName(file) + "." + data.id;
                        string objectID = pileID;
                        CustomFurnitureMod.log("Load:" + objectID);
                        string tkey = $"{data.folderName}/{ data.texture}";
                        if (!CustomFurniture.Textures.ContainsKey(tkey))
                            CustomFurniture.Textures.Add(tkey, cpack.LoadAsset<Texture2D>(data.texture));
                        CustomFurniture f = new CustomFurniture(data, objectID, Vector2.Zero);
                        furniturePile.Add(pileID, f);
                        furniture.Add(objectID, f);
                    }
                }

            }

            Monitor.Log(countPacks + " Content Packs with " + countObjects + " Objects found.",LogLevel.Trace);
        }

        private string[] parseDir(string path, string extension)
        {
            return Directory.GetFiles(path, extension, SearchOption.AllDirectories);
        }

        private bool meetsConditions(string conditions)
        {
            return (Helper.Reflection.GetMethod(Game1.currentLocation, "checkEventPrecondition").Invoke<int>("9999984/" + conditions) != -1);
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (Game1.activeClickableMenu is ShopMenu)
            {
                ShopMenu shop = (ShopMenu)Game1.activeClickableMenu;
                Dictionary<Item, int[]> items = Helper.Reflection.GetField<Dictionary<Item, int[]>>(shop, "itemPriceAndStock").GetValue();
                List<Item> selling = Helper.Reflection.GetField<List<Item>>(shop, "forSale").GetValue();
                int currency = Helper.Reflection.GetField<int>(shop, "currency").GetValue();
                bool isCatalogue = (currency == 0 && selling.Count > 0 && selling[0] is Furniture);
                string shopkeeper = "Robin";


                if (shop.portraitPerson != null || isCatalogue)
                {
                    Dictionary<Item, int> newItemsToSell = new Dictionary<Item, int>();

                    foreach (CustomFurniture f in furniture.Values)
                    {
                        if (!f.data.sellAtShop || (f.data.conditions != "none" && !meetsConditions(f.data.conditions)))
                            continue;

                        if (Game1.getCharacterFromName(f.data.shopkeeper) is NPC sk && !sk.IsInvisible)
                            shopkeeper = f.data.shopkeeper;
                        else
                            shopkeeper = "Robin";

                        if (f.data.instantGift != "none")
                        {
                            Game1.player.addItemByMenuIfNecessary(f);
                            Game1.player.mailReceived.Remove(f.data.instantGift);
                            continue;
                        }

                        if ((shop.portraitPerson is NPC shopk && shopk.Name == shopkeeper) || isCatalogue)
                                newItemsToSell.Add(f, isCatalogue ? 0 : f.Price);
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
