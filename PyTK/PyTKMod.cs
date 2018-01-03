using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;
using StardewValley.TerrainFeatures;
using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using PyTK.CustomElementHandler;
using PyTK.ConsoleCommands;
using PyTK.CustomTV;
using Harmony;
using System.Reflection;
using StardewValley.Menus;
using System.Collections.Generic;

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

            //testing();

            harmonyFix();

            registerConsoleCommands();
            CustomTVMod.load();
            SaveHandler.setUpEventHandlers();
        }

        private void harmonyFix()
        {
            var instance = HarmonyInstance.Create("Platonymous.PyTK");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void registerConsoleCommands()
        {
            CcLocations.clearSpace().register();
            CcSaveHandler.cleanup().register();
        }

        private void testing()
        {
            Keys.K.onPressed(() => Monitor.Log($"Played: {Game1.currentGameTime.TotalGameTime.Minutes} min"));
            ButtonClick.UseToolButton.onTerrainClick<Grass>(o => Monitor.Log($"Number of Weeds: {o.numberOfWeeds}", LogLevel.Info));
            new InventoryItem(new Chest(true), 100).addToNPCShop("Pierre");
            new ItemSelector<SObject>(p => p.name == "Chest").whenAddedToInventory(l => l.useAll(i => i.name = "Test"));
            Helper.Content.Load<Texture2D>($"Maps/MenuTiles", ContentSource.GameContent).setSaturation(0).injectAs($"Maps/MenuTiles");
            Game1.objectSpriteSheet.clone().setSaturation(0).injectTileInto($"Maps/springobjects", 74, 74);
            Game1.objectSpriteSheet.clone().setSaturation(0).injectTileInto($"Maps/springobjects", new Range(129, 166), new Range(129, 166));

            Action<List<string>> tileActionTest = delegate (List<string> s)
            {
                s.Remove(s[0]);
                Game1.activeClickableMenu = new DialogueBox(String.Join(" ", s));
            };

            Action mapMergeTest = delegate ()
            {
                "Beach".toLocation().Map.mergeInto("Town".toLocation().Map, new Vector2(60, 30), new Rectangle(15, 15, 20, 20)).injectAs(@"Maps/Town");
                "Town".toLocation().clearArea(new Rectangle(60, 30, 20, 20));
                "Town".toLocation().Map.addAction(new Vector2(18, 60), new TileAction("testaction", tileActionTest).register(),"Smells interesting");
            };

            SaveEvents.AfterLoad += (s, e) => mapMergeTest();
        }
    }
}
