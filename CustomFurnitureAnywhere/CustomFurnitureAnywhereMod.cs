using Harmony;
using StardewModdingAPI;
using System;
using System.Reflection;
using Entoarox.FurnitureAnywhere;

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
