using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Visualize
{
    [HarmonyPatch(typeof(SpriteBatch), "Draw", new[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(SpriteEffects), typeof(float) })]
    public class SpriteBatchFix1
    {
        internal static bool  Prefix(ref SpriteBatch __instance, ref Texture2D texture, ref Color color)
        {
            return Effects.appyEffects(ref __instance, ref color, ref texture);
        }
    }

    [HarmonyPatch(typeof(SpriteBatch), "Draw", new[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color) })]
    public class SpriteBatchFix2
    {
        internal static bool Prefix(ref SpriteBatch __instance, ref Texture2D texture, ref Color color)
        {
            return Effects.appyEffects(ref __instance, ref color, ref texture);
        }
    }

    [HarmonyPatch(typeof(SpriteBatch), "Draw", new[] { typeof(Texture2D), typeof(Rectangle), typeof(Color) })]
    public class SpriteBatchFix3
    {
        internal static bool Prefix(ref SpriteBatch __instance, ref Texture2D texture, ref Color color)
        {
            return Effects.appyEffects(ref __instance, ref color, ref texture);
        }
    }


    [HarmonyPatch(typeof(SpriteBatch), "Draw", new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle), typeof(Color), typeof(float), typeof(Vector2), typeof(Vector2), typeof(SpriteEffects), typeof(float) })]
    public class SpriteBatchFix4
    {
        internal static bool Prefix(ref SpriteBatch __instance, ref Texture2D texture, ref Color color)
        {
            return Effects.appyEffects(ref __instance, ref color, ref texture);
        }
    }

    [HarmonyPatch(typeof(SpriteBatch), "Draw", new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) })]
    public class SpriteBatchFix5
    {
        internal static bool Prefix(ref SpriteBatch __instance, ref Texture2D texture, ref Color color)
        {
            return Effects.appyEffects(ref __instance, ref color, ref texture);
        }
    }

    [HarmonyPatch(typeof(SpriteBatch), "Draw", new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color) })]
    public class SpriteBatchFix6
    {
        internal static bool Prefix(ref SpriteBatch __instance, ref Texture2D texture, ref Color color)
        {
            return Effects.appyEffects(ref __instance, ref color, ref texture);
        }
    }

    [HarmonyPatch(typeof(SpriteBatch), "Draw", new[] { typeof(Texture2D), typeof(Vector2), typeof(Color)})]
    public class SpriteBatchFix7
    {
        internal static bool Prefix(ref SpriteBatch __instance, ref Texture2D texture, ref Color color)
        {
            return Effects.appyEffects(ref __instance, ref color, ref texture);
        }
    }
}
