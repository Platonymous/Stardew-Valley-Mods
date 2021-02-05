using StardewModdingAPI;
using StardewValley;

namespace SundropNPCTest
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            SubTypePatcher.Patch<NPC, SundropNPC>();
        }
    }
}
