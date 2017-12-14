using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomFurnitureAnywhere
{
    public class CustomFurnitureAnywhereMod : Mod
    {
        public static IModHelper modhelper;
        public static IMonitor modmonitor;

        public override void Entry(IModHelper helper)
        {
            modhelper = helper;
            modmonitor = Monitor;
            harmonyFix();
        }

        public void harmonyFix()
        {
            var instance = HarmonyInstance.Create("Platonymous.CustomFurnitureAnywhere");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
