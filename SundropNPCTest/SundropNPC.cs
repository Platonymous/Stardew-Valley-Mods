using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;

namespace SundropNPCTest
{
    class SundropNPC : NPC, IPatchedSubType
    {
        public bool ShouldPatch { get; private set; } = true;

        public SundropNPC()
            : base()
        {

        }

        public SundropNPC(AnimatedSprite sprite, Vector2 position, int facingDir, string name, LocalizedContentManager content = null)
            : base(sprite,position,facingDir,name,content)
        {

        }
        public SundropNPC(AnimatedSprite sprite, Vector2 position, string defaultMap, int facingDirection, string name, bool datable, Dictionary<int, int[]> schedule, Texture2D portrait)
            : base(sprite, position, defaultMap, facingDirection, name, datable,schedule,portrait)
        {

        }
        public SundropNPC(AnimatedSprite sprite, Vector2 position, string defaultMap, int facingDir, string name, Dictionary<int, int[]> schedule, Texture2D portrait, bool eventActor, string syncedPortraitPath = null)
            : base(sprite, position, defaultMap, facingDir, name, schedule, portrait, eventActor, syncedPortraitPath)
        {

        }

        public string Patch_getMasterScheduleEntry(string schedule_key)
        {
            //....
            Console.WriteLine("-----------TEST-----------");

            ShouldPatch = false;
            var result = base.getMasterScheduleEntry(schedule_key);
            ShouldPatch = true;
            return result;
        }
    }
}
