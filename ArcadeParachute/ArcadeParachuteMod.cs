using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using PyTK.Extensions;
using PyTK.CustomElementHandler;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using PyTK.Types;

namespace ArcadeParachute
{
    public class ArcadeParachuteMod : Mod
    {
        internal static IMonitor monitor;
        public static CustomObjectData sdata;
        internal static IMod _instance;

        public override void Entry(IModHelper helper)
        {
            _instance = this;
            monitor = Monitor;
            helper.Events.GameLoop.GameLaunched += (o, e) =>
            {
                sdata = new CustomObjectData("Parachute", "Parachute/0/-300/Crafting -9/Play 'Parachute with Wumbus' at home!/true/true/0/Parachute", helper.Content.Load<Texture2D>(@"assets/arcade.png"), Color.White, bigCraftable: true, type: typeof(MachineParachute));
                if (Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone") is IMobilePhoneApi api)
                {
                    Texture2D appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "mobile_app_icon.png"));
                    bool success = api.AddApp(Helper.ModRegistry.ModID + "Mobile2048", "2048", () =>
                    {
                        Game1.currentMinigame = new GameParachute();
                    }, appIcon);
                }
            };
            helper.Events.GameLoop.SaveLoaded += (o, e) => addToCatalogue();

            helper.ConsoleCommands.Add("getparachute", "", (s, p) =>
             {
                 Game1.player.addItemByMenuIfNecessary(sdata.getObject());
             });
        }

        public void addToCatalogue()
        {
            new InventoryItem(sdata.getObject(), 5000, 1).addToNPCShop("Gus");
        }
    }
}
