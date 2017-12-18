using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Visualize
{
    [HarmonyPatch(typeof(SpriteBatch), "InternalDraw")]
    public class SpriteBatchFix
    {
        internal static bool Prefix(ref SpriteBatch __instance, ref Texture2D texture, ref Color color)
        {
            return Effects.appyEffects(ref __instance, ref color, ref texture);
        }
    }
}
