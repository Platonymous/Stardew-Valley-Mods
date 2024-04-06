using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HarpOfYobaRedux
{
    class RainMagic : IMagic
    {
        public RainMagic()
        {

        }

        public void doMagic(bool playedToday)
        {
            if (Game1.isRaining || !Game1.currentLocation.IsOutdoors)
                return;

            Game1.playSound("thunder_small");
            bool isRaining = Game1.IsRainingHere();
            Game1.delayedActions.Add(new DelayedAction(500, () =>
            {
                Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContextId()).isRaining.Value = true;
                Game1.updateWeather(Game1.currentGameTime);
            }));

            Game1.delayedActions.Add(new DelayedAction(2000, () => water(Game1.currentLocation)));
            Game1.delayedActions.Add(new DelayedAction(6000, () =>
            {
                Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContextId()).isRaining.Value = isRaining;
                Game1.updateWeather(Game1.currentGameTime);
            }));
        }

        private double getDistance(Vector2 i, Vector2 j)
        {
            float distX = Math.Abs(j.X - i.X);
            float distY = Math.Abs(j.Y - i.Y);
            double dist = Math.Sqrt((distX * distX) + (distY * distY));
            return dist;
        }

        private void water(GameLocation location)
        {
            foreach (var hoe in Game1.currentLocation.terrainFeatures.Keys.Where(k => Game1.currentLocation.terrainFeatures[k] is HoeDirt))
            {
                (Game1.currentLocation.terrainFeatures[hoe] as HoeDirt).state.Value = 1;
                (Game1.currentLocation.terrainFeatures[hoe] as HoeDirt).tickUpdate(Game1.currentGameTime);
            }
        }


    }

    public class TerrainSelector<T> where T : TerrainFeature
    {
        public Func<TerrainFeature, bool> predicate = o => o is T;

        public TerrainSelector(Func<T, bool> predicate = null)
        {
            if (predicate != null)
                this.predicate = o => (o is T) ? predicate.Invoke((T)o) : false;
        }

        public List<Vector2> keysIn(GameLocation location = null)
        {
            if (location == null)
                location = Game1.currentLocation;
            List<Vector2> list = (location.terrainFeatures.FieldDict).Keys.ToList();
            list.RemoveAll(k => !predicate(location.terrainFeatures.FieldDict[k].Value));
            return list;
        }

        public List<TerrainFeature> valuesIn(GameLocation location = null)
        {
            if (location == null)
                location = Game1.currentLocation;

            List<TerrainFeature> list = location.terrainFeatures.FieldDict.Select(t => predicate(t.Value.Value) ? t.Value.Value : null).ToList();
            return list;
        }
    }
}
