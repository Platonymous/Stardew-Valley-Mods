using Microsoft.Xna.Framework;
using PlatoTK;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
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
                    itemPriceAndStock.Add(Frame.GetNew(ComicBook.GetNew(issue.Id.ToString())), new int[] { 100, 1 });

                OpenComicsShop(itemPriceAndStock);

            });

            helper.Events.Input.ButtonPressed += (s, e) =>
            {
                if (e.Button == SButton.F8)
                    foreach (Issue issue in assetManager.LoadIssuesForToday(config.baseYear, Game1.stats.DaysPlayed))
                    {
                        Game1.player.addItemToInventory(Frame.GetNew(ComicBook.GetNew(issue.Id.ToString())));
                        return;
                    }

                if (e.Button == SButton.F7)
                {
                    var itemPriceAndStock = new Dictionary<ISalable, int[]>();

                    foreach (Issue issue in assetManager.LoadIssuesForToday(config.baseYear, Game1.stats.DaysPlayed))
                        itemPriceAndStock.Add(Frame.GetNew(ComicBook.GetNew(issue.Id.ToString())), new int[] { 100, 1 });

                    OpenComicsShop(itemPriceAndStock);
                }
            };

            helper.Events.Player.Warped += Player_Warped;

            helper.Events.GameLoop.GameLaunched += (s, e) =>
            {
                var platoHelper = helper.GetPlatoHelper();

                string comicData = "Plato:IsComicBookObject=true|Id=244342/100/-300/Basic -20/Comic Book/Comic Book";
                string frameData = "Plato:IsComicFrameObject=true|ComicId=216384/painting/1 1/1 1/1/350/Comic Book Frame";

                ComicBook.SaveIndex = platoHelper.Content.GetSaveIndex(
                    "Plato.ComicBook",
                    () => Game1.objectInformation,
                    (handle) => handle.Value == comicData,
                    (handle) => platoHelper.Content.Injections.InjectDataInsert("Data//ObjectInformation", handle.Index, comicData));

                Frame.SaveIndex = platoHelper.Content.GetSaveIndex(
                     "Plato.ComicFrame",
                     "Data//Furniture",
                    (handle) => handle.Value == frameData,
                    (handle) => platoHelper.Content.Injections.InjectDataInsert("Data//Furniture", handle.Index, frameData));

                platoHelper.Harmony.PatchTileDraw("Plato.ComicBookDraw", () => Game1.objectSpriteSheet, assetManager.Placeholder, null, () => ComicBook.SaveIndex.Index);
                platoHelper.Harmony.PatchAreaDraw("Plato.ComicBookFrame", () => Furniture.furnitureTexture, assetManager.Placeholder, null, () => new Rectangle(Frame.SaveIndex.Index * 16 % Furniture.furnitureTexture.Width, Frame.SaveIndex.Index * 16 / Furniture.furnitureTexture.Width * 16, 16, 16));

                platoHelper.Harmony.LinkContruction<StardewValley.Object, ComicBook>();
                platoHelper.Harmony.LinkContruction<Furniture, Frame>();
                platoHelper.Harmony.LinkTypes(typeof(Furniture), typeof(Frame));
                platoHelper.Harmony.LinkTypes(typeof(StardewValley.Object), typeof(ComicBook));
                platoHelper.Harmony.LinkTypes(typeof(StardewValley.Object), typeof(Frame));


                platoHelper.Harmony.RegisterTileAction((tileAction) =>
                {
                    var itemPriceAndStock = new Dictionary<ISalable, int[]>();
                    var p = tileAction.Params;
                    foreach (Issue issue in assetManager.LoadIssuesForToday(p.Length > 0 && !string.IsNullOrEmpty(p[0]) && int.TryParse(p[0], out int year) ? year : config.baseYear, Game1.stats.DaysPlayed))
                        itemPriceAndStock.Add(Frame.GetNew(ComicBook.GetNew(issue.Id.ToString())), new int[] { 100, 1 });
                    OpenComicsShop(itemPriceAndStock);
                }, "OpenComicShop");
            };
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
