using System;
using System.Collections.Generic;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

using StardewValley.Menus;
using PyTK.Types;
using PyTK.Extensions;
using Microsoft.Xna.Framework.Input;

namespace SeedBag
{
    public class SeedBagMod : Mod
    {
        internal static IModHelper _helper;
        internal static IMonitor _monitor;
        internal static EventHandler<EventArgsClickableMenuChanged> addtoshop;

        public override void Entry(IModHelper helper)
        {
            _monitor = Monitor;
            _helper = helper;
            Keys.K.onPressed(() => Game1.player.addItemByMenuIfNecessary(new SeedBagTool()));
            addtoshop = new InventoryItem(new SeedBagTool(), 30000, 1).addToNPCShop("Pierre");
        }
        

    }
}
