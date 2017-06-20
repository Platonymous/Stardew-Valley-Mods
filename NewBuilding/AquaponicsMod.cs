using System;
using System.Collections.Generic;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Aquaponics
{
    public class AquaponicsMod : Mod
    {
        public static IModHelper helper;
        public static IMonitor monitor;

        public override void Entry(IModHelper help)
        {
            helper = help;
            monitor = Monitor;

            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;
        }

        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            MenuEvents.MenuChanged -= MenuEvents_MenuChanged;
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if(e.NewMenu is CarpenterMenu carpenter)
            {
                MenuEvents.MenuClosed += MenuEvents_MenuClosed;
                List<BluePrint> blueprints = Helper.Reflection.GetPrivateValue<List<BluePrint>>(carpenter, "blueprints");
                BluePrint newBuildongBluePrint = CreateGreenhouse();
                blueprints.Add(newBuildongBluePrint);
                Game1.activeClickableMenu = carpenter;

                GameEvents.UpdateTick += GameEvents_UpdateTick;
            }
        }

        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is CarpenterMenu carpenter)
            {
                if (carpenter.CurrentBlueprint.name == "Aquaponics")
                {
                    IPrivateField<Building> cBuilding = Helper.Reflection.GetPrivateField<Building>(carpenter, "currentBuilding");

                    if (!(cBuilding.GetValue() is Aquaponics))
                    {
                       cBuilding.SetValue(new Aquaponics(Vector2.Zero, Game1.getFarm()));
                    }
                }
            }
        }

        private void MenuEvents_MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            GameEvents.UpdateTick -= GameEvents_UpdateTick;
            MenuEvents.MenuClosed -= MenuEvents_MenuClosed;
            Farm farm = Game1.getFarm();
            for (int i = 0; i < farm.buildings.Count; i++)
            {
                if (farm.buildings[i] is Building b && !(b is Aquaponics) && b.buildingType == "Aquaponics")
                {
                    farm.buildings[i] = new Aquaponics(new Vector2(b.tileX, b.tileY), farm);
                }
            }
        }

        private BluePrint CreateGreenhouse()
        {
            BluePrint AquaBP = new BluePrint("Aquaponics");
            AquaBP.itemsRequired.Clear();

            string[] strArray2 = "390 200".Split(' ');
            int index = 0;
            while (index < strArray2.Length)
            {
                if (!strArray2[index].Equals(""))
                    AquaBP.itemsRequired.Add(Convert.ToInt32(strArray2[index]), Convert.ToInt32(strArray2[index + 1]));
                index += 2;
            }
            AquaBP.texture = this.Helper.Content.Load<Texture2D>(@"assets\greenhouse.png", ContentSource.ModFolder);
            AquaBP.humanDoor = new Point(2, 2);
            AquaBP.animalDoor = new Point(-1, -1);
            AquaBP.name = "Aquaponics";
            AquaBP.displayName = AquaBP.name;
            AquaBP.description = "A place to grow plants using fertilized water from your Fish!";
            AquaBP.blueprintType = AquaBP.name;
            AquaBP.moneyRequired = 100;
            AquaBP.tilesWidth = 7;
            AquaBP.tilesHeight = 3;
            AquaBP.magical = false;

            return AquaBP;
        }

    }
}
