using System;
using System.Collections.Generic;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

using StardewValley.Menus;
using PyTK.Types;
using PyTK.Extensions;

namespace SeedBag
{
    public class SeedBagMod : Mod
    {
        internal static IModHelper _helper;
        internal static EventHandler<EventArgsClickableMenuChanged> addtoshop;

        public override void Entry(IModHelper helper)
        {
            _helper = helper;
            addtoshop = new InventoryItem(new SeedBagTool(), 30000, 1).addToNPCShop("Pierre");
        }
        

    }
}
