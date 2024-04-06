using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace Portraiture
{

    internal class FixHelper
    {
        public static Type getTypeFullSDV(string type)
        {
            Type defaulSDV = Type.GetType(type + ", Stardew Valley");

            if (defaulSDV != null)
                return defaulSDV;
            else
                return Type.GetType(type + ", StardewValley");

        }
    }

    [HarmonyPatch]
    internal class PortraitFix
    {
        internal static MethodInfo TargetMethod()
        {
            return FixHelper.getTypeFullSDV("StardewValley.NPC").GetProperty("Portrait").GetMethod;
        }


        internal static void Postfix(NPC __instance, ref Texture2D __result, ref bool __state)
        {
            var load = TextureLoader.getPortrait(__instance, __result);
            __result = load ?? __result;

            if (load == null)
            {
                if (PortraitureMod.config.ShowPortraitsAboveBox)
                    __result = ScaledTexture2D.FromTexture(__result, __result, 1);
                return;
            }

            if (__state && __result.Width > 128)
                __result = ScaledTexture2D.FromTexture(__result, __result, __result.Width == 256 && __result.Height == 256 ? __result.Width / 64f : (__result.Width / 2) / 64f);

            if (!(__result is ScaledTexture2D) && __state && PortraitureMod.config.ShowPortraitsAboveBox)
                    __result = ScaledTexture2D.FromTexture(__result, __result, 1);
        }


    }

}
