using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using System.IO;

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
                sdata = new CustomObjectData("2048", "2048/0/-300/Crafting -9/Play '2048 by Platonymous' at home!/true/true/0/2048", helper.Content.Load<Texture2D>(@"assets/arcade.png"), Color.White, bigCraftable: true, type: typeof(Machine2048));
                if (Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone") is IMobilePhoneApi api)
                {
                    Texture2D appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "mobile_app_icon.png"));
                    bool success = api.AddApp(Helper.ModRegistry.ModID + "Mobile2048", "2048", () =>
                    {
                        Game1.currentMinigame = new Game2048();
                    }, appIcon);
                }
            };
            helper.Events.GameLoop.SaveLoaded += (o, e) => addToCatalogue();
        }

        public void addToCatalogue()
        {
            new InventoryItem(sdata.getObject(), 5000, 1).addToNPCShop("Gus");
        }
    }
}
