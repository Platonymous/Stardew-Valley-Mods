using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.Reflection;
using Visualize;
using PyTK.Extensions;

namespace Capitalism
{
    public class CapitalismMod : Mod
    {
        private MainVisualizeHandler vHandlerMain;
        internal static List<IVisualizeHandler> vHandlers;
        internal static IModHelper _helper;
        internal static IMonitor _monitor;

        public override void Entry(IModHelper helper)
        {
            _monitor = Monitor;
            _helper = helper;
            onEntry();

            var instance = HarmonyInstance.Create("Platonymous.Capitalism");
            instance.PatchAll(Assembly.GetExecutingAssembly());

            setVisualizeHandlers();
        }

        private void setVisualizeHandlers()
        {
            vHandlers = new List<IVisualizeHandler>();
            vHandlers.Add(new Components.CapitalismMoneyDialPatch.MoneyDialVisualizeHandler());
            vHandlerMain = new MainVisualizeHandler();
            VisualizeMod.addHandler(vHandlerMain);
        }

        private void onEntry()
        {
            Components.CapitalismMoneyDialPatch.CapitalismMoneyDial.onEntry();
        }
    }
}
