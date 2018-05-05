using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Screener
{
    [HarmonyPatch(typeof(Game1))]
    [HarmonyPatch("Update")]
    [HarmonyPatch(new Type[] { typeof(GameTime) })]
    internal class ClearFix
    {
        internal static void Postfix()
        {
            ScreenerMod.takeScreenshot();
        }


    }
}
