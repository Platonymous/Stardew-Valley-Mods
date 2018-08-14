using Microsoft.Xna.Framework;
using System;

namespace PyTK.Types
{
    internal class AltGameTime : GameTime
    {
        public AltGameTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime)
            :base(totalGameTime,elapsedGameTime)
        {

        }
    }
}
