using StardewModdingAPI;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using StardewValley;
using PyTK.Types;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using System.Linq;

namespace PyTK.Extensions
{
    public static class PyShortcuts
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        /* Input */


        public static object GetFieldValue (this object obj, string field, bool isStatic = false)
        {
            Type t = obj is Type ? (Type)obj : obj.GetType();
            if (obj is Type)
                isStatic = true;
            return t.GetField(field, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(isStatic ? null : obj);
        }

        public static T GetFieldValue<T>(this object obj, string field, bool isStatic = false)
        {
            Type t = obj is Type ? (Type)obj : obj.GetType();
            if (obj is Type)
                isStatic = true;
            return (T) t.GetField(field, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(isStatic ? null : obj);
        }

        public static void SetFieldValue(this object obj, object value, string field, bool isStatic = false)
        {
            Type t = obj is Type ? (Type)obj : obj.GetType();
            if (obj is Type)
                isStatic = true;
            t.GetField(field, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(isStatic ? null : obj, value);
        }

        public static object GetPropertyValue(this object obj, string property, bool isStatic = false)
        {
            Type t = obj is Type ? (Type)obj : obj.GetType(); 
            if (obj is Type)
                isStatic = true;
            return t.GetProperty(property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(isStatic ? null : obj);
        }

               public static void SetPropertyValue(this object obj, object value, string property, bool isStatic = false)
        {
            if (obj is Type)
                isStatic = true;
            Type t = obj is Type ? (Type) obj : obj.GetType();
            t.GetProperty(property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(isStatic ? null : obj, value);
        }

        public static void CallAction(this object obj, string action, params object[] args)
        {
            bool isStatic = false;

            Type t = obj is Type ? (Type)obj : obj.GetType();
            if (obj is Type)
                isStatic = true;
            t.GetMethod(
                action,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                Type.DefaultBinder,
                args.Select(o => o.GetType()).ToArray(),
                new ParameterModifier[0])
                ?.Invoke(isStatic ? null : obj, args);
        }

        public static T CallFunction<T>(this object obj, string action, params object[] args)
        {
            bool isStatic = false;
            if (obj is Type)
                isStatic = true;

            Type t = obj is Type ? (Type)obj : obj.GetType();

            return (T)t.GetMethod(
                            action,
                            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                            Type.DefaultBinder,
                            args.Select(o => o.GetType()).ToArray(),
                            new ParameterModifier[0])
                            ?.Invoke(isStatic ? null : obj, args);
        }

        public static bool isDown(this Keys k)
        {
            return Keyboard.GetState().IsKeyDown(k);
        }

        public static bool isUp(this Keys k)
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
            return new Vector2((Game1.getMouseX() + Game1.viewport.X) / Game1.tileSize, (Game1.getMouseY() + Game1.viewport.Y) / Game1.tileSize);
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
            return new Point(t.X, t.Y);
        }

        public static Vector2 floorValues(this Vector2 t)
        {
            t.X = (int)t.X;
            t.Y = (int)t.Y;
            return t;
        }

        public static string toMD5Hash(this string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));
            return sb.ToString();
        }

        public static T toVector<T>(this int[] arr)
        {
            if (typeof(T) == typeof(Vector2) && arr.Length > 1)
                return (T) (object) new Vector2(arr[0], arr[1]);
            if (typeof(T) == typeof(Vector3) && arr.Length > 2)
                return (T)(object) new Vector3(arr[0], arr[1], arr[2]);
            if (typeof(T) == typeof(Vector4) && arr.Length > 3)
                return (T)(object)new Vector4(arr[0], arr[1], arr[2], arr[3]);
            else
                return (T) (object) null;
        }

        public static T toVector<T>(this List<int> arr)
        {
            return arr.ToArray().toVector<T>();
        }

        public static Color toColor(this Vector4 vec)
        {
            return new Color(vec.X, vec.Y, vec.Z, vec.W);
        }

        public static Color? toColor(this string name)
        {
            if (typeof(Color).GetProperty(name) is PropertyInfo prop)
                return (Color) prop.GetValue(null);

            return null;
        }

        public static int toInt(this string t)
        {
            return int.Parse(t);
        }

        public static bool toBool(this string t)
        {
            return t.ToLower().Equals("true");
        }

        public static bool isNumber(this string t)
        {
            int x = -1;
            return int.TryParse(t,out x);
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
