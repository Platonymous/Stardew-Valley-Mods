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
        internal static IModHelper _helper;
        internal static IMonitor _monitor;
        internal static EventHandler<MenuChangedEventArgs> addtoshop;

        public override void Entry(IModHelper helper)
        {
            _monitor = Monitor;
            _helper = helper;

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            SeedBagTool seedbag = new SeedBagTool();
            addtoshop = new InventoryItem(seedbag, 30000, 1).addToNPCShop("Pierre");
            CustomObjectData.newObject("Platonymous.SeedBag.Tool", SeedBagTool.texture, Color.White, "Seed Bag", "Empty", 0, customType: typeof(SeedBagTool));
        }
    }
}
