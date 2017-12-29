using StardewModdingAPI;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using StardewValley;
using PyTK.Types;
using System.Collections.Generic;
using System;

namespace PyTK.Extensions
{
    public static class PyShortcuts
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        /* Input */

        public static bool isDown<T>(this Keys k)
        {
            return Keyboard.GetState().IsKeyDown(k);
        }

        public static bool isUp<T>(this Keys k)
        {
            return Keyboard.GetState().IsKeyUp(k);
        }

        /* Checks */


        public static bool isLocation(this string t)
        {
            return Game1.getLocationFromName(t) is GameLocation;
        }

        /* Maps */

        public static Vector2 getTileAtMousePosition(this GameLocation t)
        {
            return new Vector2((int)(Game1.getOldMouseX() + Game1.viewport.X) / Game1.tileSize, (int)(Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize);
        }

        /* Converter */

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
            return new Point((int)t.X, (int)t.Y);
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

        public static int toInt(this string t)
        {
            return int.Parse(t);
        }

        public static bool toBool(this string t)
        {
            return t.ToLower().Equals("true");
        }

        public static GameLocation toLocation(this string t)
        {
            return Game1.getLocationFromName(t);
        }

        public static ConsoleCommand toConsoleCommand(this Action<string,string[]> t, string name, string documentation)
        {
            return new ConsoleCommand(name, documentation, t);
        }

        public static ConsoleCommand toConsoleCommand(this Action t, string name, string documentation)
        {
            return new ConsoleCommand(name, documentation, (s,p) => t.Invoke());
        }

        public static ConsoleCommand toConsoleCommand(this Action<string> t, string name, string documentation)
        {
            return new ConsoleCommand(name, documentation, (s, p) => t.Invoke(s));
        }

        public static ConsoleCommand toConsoleCommand(this Action<string[]> t, string name, string documentation)
        {
            return new ConsoleCommand(name, documentation, (s, p) => t.Invoke(p));
        }


    }
}
