using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;

namespace Snake
{
    public class SnakeMod : Mod
    {
        public static IMonitor monitor;
        public static IModHelper helper;
        public static CustomObjectData sdata;
        public override void Entry(IModHelper helper)
        {
            monitor = Monitor;
            SnakeMod.helper = helper;
            sdata = new CustomObjectData("Snake", "Snake/0/-300/Crafting -9/Play 'Snake by Platonymous' at home!/true/true/0/Snake", helper.Content.Load<Texture2D>(@"Assets/arcade.png"), Color.White, bigCraftable: true, type: typeof(SnakeMachine));
            helper.Events.GameLoop.SaveLoaded += (o, e) => addToCatalogue();
        }

        public void addToCatalogue()
        {
            new InventoryItem(sdata.getObject(), 5000, 1).addToNPCShop("Gus");
        }
    }
}
