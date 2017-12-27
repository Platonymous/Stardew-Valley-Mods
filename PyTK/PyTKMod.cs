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

            Keys.K.onPressed(new Func<string>(() => (Game1.currentLocation is GameLocation gl) ? gl.name : "").toLogAction());
            ButtonClick.UseToolButton.onTerrainClick<Grass>(o => Monitor.Log("Number of Weeds:" + o.numberOfWeeds));
            new InventoryItem(new Chest(true), 100).addToNPCShop("Pierre").once();
            new ItemSelector<SObject>(p => p.name == "Chest").whenAddedToInventory(new Action<List<SObject>>(l => l.useAll(i => i.name = "Test")));
            Helper.Content.Load<Texture2D>($"Maps/MenuTiles",ContentSource.GameContent).setSaturation(0).injectAs($"Maps/MenuTiles");
        }

    }
}
