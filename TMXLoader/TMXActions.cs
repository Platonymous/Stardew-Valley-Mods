using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK;
using PyTK.Extensions;
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

        public static bool spawnTreasureAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            List<string> text = action.Split(' ').ToList();

            return spawnTreasureAction(location, int.Parse(text[1]), int.Parse(text[2]), text[3], int.Parse(text[4]), text.Count <= 5 ? "none" : text[5], text.Count <= 6 ? 1 : int.Parse(text[6]), text.Count <= 7 ? "crystal" : text[7]);
        }

        public static bool spawnTreasureAction(string location, int x, int y, string type, int index = -1, string name = "none", int stack = 1, string sound = "crystal")
        {
            return spawnTreasureAction(Game1.getLocationFromName(location), x, y, type, index, name, stack, sound);
        }


        public static bool spawnTreasureAction(GameLocation location, int x, int y, string type, int index = -1, string name = "none", int stack = 1, string sound = "crystal")
        {
            Vector2 position = new Vector2(x, y);
            if (!location.Objects.ContainsKey(position))
            {
                Item chestItem = getItem(type, index, name);

                if (chestItem == null)
                    return false;

                chestItem.Stack = stack;
                Chest chest = new Chest(false);
                chest.TileLocation = position;
                chest.addItem(chestItem);
                location.Objects.Add(position, chest);
                if(sound != "" && sound != "none")
                    Game1.playSound(sound);
                return true;
            }
            return false;
        }

        public static Item getItem(string type, int index = -1, string name = "none")
        {
            Item item = null;

            if (type == "Object")
            {
                if (index != -1)
                    item = new StardewValley.Object(index, 1);
                else if (name != "none")
                    item = new StardewValley.Object(Game1.objectInformation.getIndexByName(name), 1);
            }
            else if (type == "BigObject")
            {
                if (index != -1)
                    item = new StardewValley.Object(Vector2.Zero, index);
                else if (name != "none")
                    item = new StardewValley.Object(Vector2.Zero, Game1.bigCraftablesInformation.getIndexByName(name));
            }
            else if (type == "Ring")
            {
                if (index != -1)
                    item = new Ring(index);
                else if (name != "none")
                    item = new Ring(Game1.objectInformation.getIndexByName(name));
            }
            else if (type == "Hat")
            {
                if (index != -1)
                    item = new Hat(index);
                else if (name != "none")
                    item = new Hat(TMXLoaderMod.helper.Content.Load<Dictionary<int, string>>(@"Data/hats", ContentSource.GameContent).getIndexByName(name));
            }
            else if (type == "Boots")
            {
                if (index != -1)
                    item = new Boots(index);
                else if (name != "none")
                    item = new Boots(TMXLoaderMod.helper.Content.Load<Dictionary<int, string>>(@"Data/Boots", ContentSource.GameContent).getIndexByName(name));
            }
            else if (type == "TV")
            {
                if (index != -1)
                    item = new StardewValley.Objects.TV(index, Vector2.Zero);
                else if (name != "none")
                    item = new TV(TMXLoaderMod.helper.Content.Load<Dictionary<int, string>>(@"Data/Furniture", ContentSource.GameContent).getIndexByName(name), Vector2.Zero);
            }
            else if (type == "IndoorPot")
                item = new StardewValley.Objects.IndoorPot(Vector2.Zero);
            else if (type == "CrabPot")
                item = new StardewValley.Objects.CrabPot(Vector2.Zero);
            else if (type == "Chest")
                item = new StardewValley.Objects.Chest(true);
            else if (type == "Cask")
                item = new StardewValley.Objects.Cask(Vector2.Zero);
            else if (type == "Cask")
                item = new StardewValley.Objects.Cask(Vector2.Zero);
            else if (type == "Furniture")
            {
                if (index != -1)
                    item = new StardewValley.Objects.Furniture(index, Vector2.Zero);
                else if (name != "none")
                    item = new Furniture(TMXLoaderMod.helper.Content.Load<Dictionary<int, string>>(@"Data/Furniture", ContentSource.GameContent).getIndexByName(name), Vector2.Zero);
            }
            else if (type == "Sign")
                item = new StardewValley.Objects.Sign(Vector2.Zero, index);
            else if (type == "Wallpaper")
                item = new StardewValley.Objects.Wallpaper(Math.Abs(index), false);
            else if (type == "Floors")
                item = new StardewValley.Objects.Wallpaper(Math.Abs(index), true);
            else if (type == "MeleeWeapon")
            {
                if (index != -1)
                    item = new MeleeWeapon(index);
                else if (name != "none")
                    item = new MeleeWeapon(TMXLoaderMod.helper.Content.Load<Dictionary<int, string>>(@"Data/weapons", ContentSource.GameContent).getIndexByName(name));

            }
            else if (type == "CustomObject" && PyTK.CustomElementHandler.CustomObjectData.collection.ContainsKey(name))
                item = PyTK.CustomElementHandler.CustomObjectData.collection[name].getObject();
            else if (type == "SDVType")
            {
                try
                {
                    if (index == -1)
                        item = Activator.CreateInstance(PyUtils.getTypeSDV(name)) is Item i ? i : null;
                    else
                        item = Activator.CreateInstance(PyUtils.getTypeSDV(name), index) is Item i ? i : null;
                }
                catch (Exception ex)
                {
                    TMXLoaderMod.monitor.Log(ex.Message + ":" + ex.StackTrace, LogLevel.Error);
                    TMXLoaderMod.monitor.Log("Couldn't load item SDVType: " + name);
                }
            }
            else if (type == "ByType")
            {
                try
                {
                    if (index == -1)
                        item = Activator.CreateInstance(Type.GetType(name)) is Item i ? i : null;
                    else
                        item = Activator.CreateInstance(Type.GetType(name), index) is Item i ? i : null;
                }
                catch (Exception ex)
                {
                    TMXLoaderMod.monitor.Log(ex.Message + ":" + ex.StackTrace, LogLevel.Error);
                    TMXLoaderMod.monitor.Log("Couldn't load item ByType: " + name);
                }
            }

            return item;
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

                    if (!PyUtils.checkEventConditions(inventory.conditions, Game1.player))
                        continue;

                    Item item = getItem(inventory.type,inventory.index, inventory.name);

                    if (item == null)
                        continue;

                    int price = 100;
                    try
                    {
                        price = inventory.price > 0 ? inventory.price : item.salePrice();
                    }
                    catch (Exception ex)
                    {
                        TMXLoaderMod.monitor.Log(ex.Message + ":" + ex.StackTrace, LogLevel.Error);

                    }

                    if (price < 0)
                        price = 100;

                    if (item != null)
                        priceAndStock.AddOrReplace(item, new int[] { price, inventory.stock });
                }
                var shop = new ShopMenu(priceAndStock, 0,  null);
                if (text.Count > 2)
                {
                    try
                    {
                        shop.setUpShopOwner(text[2]);
                        shop.portraitPerson = Game1.getCharacterFromName(text[2]);
                        if (shop.portraitPerson != null)
                        {
                            shop.setUpShopOwner(text[2]);
                            shop.portraitPerson = Game1.getCharacterFromName(text[2]);
                        }
                        else
                        {
                            var npc = new NPC(null, Vector2.Zero, "Town", 0, text[2].Split('.')[0], false, null, TMXLoaderMod.helper.Content.Load<Texture2D>(@"Portraits/" + text[2], ContentSource.GameContent));
                            shop.portraitPerson = npc;
                            Game1.removeThisCharacterFromAllLocations(npc);
                        }
                    }
                    catch (Exception ex)
                    {
                        TMXLoaderMod.monitor.Log(ex.Message + ":" + ex.StackTrace, LogLevel.Error);

                    }
                }
                if (text.Count > 3) {
                    string prefix = text[0] + " " + text[1] + " " + text[2] + " ";
                    string shopText = action.Replace(prefix, "");
                    shop.potraitPersonDialogue = Game1.parseText(shopText, Game1.dialogueFont, 304);
                }
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
                    {
                        Layer l = location.map.GetLayer(layers[0]);
                        if (l != null) {
                            var size = new Microsoft.Xna.Framework.Rectangle(0, 0, l.DisplayWidth / Game1.tileSize, l.DisplayHeight / Game1.tileSize);

                            for (int x = 0; x < size.Width; x++)
                                for (int y = 0; y < size.Height; y++)
                                    copyTileBetweenLayers(location.map, layers[0], layers[1], x, y);
                        }
                    }
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
        /*
        public static Map copyLayers(Map t, string layer1, string layer2)
        {
            Layer newLayer2 = t.GetLayer(layer1);
            t.RemoveLayer(t.GetLayer(layer2));

            newLayer2.Id = layer2;
            t.AddLayer(newLayer2);

            return t;
        }
        */
        public static Map copyTileBetweenLayers(Map t, string layer1, string layer2, int x, int y)
        {
            Location tileLocation = new Location(x, y);

            Tile tile1 = t.GetLayer(layer1).Tiles[tileLocation];

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
