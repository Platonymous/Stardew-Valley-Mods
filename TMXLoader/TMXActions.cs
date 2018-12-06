using Microsoft.Xna.Framework;
using PyTK;
using PyTK.Extensions;
using PyTK.Lua;
using PyTK.Types;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace TMXLoader
{
    public class TMXActions
    {
        public TMXActions()
        {

        }

        public static bool sayAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            List<string> text = action.Split(' ').ToList();
            bool inDwarvish = false;

            if (text[1] == ("Dwarvish"))
            {
                text.RemoveAt(1);
                if (!Game1.player.canUnderstandDwarves)
                    inDwarvish = true;
            }
            text.RemoveAt(0);
            action = String.Join(" ", text);
            action = inDwarvish ? Dialogue.convertToDwarvish(action) : action;

            Game1.drawDialogueNoTyping(action); return true;
        }

        public static bool gameAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            List<string> text = action.Split(' ').ToList();
            text.RemoveAt(0);
            action = String.Join(" ", text);
            return location.performAction(action, Game1.player, new Location((int)tile.X, (int)tile.Y));
        }

        public static bool confirmAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            List<string> text = action.Split(' ').ToList();
            text.RemoveAt(0);
            action = String.Join(" ", text);
            Game1.activeClickableMenu = new StardewValley.Menus.ConfirmationDialog(action, (who) =>
            {
                if (Game1.activeClickableMenu is StardewValley.Menus.ConfirmationDialog cd)
                    cd.cancel();

            TileAction.invokeCustomTileActions("Success", location, tile, layer);
            });
            
            return true;
        }

        public static bool sayAction(string action)
        {
            return sayAction(action, Game1.currentLocation, Vector2.Zero, "Map");
        }

        public static bool luaAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            string[] a = action.Split(' ');
            if (a.Length > 2)
                if (a[2] == "this")
                {
                    string id = location.Name + "." + layer + "." + tile.Y + tile.Y;
                    if (!PyLua.hasScript(id))
                    {
                        if (layer == "Map")
                        {
                            if (location.map.Properties.ContainsKey("Lua"))
                                PyLua.loadScriptFromString(location.map.Properties["Lua"].ToString(), id);
                            else
                                PyLua.loadScriptFromString("Luau.log(\"Error: Could not find Lua property on Map.\")", id);
                        }
                        else
                        {
                            if (location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Lua", layer) is string lua)
                                PyLua.loadScriptFromString(lua, id);
                            else
                                PyLua.loadScriptFromString("Luau.log(\"Error: Could not find Lua property on Tile.\")", id);
                        }
                    }
                    PyLua.callFunction(id, a[2], new object[] { location, tile, layer });
                }
                else
                    PyLua.callFunction(a[1], a[2], new object[] { location, tile, layer });
            return true;
        }

        public static bool switchLayersAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            string[] actions = action.Split(' ');

            foreach (string s in actions)
            {
                string[] layers = s.Split(':');
                if (layers.Length > 1)
                {
                    if (layers.Length < 4)
                        location.map.switchLayers(layers[0], layers[1]);
                    else
                    {
                        string[] xStrings = layers[2].Split('-');
                        string[] yStrings = layers[3].Split('-');
                        Range xRange = new Range(int.Parse(xStrings[0]), int.Parse(xStrings.Last()) + 1);
                        Range yRange = new Range(int.Parse(yStrings[0]), int.Parse(yStrings.Last()) + 1);

                        foreach (int x in xRange.toArray())
                            foreach (int y in yRange.toArray())
                                location.map.switchTileBetweenLayers(layers[0], layers[1], x, y);
                    }
                }

            }

            return true;
        }

        public static bool copyLayersAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            string[] actions = action.Split(' ');

            foreach (string s in actions)
            {
                string[] layers = s.Split(':');
                if (layers.Length > 1)
                {
                    if (layers.Length < 4)
                        copyLayers(location.map, layers[0], layers[1]);
                    else
                    {
                        string[] xStrings = layers[2].Split('-');
                        string[] yStrings = layers[3].Split('-');
                        Range xRange = new Range(int.Parse(xStrings[0]), int.Parse(xStrings.Last()) + 1);
                        Range yRange = new Range(int.Parse(yStrings[0]), int.Parse(yStrings.Last()) + 1);

                        foreach (int x in xRange.toArray())
                            foreach (int y in yRange.toArray())
                                copyTileBetweenLayers(location.map, layers[0], layers[1], x, y);
                    }
                }

            }

            return true;
        }

        public static Map copyLayers(Map t, string layer1, string layer2)
        {
            Layer newLayer2 = t.GetLayer(layer1);
            t.RemoveLayer(t.GetLayer(layer2));

            newLayer2.Id = layer2;
            t.AddLayer(newLayer2);

            return t;
        }

        public static Map copyTileBetweenLayers(Map t, string layer1, string layer2, int x, int y)
        {
            Location tileLocation = new Location(x, y);

            Tile tile1 = t.GetLayer(layer1).Tiles[tileLocation];
            Tile tile2 = t.GetLayer(layer2).Tiles[tileLocation];

            t.GetLayer(layer2).Tiles[tileLocation] = tile1;
            return t;
        }

        public static bool switchLayersAction(string action, GameLocation location)
        {
            return switchLayersAction(action, location, Vector2.Zero, "Map");
        }

        public static bool lockAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            string[] strings = action.Split(' ');

            if (Game1.player.ActiveObject is Item i && i.ParentSheetIndex == int.Parse(strings[2]) && i.Stack >= int.Parse(strings[1]))
            {
                int amount = int.Parse(strings[1]);
                Game1.playSound("newArtifact");

                if (i.Stack > amount)
                    i.Stack -= amount;
                else
                    Game1.player.removeItemFromInventory(i);

                TileAction.invokeCustomTileActions("Success", location, tile, layer);
            }
            else if (Game1.player.ActiveObject == null)
                TileAction.invokeCustomTileActions("Default", location, tile, layer);
            else
                TileAction.invokeCustomTileActions("Failure", location, tile, layer);
            return true;
        }


        public static bool hasLayer(Map map, string layer)
        {
            foreach (Layer l in map.Layers)
                if (l.Id == layer)
                    return true;

            return false;
        }

        public static Tile getTile(GameLocation location, string layer, int x, int y)
        {
            return location.map.GetLayer(layer).PickTile(new Location(x * Game1.tileSize, y * Game1.tileSize), Game1.viewport.Size);
        }
    }
}
