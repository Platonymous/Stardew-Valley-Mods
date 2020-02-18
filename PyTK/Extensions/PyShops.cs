using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PyTK.Extensions
{
    public static class PyShops
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        public static bool sells<T>(this ShopMenu shop)
        {
            return shop.getForSale().Find(i => (i is T)) != null;
        }

        public static bool sellsOnly<T>(this ShopMenu shop)
        {
            return shop.getForSale().Find(i => !(i is T)) == null;
        }

        public static bool isFurnitureCataogue(this ShopMenu shop)
        {
            List<ISalable> items = shop.getForSale();
            return (!(shop.portraitPerson is NPC) && shop.sellsOnly<Furniture>());
        }

        public static bool isWallpaperCatalogue(this ShopMenu shop)
        {
            List<ISalable> items = shop.getForSale();
            return (!(shop.portraitPerson is NPC) && shop.sellsOnly<Wallpaper>());
        }

        public static bool isHatShop(this ShopMenu shop)
        {
            List<ISalable> items = shop.getForSale();
            return (!(shop.portraitPerson is NPC) && shop.sellsOnly<Hat>());
        }

        public static int getCurrency(this ShopMenu shop)
        {
            return shop.currency;
        }

        public static List<ISalable> getForSale(this ShopMenu shop)
        {
            return shop.forSale;
        }

        public static Dictionary<ISalable, int[]> getItemPriceAndStock(this ShopMenu shop)
        {
            return shop.itemPriceAndStock;
        }

        public static List<ISalable> forSale(this List<InventoryItem> list)
        {
            return list.Select(i => (i.item as ISalable)).ToList();
        }

        public static Dictionary<ISalable, int[]> priceAndStock(this List<InventoryItem> list)
        {
            Dictionary<ISalable, int[]> priceAndStock = new Dictionary<ISalable, int[]>();
            foreach (InventoryItem inventory in list)
                priceAndStock.Add(inventory.item, new int[] { inventory.price, inventory.stock });
            return priceAndStock;
        }

        public static List<InventoryItem> toInventory<T>(this List<T> list, Func<T, Item> item, Func<T, int> price, Func<T, int> stock)
        {
            List<InventoryItem> inventory = new List<InventoryItem>();
            foreach (T element in list)
                inventory.Add(new InventoryItem(item.Invoke(element), price.Invoke(element), stock.Invoke(element)));
            return inventory;
        }
    }
}
