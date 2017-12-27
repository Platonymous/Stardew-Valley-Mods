using System.Collections.Generic;
using StardewValley;
using SObject = StardewValley.Object;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using StardewValley.Objects;
using StardewValley.Locations;

namespace PyTK.Extensions
{
    public static class PyMaps
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        /// <summary>Looks for an object of the requested type on this map position.</summary>
        /// <returns>Return the object or null.</returns>
        public static T sObjectOnMap<T>(this Vector2 t) where T : SObject
        {
            if (Game1.currentLocation is GameLocation location)
            {
                Dictionary<Vector2, SObject> objects = location.objects;
                if (objects.ContainsKey(t) && (objects[t] is T))
                    return (objects[t] as T);
            }
            return null;
        }

        /// <summary>Looks for an object of the requested type on this map position.</summary>
        /// <returns>Return the object or null.</returns>
        public static T terrainOnMap<T>(this Vector2 t) where T : TerrainFeature
        {
            if (Game1.currentLocation is GameLocation location)
            {
                Dictionary<Vector2, TerrainFeature> terrain = location.terrainFeatures;
                if (terrain.ContainsKey(t) && (terrain[t] is T))
                    return (terrain[t] as T);
            }
            return null;
        }

        /// <summary>Looks for an object of the requested type on this map position.</summary>
        /// <returns>Return the object or null.</returns>
        public static T furnitureOnMap<T>(this Vector2 t) where T : Furniture
        {
            if (Game1.currentLocation is DecoratableLocation location)
            {
                List<Furniture> furniture = location.furniture;
                return (furniture.Find(f => f.getBoundingBox(t).Intersects(new Rectangle((int) t.X * Game1.tileSize, (int) t.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize))) as T);
            }
            return null;
        }

        /// <summary>Looks for an object of the requested type on this map position.</summary>
        /// <returns>Return the object or null.</returns>
        public static SObject sObjectOnMap(this Vector2 t)
        {
            if (Game1.currentLocation is GameLocation location)
            {
                Dictionary<Vector2, SObject> objects = location.objects;
                if (objects.ContainsKey(t))
                    return objects[t];
            }
            return null;
        }

        public static Vector2 toVector2(this Point p)
        {
            return new Vector2(p.X, p.Y);
        }

        public static Vector2 toVector2(this Rectangle r)
        {
            return new Vector2(r.X, r.Y);
        }

        public static Vector2 toVector2(this xTile.Dimensions.Rectangle r)
        {
            return new Vector2(r.X, r.Y);
        }

        public static Point toPoint(this Vector2 t)
        {
            return new Point((int) t.X, (int) t.Y);
        }

        public static Point toPoint(this MouseState t)
        {
            return new Point((int)t.X, (int)t.Y);
        }

        public static Vector2 floorValues(this Vector2 t)
        {
            t.X = (int)t.X;
            t.Y = (int)t.Y;
            return t;
        }



    }
}
