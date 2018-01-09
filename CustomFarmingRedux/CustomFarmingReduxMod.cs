using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using System.IO;
using PyTK;
using PyTK.Extensions;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;

namespace CustomFarmingRedux
{
    public class CustomFarmingReduxMod : Mod
    {
        public static IModHelper _helper;
        public static IMonitor _monitor;
        public static List<CustomMachineBlueprint> machines;
        public static List<CustomFarmingPack> packs;
        public static Config _config;
        public static string folder = "Machines";

        public override void Entry(IModHelper helper)
        {
            _helper = Helper;
            _monitor = Monitor;
            _config = Helper.ReadConfig<Config>();

            loadPacks();

            Keys.J.onPressed(() => Game1.activeClickableMenu = new ItemGrabMenu(machines.toList(p => (Item) new CustomMachine(p))));
            Keys.K.onPressed(() => Game1.currentLocation.objects.toList(k => k.Value is CustomMachine ? k.Key : Vector2.Zero).useAll(x => Game1.currentLocation.objects.Remove(x)));
        }

        private void loadPacks()
        {
            PyUtils.loadContentPacks(out packs, Path.Combine(Helper.DirectoryPath, folder), SearchOption.AllDirectories, Monitor);
            machines = new List<CustomMachineBlueprint>();
            foreach (CustomFarmingPack pack in packs)
                foreach (CustomMachineBlueprint blueprint in pack.machines)
                    {
                        blueprint.pack = pack;
                        machines.AddOrReplace(blueprint);
                        if (blueprint.production != null)
                            foreach (RecipeBlueprint recipe in blueprint.production)
                                recipe.mBlueprint = blueprint;
                    }

            Monitor.Log(packs.Count + " Content Packs with " + machines.Count + " machines found.", LogLevel.Trace);
        }
    }
}
