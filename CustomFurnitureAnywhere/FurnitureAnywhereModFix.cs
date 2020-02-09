using Harmony;
using StardewValley;
using Entoarox.Framework.Events;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using StardewValley.Locations;
using System.Reflection;
using System;

namespace CustomFurnitureAnywhere
{
    [HarmonyPatch]
    public class FurnitureAnywhereModFix
    {
        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(Type.GetType("Entoarox.FurnitureAnywhere.ModEntry, FurnitureAnywhere"), "InitSpecialObject");
        }

        internal static bool Prefix(Item i)
        {
            if (i is CustomFurniture.CustomFurniture)
            {
                for (int c = 0; c < Game1.player.Items.Count; c++)
                    if (Game1.player.Items[c] != null && Game1.player.Items[c].Equals(i))
                        Game1.player.Items[c] = new AnywhereCustomFurniture(Game1.player.Items[c] as CustomFurniture.CustomFurniture);

                return false;
            }
            return true;
        }
    }

    [HarmonyPatch]
    public class FurnitureAnywhereModFix2
    {
        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(Type.GetType("Entoarox.FurnitureAnywhere.ModEntry, FurnitureAnywhere"), "MoreEvents_ActiveItemChanged");
        }

        internal static bool Prefix(object s, EventArgsActiveItemChanged e, object __instance)
        {

            if (e.OldItem != null && e.OldItem is AnywhereCustomFurniture)
            {
                CustomFurnitureAnywhereMod.modhelper.Reflection.GetMethod(__instance, "RestoreVanillaObjects").Invoke();
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    public class FurnitureAnywhereModFix3
    {
        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(Type.GetType("Entoarox.FurnitureAnywhere.ModEntry, FurnitureAnywhere"), "RestoreVanillaObjects");
        }

        internal static bool Prefix()
        {
            for (int c = 0; c < Game1.player.Items.Count; c++)
                if (Game1.player.Items[c] != null && Game1.player.Items[c] is AnywhereCustomFurniture)
                {
                    Game1.player.Items[c] = (Game1.player.Items[c] as AnywhereCustomFurniture).Revert();
                    return false;
                }
            return true;
        }
    }

    [HarmonyPatch]
    public class FurnitureAnywhereModFix4
    {
        internal static MethodInfo TargetMethod()
        {
            if (Type.GetType("StardewValley.GameLocation, Stardew Valley") != null)
                return AccessTools.Method(Type.GetType("StardewValley.GameLocation, Stardew Valley"), "isCollidingPosition", new[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character) });
            else
                return AccessTools.Method(Type.GetType("StardewValley.GameLocation, StardewValley"), "isCollidingPosition", new[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character) });
        }

        internal static void Postfix(ref bool __result, GameLocation __instance, Rectangle position)
        {
            var objects = __instance.objects;

            if (__instance is DecoratableLocation)
                return;

            foreach (Vector2 k in objects.Keys)
                if (objects[k] is Furniture f && f.furniture_type.Value != Furniture.rug && f.boundingBox.Value.Intersects(position))
                {
                    __result = true;
                    return;
                }
        }
    }

    public class FakeObject : StardewValley.Object
    {
        public override bool clicked(Farmer who)
        {
            return false;
        }
    }

    [HarmonyPatch]
    public class FurnitureAnywhereModFix5
    {
        

        internal static MethodInfo TargetMethod()
        {
            try
            {
                if (Type.GetType("StardewValley.Object, Stardew Valley") != null)
                    return AccessTools.Method(Type.GetType("StardewValley.Object, Stardew Valley"), "clicked");
                else
                    return AccessTools.Method(Type.GetType("StardewValley.Object, StardewValley"), "clicked");
            }
            catch
            {
                return AccessTools.Method(typeof(FakeObject), "clicked");
            }
        }

        internal static void Postfix(StardewValley.Object __instance, StardewValley.Farmer who, ref bool __result)
        {
            if (who == null)
                who = Game1.player;

            if (!(Game1.currentLocation is DecoratableLocation) && __instance is Furniture f)
                __result = f.clicked(who);
        }
    }  
}
    
