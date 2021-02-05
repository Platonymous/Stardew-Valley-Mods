using StardewValley;
using System;

namespace SundropNPCTest
{
    class SundropNPC : NPC, IPatchedSubType
    {
        public Type SubTypeOf => typeof(NPC);
        public bool ShouldPatch { get; private set; } = true;

        //...

        public string Patch_getMasterScheduleEntry(string schedule_key)
        {
            //...

            ShouldPatch = false;
            var result = base.getMasterScheduleEntry(schedule_key);
            ShouldPatch = true;
            return result;
        }
    }
}
