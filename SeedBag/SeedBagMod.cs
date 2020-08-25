using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using PlatoTK;
using StardewValley;
using StardewValley.Menus;

namespace SeedBag
{
    public class SeedBagMod : Mod
    {
        internal static SeedBagMod _instance;
        internal static IModHelper _helper => _instance.Helper;
        internal static ITranslationHelper i18n => _helper.Translation;
        internal static Config config;


        public override void Entry(IModHelper helper)
        {
            _instance = this;
            config = helper.ReadConfig<Config>();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var platoHelper = Helper.GetPlatoHelper();

            platoHelper.Harmony.LinkContruction<StardewValley.Tools.GenericTool, SeedBagTool>();
            platoHelper.Harmony.LinkTypes(typeof(StardewValley.Tools.GenericTool), typeof(SeedBagTool));

            SeedBagTool.LoadTextures(platoHelper);
            string seedBagTool = "Plato:IsSeedBag=true/"+ config.Price +"/-300/Basic -20/"+i18n.Get("Name")+"/"+ i18n.Get("Name");

            SeedBagTool.TileIndex = ((Game1.toolSpriteSheet.Width / 16) * (Game1.toolSpriteSheet.Height / 16)) + 99;

            platoHelper.Harmony.PatchTileDraw("Plato.SeedBagToolTile", () => Game1.toolSpriteSheet, SeedBagTool.Texture, null, SeedBagTool.TileIndex);

            Helper.Events.Display.MenuChanged += (s, ev) =>
            {
                if (ev.NewMenu is ShopMenu shop && shop.portraitPerson.Name == config.Shop)
                {
                    var sale = SeedBagTool.GetNew(platoHelper);
                    shop.itemPriceAndStock.Add(sale, new int[2] { config.Price, 1 });
                    shop.forSale.Add(sale);
                }
            };

            if (Helper.ModRegistry.GetApi<PlatoTK.APIs.ISerializerAPI>("Platonymous.Toolkit") is PlatoTK.APIs.ISerializerAPI pytk)
            {
                pytk.AddPostDeserialization(ModManifest, (o) =>
                {
                    var data = pytk.ParseDataString(o);

                    if (data.ContainsKey("@Type") && data["@Type"].Contains("SeedBagTool"))
                        return SeedBagTool.GetNew(platoHelper);

                    return o;
                });
            }

        }
    }
}
