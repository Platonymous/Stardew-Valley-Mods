using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using PyTK.Types;
using PyTK.Extensions;
using PyTK.CustomElementHandler;
using Microsoft.Xna.Framework;

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
            SeedBagTool seedbag = new SeedBagTool();
            CustomObjectData.newObject("Platonymous.SeedBag.Tool", SeedBagTool.texture, Color.White, i18n.Get("Name"), i18n.Get("Empty"), 0, customType: typeof(SeedBagTool));
            InventoryItem bag = new InventoryItem(seedbag, config.Price, 1);
            bag.addToNPCShop(config.Shop);
        }
    }
}
