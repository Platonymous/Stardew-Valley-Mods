using Harmony;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomElementHandlerHarmony
{
    public class CustomElementHandlerHarmonyMod : Mod
    {
        internal static IMonitor _monitor;
        public override void Entry(IModHelper helper)
        {
            _monitor = Monitor;
            var instance = HarmonyInstance.Create("Platonymous.CusromElementHandlerHarmony");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
