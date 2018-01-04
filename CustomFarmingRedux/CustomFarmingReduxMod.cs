using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;


namespace CustomFarmingRedux
{
    public class CustomFarmingReduxMod : Mod
    {
        public static IModHelper _helper;
        public static IMonitor _monitor;
        public static List<CustomMachineBlueprint> machines;

        public static string folder = "Machines";

        public override void Entry(IModHelper helper)
        {
            _helper = Helper;
            _monitor = Monitor;
        }
    }
}
