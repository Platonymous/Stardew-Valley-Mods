using StardewValley;
using StardewValley.TerrainFeatures;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using StardewValley.Locations;

namespace NoSoilDecayRedux
{
    public class NoSoilDecayReduxMod : Mod
    {

        public override void Entry(IModHelper helper)
        {
            LocationEvents.CurrentLocationChanged += LocationEvents_CurrentLocationChanged; ;
            
            
        }

        private void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            if(e.NewLocation is FarmHouse)
            {
                plantFalseCrops(Game1.getFarm());
                plantFalseCrops(Game1.getLocationFromName("Greenhouse"));
            }

            if(e.NewLocation is Farm)
            {
                removeFalseCrops(Game1.getFarm());
            }

            if (e.NewLocation.name == "Greenhouse")
            {
                removeFalseCrops(Game1.getLocationFromName("Greenhouse"));
            }
        }

        private void removeFalseCrops(GameLocation location)
        {
            foreach (Vector2 keyV in location.terrainFeatures.Keys)
            {
                TerrainFeature terrain = location.terrainFeatures[keyV];

                if (terrain is HoeDirt)
                {
                    HoeDirt hoeDirt = (HoeDirt)terrain;

                    if (hoeDirt.crop != null && hoeDirt.crop.dead)
                    {
                        hoeDirt.crop = null;
                    }

                }
            }
        }

        private void plantFalseCrops(GameLocation location)
        {
            foreach (Vector2 keyV in location.terrainFeatures.Keys)
            {
                TerrainFeature terrain = location.terrainFeatures[keyV];

                if (terrain is HoeDirt)
                {
                    HoeDirt hoeDirt = (HoeDirt)terrain;

                    if (hoeDirt.crop == null)
                    {
                        string season = Game1.currentSeason;
                        int cropIndex = 770;
      
                        if (Game1.IsWinter || (Game1.dayOfMonth == 28 && Game1.currentSeason == "fall"))
                        {   
                                cropIndex = 498;
                        }

                        Crop placeholder = new Crop(cropIndex, (int)keyV.X, (int)keyV.Y);
                        placeholder.dead = true;
                        hoeDirt.crop = placeholder;
                    }
                    
                }
            }
        }

    }
}
