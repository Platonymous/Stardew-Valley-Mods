using StardewModdingAPI;
using StardewModdingAPI.Events;
using PlatoTK;
using StardewValley;
using StardewValley.Menus;
using System.Linq;
using StardewValley.Objects;

namespace SeedBag
{
    public class SeedBagMod : Mod
    {
        internal static SeedBagMod _instance;
        internal static IModHelper _helper => _instance.Helper;
        internal static ITranslationHelper i18n => _helper.Translation;
        internal static Config config;
        internal static string seedBagToolData;


        public override void Entry(IModHelper helper)
        {
            _instance = this;
            config = helper.ReadConfig<Config>();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Player.InventoryChanged += Player_InventoryChanged;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Game1.player.items.Where(i => i is StardewValley.Tools.GenericTool).ToList().ForEach(i =>
            {
                if (i is Tool t && t.netName.Value.Contains("Plato:IsSeedBag"))
                {
                    Game1.player.removeItemFromInventory(t);
                    try
                    {
                        Game1.player.addItemToInventory(SeedBagTool.GetNew(Helper.GetPlatoHelper(), t.attachments[0], t.attachments[1]));
                    }
                    catch
                    {

                    }
                    }
            });
        }

        private void Player_InventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            e.Added.Where(a => e.IsLocalPlayer && !(a is Tool) && (a?.netName?.Value?.Contains("SeedBag") ?? false)).ToList().ForEach(s =>
            {
               e.Player.removeItemFromInventory(s);
               e.Player.addItemToInventory(SeedBagTool.GetNew(Helper.GetPlatoHelper()));
            });
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var platoHelper = Helper.GetPlatoHelper();

            platoHelper.Harmony.LinkContruction<StardewValley.Tools.GenericTool, SeedBagTool>();
            platoHelper.Harmony.LinkTypes(typeof(StardewValley.Tools.GenericTool), typeof(SeedBagTool));
            SeedBagTool.LoadTextures(platoHelper);
            seedBagToolData = "SeedBag/"+ config.Price +"/-300/Crafting/"+i18n.Get("Name")+"/"+ i18n.Get("Empty");

            SeedBagTool.SaveIndex = platoHelper.Content.GetSaveIndex("Plato.SeedBagTool", () => Game1.objectInformation, (s) => s.Value.StartsWith("SeedBag"), (s) =>
            {
                platoHelper.Content.Injections.InjectDataInsert("Data/ObjectInformation", s.Index, seedBagToolData);
                Helper.Content.InvalidateCache("Data/ObjectInformation");
                platoHelper.Harmony.PatchTileDraw("Plato.SeedBagObjectTile", Game1.objectSpriteSheet, (t) => t.Name == @"Maps\springobjects" || t.Equals(Game1.objectSpriteSheet), SeedBagTool.Texture, null, s.Index);
                platoHelper.Harmony.PatchTileDraw("Plato.SeedBagToolTile", Game1.toolSpriteSheet, (t) => t.Equals(Game1.toolSpriteSheet), SeedBagTool.Texture, null, s.Index);
            });

            Helper.Events.Display.MenuChanged += (s, ev) =>
            {
                if (ev.NewMenu is ShopMenu shop && shop.portraitPerson.Name == config.Shop)
                {
                    var sale = SeedBagTool.GetNew(platoHelper);

                    if (!shop.itemPriceAndStock.Keys.Any(k => k is Tool t && t.netName.Value.Contains("SeedBag") || k.DisplayName == sale.DisplayName || k.DisplayName == i18n.Get("Name")))
                    {
                        shop.itemPriceAndStock.Add(sale, new int[2] { config.Price, 1 });
                        shop.forSale.Add(sale);
                    }
                }
            };

            if (Helper.ModRegistry.GetApi<PlatoTK.APIs.ISerializerAPI>("Platonymous.Toolkit") is PlatoTK.APIs.ISerializerAPI pytk)
            {
                pytk.AddPostDeserialization(ModManifest, (o) =>
                {
                    if (o is Chest c)
                    {
                        var data = pytk.ParseDataString(o);

                        if (data.ContainsKey("@Type") && data["@Type"].Contains("SeedBagTool"))
                        {
                            StardewValley.Object seed = (StardewValley.Object)c.items.FirstOrDefault(i => i.Category == -74);
                            StardewValley.Object fertilizer = (StardewValley.Object)c.items.FirstOrDefault(i => i.Category == -19);
                            return SeedBagTool.GetNew(platoHelper, seed, fertilizer);
                        }
                    }

                    return o;
                });
            }

        }
    }
}
