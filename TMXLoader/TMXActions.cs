using Microsoft.Xna.Framework;
using PyTK;
using PyTK.Extensions;
using PyTK.Lua;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
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

        public static bool shopAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            List<string> text = action.Split(' ').ToList();

            if (text.Count < 2)
                return false;

            List<TileShopItem> items = new List<TileShopItem>();

            if (TMXLoaderMod.tileShops.TryGetValue(text[1], out items))
            {
                Dictionary<Item, int[]> priceAndStock = new Dictionary<Item, int[]>();
                foreach (TileShopItem inventory in items)
                {
                    Item item = null;

                    if (inventory.type == "Object")
                    {
                        if (inventory.index != -1)
                            item = new StardewValley.Object(inventory.index, 1);
                        else if (inventory.name != "none")
                            item = new StardewValley.Object(Game1.objectInformation.getIndexByName(inventory.name), 1);
                    }
                    else if (inventory.type == "BigObject")
                    {
                        if (inventory.index != -1)
                            item = new StardewValley.Object(Vector2.Zero, inventory.index);
                        else if (inventory.name != "none")
                            item = new StardewValley.Object(Vector2.Zero, Game1.bigCraftablesInformation.getIndexByName(inventory.name));
                    }
                    else if (inventory.type == "Ring")
                    {
                        if (inventory.index != -1)
                            item = new Ring(inventory.index);
                        else if (inventory.name != "none")
                            item = new Ring(Game1.objectInformation.getIndexByName(inventory.name));
                    }
                    else if (inventory.type == "Hat")
                    {
                        if (inventory.index != -1)
                            item = new Hat(inventory.index);
                        else if (inventory.name != "none")
                            item = new Hat(TMXLoaderMod.helper.Content.Load<Dictionary<int, string>>(@"Data/hats", ContentSource.GameContent).getIndexByName(inventory.name));
                    }
                    else if (inventory.type == "Boots")
                    {
                        if (inventory.index != -1)
                            item = new Boots(inventory.index);
                        else if (inventory.name != "none")
                            item = new Boots(TMXLoaderMod.helper.Content.Load<Dictionary<int, string>>(@"Data/Boots", ContentSource.GameContent).getIndexByName(inventory.name));
                    }
                    else if (inventory.type == "TV")
                    {
                        if (inventory.index != -1)
                            item = new StardewValley.Objects.TV(inventory.index, Vector2.Zero);
                        else if (inventory.name != "none")
                            item = new TV(TMXLoaderMod.helper.Content.Load<Dictionary<int, string>>(@"Data/Furniture", ContentSource.GameContent).getIndexByName(inventory.name), Vector2.Zero);
                    }
                    else if (inventory.type == "IndoorPot")
                        item = new StardewValley.Objects.IndoorPot(Vector2.Zero);
                    else if (inventory.type == "CrabPot")
                        item = new StardewValley.Objects.CrabPot(Vector2.Zero);
                    else if (inventory.type == "Chest")
                        item = new StardewValley.Objects.Chest(true);
                    else if (inventory.type == "Cask")
                        item = new StardewValley.Objects.Cask(Vector2.Zero);
                    else if (inventory.type == "Cask")
                        item = new StardewValley.Objects.Cask(Vector2.Zero);
                    else if (inventory.type == "Furniture")
                    {
                        if (inventory.index != -1)
                            item = new StardewValley.Objects.Furniture(inventory.index, Vector2.Zero);
                        else if (inventory.name != "none")
                            item = new Furniture(TMXLoaderMod.helper.Content.Load<Dictionary<int, string>>(@"Data/Furniture", ContentSource.GameContent).getIndexByName(inventory.name), Vector2.Zero);
                    }
                    else if (inventory.type == "Sign")
                        item = new StardewValley.Objects.Sign(Vector2.Zero, inventory.index);
                    else if (inventory.type == "Wallpaper")
                        item = new StardewValley.Objects.Wallpaper(Math.Abs(inventory.index), false);
                    else if (inventory.type == "Floors")
                        item = new StardewValley.Objects.Wallpaper(Math.Abs(inventory.index), true);
                    else if (inventory.type == "MeleeWeapon")
                    {
                        if (inventory.index != -1)
                            item = new MeleeWeapon(inventory.index);
                        else if (inventory.name != "none")
                            item = new MeleeWeapon(TMXLoaderMod.helper.Content.Load<Dictionary<int, string>>(@"Data/weapons", ContentSource.GameContent).getIndexByName(inventory.name));

                    }
                    
                    else if (inventory.type == "CustomObject" && PyTK.CustomElementHandler.CustomObjectData.collection.ContainsKey(inventory.name))
                        item = PyTK.CustomElementHandler.CustomObjectData.collection[inventory.name].getObject();
                    else if (inventory.type == "SDVType")
                    {
                        try
                        {
                            if (inventory.index == -1)
                                item = Activator.CreateInstance(PyUtils.getTypeSDV(inventory.name)) is Item i ? i : null;
                            else
                                item = Activator.CreateInstance(PyUtils.getTypeSDV(inventory.name), inventory.index) is Item i ? i : null;
                        }
                        catch
                        {
                            TMXLoaderMod.monitor.Log("Couldn't load to shop SDVType: " + inventory.name);
                        }
                    }
                    else if (inventory.type == "ByType")
                    {
                        try
                        {
                            if (inventory.index == -1)
                                item = Activator.CreateInstance(Type.GetType(inventory.name)) is Item i ? i : null;
                            else
                                item = Activator.CreateInstance(Type.GetType(inventory.name), inventory.index) is Item i ? i : null;
                        }
                        catch
                        {
                            TMXLoaderMod.monitor.Log("Couldn't load to shop ByType: " + inventory.name);
                        }
                    }
                    int price = 100;
                    try
                    {
                        price = inventory.price > 0 ? inventory.price : item.salePrice();
                    }
                    catch
                    {

                    }

                    if (price < 0)
                        price = 100;

                    if (item != null)
                        priceAndStock.AddOrReplace(item, new int[] { price, inventory.stock });
                }
                var shop = new ShopMenu(priceAndStock, 0,  null);
                if (text.Count > 2)
                    shop.portraitPerson = Game1.getCharacterFromName(text[2]);
                Game1.activeClickableMenu = shop;
                return true;
            }

            return false;
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
