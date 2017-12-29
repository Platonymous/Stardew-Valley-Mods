using StardewModdingAPI;
using StardewValley;
using System;
using PyTK.Extensions;
using PyTK.Types;
using Microsoft.Xna.Framework.Input;
using StardewValley.TerrainFeatures;
using StardewValley.Objects;
using SObject = StardewValley.Object;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace PyTK
{
    public class PyTKMod : Mod
    {
        internal static IModHelper _helper;
        internal static IMonitor _monitor;

        public override void Entry(IModHelper helper)
        {
            _helper = helper;
            _monitor = Monitor;

            /*
            Keys.K.onPressed(new Func<string>(() => Game1.currentGameTime.TotalGameTime.Seconds.ToString()).toLogAction(LogLevel.Info, Monitor));
            ButtonClick.UseToolButton.onTerrainClick<Grass>(o => Monitor.Log($"Number of Weeds: {o.numberOfWeeds}",LogLevel.Info));
            new InventoryItem(new Chest(true), 100).addToNPCShop("Pierre");
            new ItemSelector<SObject>(p => p.name == "Chest").whenAddedToInventory(l => l.useAll(i => i.name = "Test"));
            Helper.Content.Load<Texture2D>($"Maps/MenuTiles",ContentSource.GameContent).setSaturation(0).injectAs($"Maps/MenuTiles");
            Game1.objectSpriteSheet.clone().setSaturation(0).injectTileInto($"Maps/springobjects", 74, 74);
            Game1.objectSpriteSheet.clone().setSaturation(0).injectTileInto($"Maps/springobjects", new Range(129, 166), new Range(129,166));
            */
            
            registerConsoleCommands();
        }

        private void registerConsoleCommands()
        {
            ConsoleCommands.CcLocations.clearSpace().register();
        }

    }
}
