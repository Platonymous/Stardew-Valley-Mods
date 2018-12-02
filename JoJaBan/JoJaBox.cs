using Microsoft.Xna.Framework;
using Netcode;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoJaBan
{
    class JoJaBox : PySObject
    {
        public bool onTarget = false;

        public JoJaBox()
            :base()
        {

        }

        public JoJaBox(CustomObjectData data)
            : base(data,Vector2.Zero)
        {

        }

        public JoJaBox(CustomObjectData data, Vector2 tileLocation)
            : base(data, tileLocation)
        {

        }

        public override Item getOne()
        {
            return new JoJaBox(data);
        }

        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity)
                return true;

            shakeTimer = 500;

            Vector2 t = who.getTileLocation();
            Vector2 newLocation = tileLocation.Value;

            if (t.X == newLocation.X && t.Y == newLocation.Y - 1)
                newLocation.Y++;
            else if (t.X == newLocation.X && t.Y == newLocation.Y + 1)
                newLocation.Y--;
            else if (t.Y == newLocation.Y && t.X == newLocation.X - 1)
                newLocation.X++;
            else if (t.Y == newLocation.Y && t.X == newLocation.X + 1)
                newLocation.X--;
            else
                return true;

            if (!who.currentLocation.isTileOccupied(newLocation) && who.currentLocation.map.GetLayer("Buildings").Tiles[(int)newLocation.X, (int)newLocation.Y] == null)
            {
                who.currentLocation.objects.Remove(tileLocation);
                who.currentLocation.objects.Remove(newLocation);
                who.currentLocation.objects.Add(newLocation, this);
                tileLocation.Value = newLocation;
                who.currentLocation.playSound("hammer");
            }

            if (who.currentLocation.map.GetLayer("Back").Tiles[(int)tileLocation.X, (int)tileLocation.Y].TileIndex.ToString() == Game1.currentLocation.map.Properties["Target"])
            {
                onTarget = true;
                who.currentLocation.playSound("coin");

                bool check = true;

                foreach (JoJaBox obj in who.currentLocation.objects.Values.Where(o => o is JoJaBox))
                    check = obj.onTarget && check;

                if(check)
                    JoJaBanMod.nextLevel(who.currentLocation);

                return true;
            }

            onTarget = false;
            return true;
        }

        public override bool performToolAction(Tool t, GameLocation location)
        {
            return false;
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            return new JoJaBox(JoJaBanMod.boxData);
        }



    }
}
