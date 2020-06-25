using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
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

            AssetManager.LoadImagesInShop = config.loadCoversInShop;

            helper.ConsoleCommands.Add("comics", "", (s, p) =>
            {
                var itemPriceAndStock = new Dictionary<ISalable, int[]>();

                foreach (Issue issue in assetManager.LoadIssuesForToday(p.Length > 0 && !string.IsNullOrEmpty(p[0]) && int.TryParse(p[0], out int year) ? year : config.baseYear, Game1.stats.DaysPlayed))
                    itemPriceAndStock.Add(new Frame(new ComicBook(issue.Id.ToString()), Vector2.Zero), new int[] { 100, 1 });

                OpenComicsShop(itemPriceAndStock);

            });

            helper.Events.Player.Warped += Player_Warped;

            PyTK.PyUtils.addTileAction("OpenComicShop", (s, p, loc, pos, layer) =>
             {
                 var itemPriceAndStock = new Dictionary<ISalable, int[]>();

                 foreach (Issue issue in assetManager.LoadIssuesForToday(!string.IsNullOrEmpty(p) && int.TryParse(p, out int year) ? year : config.baseYear, Game1.stats.daysPlayed))
                     itemPriceAndStock.Add(new Frame(new ComicBook(issue.Id.ToString()), Vector2.Zero), new int[] { 100, 1 });
                 
                 OpenComicsShop(itemPriceAndStock);
                 return true;
             });
        }

        public bool OpenComicsShop(Dictionary<ISalable, int[]> itemPriceAndStock)
        {
            var shop = new ShopMenu(itemPriceAndStock, 0, ComicsModAPI.Shopkeeper, null, null, null);

            if (Game1.getCharacterFromName(ComicsModAPI.Shopkeeper) is NPC npc)
                shop.portraitPerson = npc;

            if (ComicsModAPI.ShopText != "")
                shop.potraitPersonDialogue = ComicsModAPI.ShopText;

            Game1.activeClickableMenu = shop;

            return true;
        }

        public override object GetApi()
        {
            return new ComicsModAPI();
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if(e.NewLocation is SeedShop s && ComicsModAPI.PlaceShop)
                s.Map.GetLayer("Buildings").Tiles[6, 18].Properties["Action"] = "OpenComicShop";
        }
    }
}
