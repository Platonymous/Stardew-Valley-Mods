using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using PyTK.Types;
using PyTK.Extensions;


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
            addtoshop = new InventoryItem(new SeedBagTool(), 30000, 1).addToNPCShop("Pierre");
        }
    }
}
