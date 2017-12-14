using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entoarox.FurnitureAnywhere;
using Harmony;
using StardewValley;
using StardewModdingAPI;
using CustomFurniture;
using Entoarox.Framework.Events;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using StardewValley.Locations;

namespace CustomFurnitureAnywhere
{

    [HarmonyPatch(typeof(FurnitureAnywhereMod), "InitSpecialObject")]
    public class FurnitureAnywhereModFix
    {
        internal static bool Prefix(Item i)
        {
            if (i is CustomFurniture.CustomFurniture)
            {
                for (int c = 0; c < Game1.player.items.Count; c++)
                    if (Game1.player.items[c] != null && Game1.player.items[c].Equals(i))
                        Game1.player.items[c] = new AnywhereCustomFurniture(Game1.player.items[c] as CustomFurniture.CustomFurniture);

                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FurnitureAnywhereMod), "MoreEvents_ActiveItemChanged")]
    public class FurnitureAnywhereModFix2
    {
        internal static bool Prefix(object s, EventArgsActiveItemChanged e, FurnitureAnywhereMod __instance)
        {
 
                if (e.OldItem != null && e.OldItem is AnywhereCustomFurniture)
                {
                    CustomFurnitureAnywhereMod.modhelper.Reflection.GetPrivateMethod(__instance, "RestoreVanillaObjects").Invoke();
                    return false;
                }
            
            return true;
        }
    }

    [HarmonyPatch(typeof(FurnitureAnywhereMod), "RestoreVanillaObjects")]
    public class FurnitureAnywhereModFix3
    {
        internal static bool Prefix()
        {
            for (int c = 0; c < Game1.player.items.Count; c++)
                if (Game1.player.items[c] != null && Game1.player.items[c] is AnywhereCustomFurniture)
                {
                    Game1.player.items[c] = (Game1.player.items[c] as AnywhereCustomFurniture).Revert();
                    return false;
                }
            return true;
        }
    }

    [HarmonyPatch(typeof(GameLocation), "isCollidingPosition", new[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character)})]
    public class FurnitureAnywhereModFix4
    {
        internal static void Prefix(GameLocation __instance, Rectangle position)
        {
            SerializableDictionary<Vector2, StardewValley.Object> objects = __instance.objects;
            Vector2 key = new Vector2((float)(position.Left / Game1.tileSize), (float)(position.Top / Game1.tileSize));

            if (__instance is DecoratableLocation || objects.ContainsKey(key) || (objects[key] is Furniture f && f.furniture_type == 12))
                return;

            foreach (Vector2 k in objects.Keys)
                if (objects[k] is Furniture)
                    if (objects[k].boundingBox.Intersects(position))
                    {
                        Chest chest = new Chest(true);
                        chest.name = "collider";
                        objects.Add(key,chest);
                        return;
                    }
        }

        internal static void Postfix(GameLocation __instance, Rectangle position)
        {
            SerializableDictionary<Vector2, StardewValley.Object> objects = __instance.objects;
            Vector2 key = new Vector2((float)(position.Left / Game1.tileSize), (float)(position.Top / Game1.tileSize));

            if (objects.ContainsKey(key) && objects[key] is Chest chest && chest.name == "collider")
                objects.Remove(key);
        }
    }

}
    
