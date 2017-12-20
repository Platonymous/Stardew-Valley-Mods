using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Reflection;

namespace Capitalism
{
    public class CapitalismMod : Mod
    {
        internal static int counter = -1;
        internal static bool showZero = false;
        internal static Texture2D pointTex;
        private VisualizeHandler vHandler;

        public override void Entry(IModHelper helper)
        {
            var instance = HarmonyInstance.Create("Platonymous.Capitalism");
            instance.PatchAll(Assembly.GetExecutingAssembly());

            pointTex = Helper.Content.Load<Texture2D>("point.png");
            vHandler = new VisualizeHandler();

            Visualize.VisualizeMod.addHandler(vHandler);
        }
    }
}
