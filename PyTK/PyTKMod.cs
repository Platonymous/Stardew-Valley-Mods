using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;
using StardewValley.TerrainFeatures;
using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using PyTK.Extensions;
using PyTK.Types;
using PyTK.CustomElementHandler;
using PyTK.ConsoleCommands;
using PyTK.CustomTV;
using Harmony;
using System.Reflection;
using StardewValley.Menus;
using System.Collections.Generic;
using PyTK.Overrides;
using xTile.Format;
using System.Linq;
using PyTK.Tiled;

namespace PyTK
{

    internal class Config
    {
        bool patchSpriteBatch { get; set; } = true;
    }

    public class PyTKMod : Mod
    {
        internal static IModHelper _helper;
        internal static IMonitor _monitor;
        internal static bool _activeSpriteBatchFix = true;
        internal static string sdvContentFolder => PyUtils.getContentFolder();

        public override void Entry(IModHelper helper)
        {
            _helper = helper;
            _monitor = Monitor;

            //testing();

            harmonyFix();
            FormatManager.Instance.RegisterMapFormat(new NewTiledTmxFormat());

            SaveHandler.BeforeRebuilding += (a,b) => CustomObjectData.collection.useAll(k => k.Value.sdvId = k.Value.getNewSDVId());
            registerConsoleCommands();
            CustomTVMod.load();
            SaveHandler.setUpEventHandlers();
        }

        private void harmonyFix()
        {
            HarmonyInstance instance = HarmonyInstance.Create("Platonymous.PyTK");
            OvSpritebatch.DrawFix1.init("SObject",PyUtils.getTypeSDV("Object"), new List<string>() { "draw", "drawInMenu", "drawWhenHeld", "drawAsProp" });
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void registerConsoleCommands()
        {
            CcLocations.clearSpace().register();
            CcSaveHandler.cleanup().register();
            CcSaveHandler.savecheck().register();
            CcTime.skip().register();
        }

        private void testing()
        {
            CustomObjectData.newBigObject("Platonymous.BigTest", Game1.bigCraftableSpriteSheet.clone().setSaturation(0), Color.Aquamarine, "Test Machine", "Test Description", 24, craftingData: new CraftingData("Test Machine"));
            CustomObjectData.newObject("Platonymous.Rubici", Game1.objectSpriteSheet.clone().setSaturation(0), Color.Yellow, "Rubici", "Rubici Test", 16, "Rubici", "Minerals -2", 50, -300);
            new CustomObjectData("Platonymous.Rubico" + Color.Red.ToString(), "Rubico/250/-300/Minerals -2/Rubico/A precious stone that is sought after for its rich color and beautiful fluster.", Game1.objectSpriteSheet.clone().setSaturation(0), Color.Red, 16);

            Keys.K.onPressed(() => Monitor.Log($"Played: {Game1.currentGameTime.TotalGameTime.Minutes} min"));
            ButtonClick.UseToolButton.onTerrainClick<Grass>(o => Monitor.Log($"Number of Weeds: {o.numberOfWeeds}", LogLevel.Info));
            new InventoryItem(new Chest(true), 100).addToNPCShop("Pierre");
            new ItemSelector<SObject>(p => p.name == "Chest").whenAddedToInventory(l => l.useAll(i => i.name = "Test"));
            Helper.Content.Load<Texture2D>($"Maps/MenuTiles", ContentSource.GameContent).setSaturation(0).injectAs($"Maps/MenuTiles");
            Game1.objectSpriteSheet.clone().setSaturation(0).injectTileInto($"Maps/springobjects", 74);
            Game1.objectSpriteSheet.clone().setSaturation(0).injectTileInto($"Maps/springobjects", new Range(129, 166), new Range(129, 166));

            Func<string, GameLocation, Vector2, string, bool> tileActionTest = (s, l, t, ly) =>
             {
                 List<string> strings = s.Split(' ').ToList();
                 strings.Remove(strings[0]);
                 Game1.activeClickableMenu = new DialogueBox(String.Join(" ", s));
                 return true;
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
