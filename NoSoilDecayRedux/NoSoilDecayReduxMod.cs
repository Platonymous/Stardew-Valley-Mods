using StardewValley;
using StardewValley.TerrainFeatures;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using StardewValley.Locations;
using System.Collections.Generic;
using StardewValley.Objects;

namespace NoSoilDecayRedux
{
    public class NoSoilDecayReduxMod : Mod
    {
        private GameLocation savelocation;
        private Vector2 savepoint;

        public override void Entry(IModHelper helper)
        {
            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;
            savepoint = new Vector2(2, 0);

            MenuEvents.MenuClosed += MenuEvents_MenuClosed;
        }

        private void startSprinklers()
        {
            List<GameLocation> gls = new List<GameLocation>();
            gls.Add(Game1.getLocationFromName("Greenhouse"));
            gls.Add(Game1.getFarm());


            for (int i = 0; i < gls.Count; i++)
            {
                GameLocation location = gls[i];
                foreach (Vector2 keyV in location.objects.Keys)
                {
                    if (location.objects[keyV] is Object obj && obj.name.ToLower().Contains("sprinkler"))
                    {
                        obj.DayUpdate(location);
                    }
                }
            }
        }

        private void TimeEvents_AfterDayStarted(object sender, System.EventArgs e)
        {
            savelocation = Game1.getLocationFromName("Town");
            loadHoeDirt();
            startSprinklers();
        }

        private void MenuEvents_MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            if ((Game1.currentLocation is GameLocation) && Game1.newDay)
            {
                saveHoeDirt();
            }
        }

        private void saveHoeDirt()
        {
            List<string> saves = new List<string>();

            List<GameLocation> gls = new List<GameLocation>();
            gls.Add(Game1.getLocationFromName("Greenhouse"));
            gls.Add(Game1.getFarm());


            for (int i = 0; i < gls.Count; i++)
            {
                GameLocation location = gls[i];
                foreach (Vector2 keyV in location.terrainFeatures.Keys)
                {
                    TerrainFeature terrain = location.terrainFeatures[keyV];

                    if (terrain is HoeDirt)
                    {
                        saves.Add(location.name + "-" + keyV.X + "-" + keyV.Y);

                    }
                }
            }

            string savestring = string.Join("/", saves) + ">|ignore|-NoSoilDecayRedux";

            Chest saveobject = new Chest(true);
            saveobject.name = savestring;
      
            if (savelocation.objects.ContainsKey(savepoint))
            {
                 savelocation.objects.Remove(savepoint);
            }

            savelocation.objects.Add(savepoint, saveobject);

        }

        private void loadHoeDirt()
        {
            if (savelocation != null && savelocation.objects.ContainsKey(savepoint) && savelocation.objects[savepoint] is Chest)
            {
        
                string[] hoedirttiles = savelocation.objects[savepoint].name.Split('>')[0].Split('/');

                if(hoedirttiles.Length <= 1)
                {
                    return;
                }

                foreach(string hoedirt in hoedirttiles)
                {
                    string[] placement = hoedirt.Split('-');
                    GameLocation location = Game1.getLocationFromName(placement[0]);
                    Vector2 position = new Vector2(int.Parse(placement[1]), int.Parse(placement[2]));

                  
                    if (!location.terrainFeatures.ContainsKey(position) || !(location.terrainFeatures[position] is HoeDirt))
                    {
                        int state = Game1.isRaining ? 1 : 0;
                        location.terrainFeatures[position] = new HoeDirt(state);
                    }

                    if (location.objects.ContainsKey(position) && location.objects[position] is Object o && (o.name.Equals("Weeds") || o.name.Equals("Stone") || o.name.Equals("Twig"))){
                        location.objects.Remove(position);
                    }


                }

                savelocation.objects.Remove(savepoint);

            }
       

        }

        
       

    }
}
