using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace Comics
{
    public class ComicsMod : Mod
    {
        internal Config config;

        internal AssetManager assetManager;

        public override void Entry(IModHelper helper)
        {

            config = helper.ReadConfig<Config>();

            assetManager = new AssetManager(helper);
            
            helper.ConsoleCommands.Add("comics", "", (s, p) =>
            {
                var itemPriceAndStock = new Dictionary<ISalable, int[]>();

                foreach (Issue issue in assetManager.LoadIssuesForToday(p.Length > 0 && !string.IsNullOrEmpty(p[0]) && int.TryParse(p[0], out int year) ? year : config.baseYear))
                        itemPriceAndStock.Add(new Frame(new ComicBook(issue.Id.ToString()), Vector2.Zero), new int[] { 100, 1 });

                var shop = new ShopMenu(itemPriceAndStock, 0, "Pierre", null, null, null);
            
               Game1.activeClickableMenu = shop;
            });

            helper.Events.Player.Warped += Player_Warped;

            PyTK.PyUtils.addTileAction("OpenComicShop", (s, p, loc, pos, layer) =>
             {
                 var itemPriceAndStock = new Dictionary<ISalable, int[]>();

                 foreach (Issue issue in assetManager.LoadIssuesForToday(!string.IsNullOrEmpty(p) && int.TryParse(p, out int year) ? year : config.baseYear))
                     itemPriceAndStock.Add(new Frame(new ComicBook(issue.Id.ToString()), Vector2.Zero), new int[] { 100, 1 });

                 var shop = new ShopMenu(itemPriceAndStock, 0, "Pierre", null, null, null);

                 Game1.activeClickableMenu = shop;
                 return true;
             });
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if(e.NewLocation is SeedShop s)
                s.Map.GetLayer("Buildings").Tiles[6, 18].Properties["Action"] = "OpenComicShop";
        }
    }
}
