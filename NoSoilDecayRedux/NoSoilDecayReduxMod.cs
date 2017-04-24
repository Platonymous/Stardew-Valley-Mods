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
        private bool hoeDirtReplaced;

        public override void Entry(IModHelper helper)
        {
            LocationEvents.CurrentLocationChanged += LocationEvents_CurrentLocationChanged; ;
            GameEvents.OneSecondTick += GameEvents_OneSecondTick;
            
        }

        private void GameEvents_OneSecondTick(object sender, System.EventArgs e)
        {
            if(!hoeDirtReplaced && Game1.timeOfDay == 600 && Game1.activeClickableMenu == null)
            {
                loadHoeDirt();
                hoeDirtReplaced = true;
            }

        }

        private void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            

            if (e.PriorLocation is Farm)
            {
                saveHoeDirt();
                hoeDirtReplaced = false;
               
            }
          
        }

        private void saveHoeDirt()
        {
            Monitor.Log("Save Hoedirt");
            savelocation = Game1.getLocationFromName("Town");
            savepoint = new Vector2(2, 0);

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
                        saves.Add(location.name + "-" + keyV.X + "-" + keyV.Y + "-|ignore|-NoSoilDecayRedux");

                    }
                }
            }

            string savestring = string.Join("/", saves);

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
            Monitor.Log("Load Soil");
            savelocation = Game1.getLocationFromName("Town");
            savepoint = new Vector2(2, 0);

            if (savelocation != null && savelocation.objects.ContainsKey(savepoint) && savelocation.objects[savepoint] is Chest)
            {
        
                string[] hoedirttiles = savelocation.objects[savepoint].name.Split('/');
                

                foreach(string hoedirt in hoedirttiles)
                {
                    string[] placement = hoedirt.Split('-');
                    GameLocation location = Game1.getLocationFromName(placement[0]);
                    Vector2 position = new Vector2(int.Parse(placement[1]), int.Parse(placement[2]));

                    

                    if(!location.terrainFeatures.ContainsKey(position) || !(location.terrainFeatures[position] is HoeDirt))
                    {
                        int state = Game1.isRaining ? 1 : 0;
                        location.terrainFeatures[position] = new HoeDirt(state);
                    }


                }


            }
       

        }

        
       

    }
}
