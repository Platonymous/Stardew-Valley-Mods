using Microsoft.Xna.Framework.Input;
using System;

namespace MuliplayerTweaks
{
    class Config
    {
        public bool UseAllSaves { get; set; } = true;
        public bool LimitPositionSync { get; set; } = true;
        public int LimitPositonSyncMaxDistance { get; set; } = 900;
        public int LimitPositonSyncOverflow { get; set; } = 6;

        public Config()
        {

        }
    }
}
