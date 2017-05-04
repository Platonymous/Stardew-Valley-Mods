using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace SeedBag
{
    public class SeedBagMod : Mod
    {
        internal static Mod mod;
        internal static IMonitor monitor;

        public override void Entry(IModHelper helper)
        {
            mod = this;
            monitor = Monitor;
            SeedBagTool.loadTextures();
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;
        }

        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            MenuEvents.MenuChanged -= MenuEvents_MenuChanged;
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (Game1.activeClickableMenu is ShopMenu)
            {
                ShopMenu shop = (ShopMenu)Game1.activeClickableMenu;
                Dictionary<Item, int[]> items = Helper.Reflection.GetPrivateValue<Dictionary<Item, int[]>>(shop, "itemPriceAndStock");
                List<Item> selling = Helper.Reflection.GetPrivateValue<List<Item>>(shop, "forSale");

                if (shop.portraitPerson.name == "Pierre")
                {
                    Dictionary<Item, int> newItemsToSell = new Dictionary<Item, int>();

                    newItemsToSell.Add(new SeedBagDummy(), 30000);

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
