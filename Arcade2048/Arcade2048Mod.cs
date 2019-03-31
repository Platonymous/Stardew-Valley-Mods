using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;

namespace Arcade2048
{
    public class Arcade2048Mod : Mod
    {
        internal static IMonitor monitor;
        public static CustomObjectData sdata;

        public override void Entry(IModHelper helper)
        {
            monitor = Monitor;
            helper.Events.GameLoop.GameLaunched += (o, e) =>
            {
                sdata = new CustomObjectData("2048", "2048/0/-300/Crafting -9/Play '2048 by Platonymous' at home!/true/true/0/2048", helper.Content.Load<Texture2D>(@"Assets/arcade.png"), Color.White, bigCraftable: true, type: typeof(Machine2048));
            };
            helper.Events.GameLoop.SaveLoaded += (o, e) => addToCatalogue();
        }

        public void addToCatalogue()
        {
            new InventoryItem(sdata.getObject(), 5000, 1).addToNPCShop("Gus");
        }
    }
}
