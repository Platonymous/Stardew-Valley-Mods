using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
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
using Range = TMXLoader.Range;

namespace TMXLoader
{
    public class TMXActions
    {
        public static Dictionary<string, List<Item>> itemLists = new Dictionary<string, List<Item>>();
        public static string lastInventoryId = null;
        public TMXActions()
        {

        }
        public static bool IsBuilding()
        {
            return (Game1.activeClickableMenu is PlatoUIMenu p && p.Id == "BuildablesMenu");
        }



        public static bool spawnTreasureAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            List<string> text = action.Split(' ').ToList();

            return spawnTreasureAction(location, int.Parse(text[1]), int.Parse(text[2]), text[3], text[4], text.Count <= 5 ? "none" : text[5], text.Count <= 6 ? 1 : int.Parse(text[6]), text.Count <= 7 ? "crystal" : text[7]);
        }

        public static bool spawnTreasureAction(string location, int x, int y, string type, string index = "-1", string name = "none", int stack = 1, string sound = "crystal")
        {
            return spawnTreasureAction(Game1.getLocationFromName(location), x, y, type, index, name, stack, sound);
        }

        public static bool addToItemList(string id, Item item)
        {
            if (!itemLists.ContainsKey(id))
                itemLists.Add(id, new List<Item>());

            for (int index = 0; index < itemLists[id].Count; ++index)
                if (itemLists[id][index] != null && itemLists[id][index].canStackWith(item))
                {
                    item.Stack = itemLists[id][index].addToStack(item);
                    return true;
                }

            itemLists[id].Add(item);
            return true;
        }

        public static bool spawnTreasureAction(GameLocation location, int x, int y, string type, string index = "-1", string name = "none", int stack = 1, string sound = "crystal")
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
                if (sound != "" && sound != "none")
                    Game1.playSound(sound);
                return true;
            }
            return false;
        }

        public static bool warpHomeAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            GameLocation home = Game1.getLocationFromName(Game1.player.homeLocation.Value, Game1.player.homeLocation.Value == "FarmHouse");
            Game1.warpFarmer(Game1.player.homeLocation.Value, home.warps[0].X, home.warps[0].Y - 1, Game1.player.FacingDirection, home.isStructure.Value);
            return true;
        }

        public static bool warpIntoAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            List<string> text = action.Split(' ').ToList();
            int w = 0;
            if (text.Count > 2)
                w = int.Parse(text[2]);

            GameLocation target = Game1.getLocationFromName(text[1], false);
            bool up = Game1.player.FacingDirection == 0;
            Game1.warpFarmer(text[1], target.warps[w].X, target.warps[w].Y + (up ? -1 : 1), Game1.player.FacingDirection);
            return true;
        }

        public static bool warpFromAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            List<string> text = action.Split(' ').ToList();
            int w = 0;
            if (text.Count > 3)
                w = int.Parse(text[3]);

            GameLocation target = Game1.getLocationFromName(text[1], false);
            int direction = Game1.player.FacingDirection;

            if (text.Count > 4)
                direction = int.Parse(text[4]);

            List<Warp> warps = target.warps.ToList();

            if (text.Count > 2)
                warps = warps.Where(wp => wp.TargetName == text[2]).ToList();

            Game1.warpFarmer(text[1], warps[w].X + (direction == 1 ? 1 : direction == 3 ? -1 : 0), warps[w].Y + (direction == 0 ? -1 : direction == 2 ? 1 : 0), direction);
            return true;
        }

        public static Item getItem(string type, string index = "-1", string name = "none")
        {
            Item item = null;

            if (type == "Object")
            {
                if (index != "-1")
                    item = new StardewValley.Object(index, 1);
                else if (name != "none")
                    item = new StardewValley.Object(Game1.objectData.Keys.FirstOrDefault(k => k == name || Game1.objectData[k].Name == name), 1);
            }
            else if (type == "BigObject")
            {
                if (index != "-1")
                    item = new StardewValley.Object(Vector2.Zero, index);
                else if (name != "none")
                    item = new StardewValley.Object(Vector2.Zero, Game1.bigCraftableData.Keys.FirstOrDefault(k => k == name || Game1.objectData[k].Name == name));
            }
            else if (type == "Ring")
            {
                if (index != "-1")
                    item = new Ring(index);
                else if (name != "none")
                    item = new Ring(Game1.objectData.Keys.FirstOrDefault(k => k == name || Game1.objectData[k].Name == name));
            }
            else if (type == "Hat")
            {
                if (index != "-1")
                    item = new Hat(index);
                else if (name != "none")
                    item = new Hat(TMXLoaderMod.helper.GameContent.Load<Dictionary<string, string>>("Data/hats").getIndexByName(name));
            }
            else if (type == "Boots")
            {
                if (index != "-1")
                    item = new Boots(index);
                else if (name != "none")
                    item = new Boots(TMXLoaderMod.helper.GameContent.Load<Dictionary<string, string>>("Data/Boots").getIndexByName(name));
            }
            else if (type == "Clothing")
            {
                if (index != "-1")
                    item = new Clothing(index);
                else if (name != "none")
                    item = new Clothing(TMXLoaderMod.helper.GameContent.Load<Dictionary<string, string>>("Data/ClothingInformation").getIndexByName(name));
            }
            else if (type == "TV")
            {
                if (index != "-1")
                    item = new StardewValley.Objects.TV(index, Vector2.Zero);
                else if (name != "none")
                    item = new TV(TMXLoaderMod.helper.GameContent.Load<Dictionary<string, string>>("Data/Furniture").getIndexByName(name), Vector2.Zero);
            }
            else if (type == "IndoorPot")
                item = new StardewValley.Objects.IndoorPot(Vector2.Zero);
            else if (type == "CrabPot")
                item = new StardewValley.Objects.CrabPot();
            else if (type == "Chest")
                item = new StardewValley.Objects.Chest(true);
            else if (type == "Cask")
                item = new StardewValley.Objects.Cask(Vector2.Zero);
            else if (type == "Cask")
                item = new StardewValley.Objects.Cask(Vector2.Zero);
            else if (type == "Furniture")
            {
                if (index != "-1")
                    item = new StardewValley.Objects.Furniture(index, Vector2.Zero);
                else if (name != "none")
                    item = new Furniture(TMXLoaderMod.helper.GameContent.Load<Dictionary<string, string>>("Data/Furniture").getIndexByName(name), Vector2.Zero);
            }
            else if (type == "Sign")
                item = new StardewValley.Objects.Sign(Vector2.Zero, index);
            else if (type == "Wallpaper")
                item = new StardewValley.Objects.Wallpaper(Math.Abs(int.Parse(index)), false);
            else if (type == "Floors")
                item = new StardewValley.Objects.Wallpaper(Math.Abs(int.Parse(index)), true);
            else if (type == "MeleeWeapon")
            {
                if (index != "-1")
                    item = new MeleeWeapon(index);
                else if (name != "none")
                    item = new MeleeWeapon(TMXLoaderMod.helper.GameContent.Load<Dictionary<string, string>>("Data/weapons").getIndexByName(name));

            }
            else if (type == "SDVType")
            {
                try
                {
                    if (index == "-1")
                        item = Activator.CreateInstance(PyUtils.getTypeSDV(name)) is Item i ? i : null;
                    else
                        item = Activator.CreateInstance(PyUtils.getTypeSDV(name), index) is Item i ? i : null;
                }
                catch (Exception ex)
                {
                    TMXLoaderMod.monitor?.Log(ex.Message + ":" + ex.StackTrace, LogLevel.Error);
                    TMXLoaderMod.monitor?.Log("Couldn't load item SDVType: " + name);
                }
            }
            else if (type == "ByType")
            {
                try
                {
                    if (index == "-1")
                        item = Activator.CreateInstance(Type.GetType(name)) is Item i ? i : null;
                    else
                        item = Activator.CreateInstance(Type.GetType(name), index) is Item i ? i : null;
                }
                catch (Exception ex)
                {
                    TMXLoaderMod.monitor?.Log(ex.Message + ":" + ex.StackTrace, LogLevel.Error);
                    TMXLoaderMod.monitor?.Log("Couldn't load item ByType: " + name);
                }
            }

            return item;
        }

        public static bool shopAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            List<string> text = action.Split(' ').ToList();
            lastInventoryId = null;
            if (text.Count < 2)
                return false;

            List<TileShopItem> items = new List<TileShopItem>();
            Dictionary<ISalable, int[]> priceAndStock = new Dictionary<ISalable, int[]>();
            {
                TMXLoaderMod.monitor?.Log("Key:->" + text[1] + "<-");
                foreach(TileShop tss in TMXLoaderMod.tileShops.Keys)
                    TMXLoaderMod.monitor?.Log(tss.id + " == " + text[1] + "> " +(tss.id == text[1]));


                if (TMXLoaderMod.tileShops.FirstOrDefault(kvp => kvp.Key.id == text[1]) is KeyValuePair<TileShop, List<TileShopItem>> tsx)
                    TMXLoaderMod.monitor?.Log(tsx.Key + "-" + tsx.Value);
                else
                    TMXLoaderMod.monitor?.Log("not found");
            }
            if (TMXLoaderMod.tileShops.FirstOrDefault(kvp => kvp.Key.id == text[1] || (text[1].StartsWith("EmptyShop_") && kvp.Key.id == "EmptyShop")) is KeyValuePair<TileShop, List<TileShopItem>> ts && TMXLoaderMod.tileShops.TryGetValue(ts.Key, out items))
            {
                if (text[1].StartsWith("EmptyShop_"))
                    ts.Key.inventoryId = text[1].Split('_')[1];

                foreach (TileShopItem inventory in items)
                {

                    if (!PyUtils.checkEventConditions(inventory.conditions, Game1.player))
                        continue;

                    Item item = getItem(inventory.type, inventory.index, inventory.name);

                    if (item == null)
                        continue;

                    int price = 100;
                    try
                    {
                        price = inventory.price > 0 ? inventory.price : item.salePrice();
                    }
                    catch (Exception ex)
                    {
                        TMXLoaderMod.monitor?.Log(ex.Message + ":" + ex.StackTrace, LogLevel.Error);

                    }

                    if (price < 0)
                        price = 100;

                    if (item != null)
                    {
                        priceAndStock.Remove(item);
                        priceAndStock.Add(item, new int[] { price, inventory.stock });
                    }
                }

                if (ts.Key.inventoryId != null)
                {
                    if (!itemLists.ContainsKey(ts.Key.inventoryId))
                        itemLists.Add(ts.Key.inventoryId, new List<Item>());

                    lastInventoryId = ts.Key.inventoryId;
                    foreach (Item item in itemLists[ts.Key.inventoryId])
                    {
                        priceAndStock.Remove(item.getOne());
                        priceAndStock.Add(item.getOne(), new int[] { item.salePrice(), item.Stack });
                    }

                }

                var shop = new ShopMenu(text[2], new StardewValley.GameData.Shops.ShopData(), new StardewValley.GameData.Shops.ShopOwnerData());
                if (text.Count > 2)
                {
                    try
                    {
                        shop.setUpShopOwner(text[2], shop.ShopId);
                        shop.portraitTexture = Game1.getCharacterFromName(text[2]).Portrait;
                        if (shop.portraitTexture != null)
                        {
                            shop.setUpShopOwner(text[2],shop.ShopId);
                            shop.portraitTexture = Game1.getCharacterFromName(text[2]).Portrait;
                        }
                        else
                        {
                            shop.portraitTexture = TMXLoaderMod.helper.GameContent.Load<Texture2D>($"Portraits/{text[2]}");
                        }

                        if (text[2] == "PlayerShop" && location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Player", layer) is string playerId)
                            shop.ShopData.CustomFields.Add("PlayerShopId", playerId);
                    }
                    catch (Exception ex)
                    {
                        TMXLoaderMod.monitor?.Log(ex.Message + ":" + ex.StackTrace, LogLevel.Error);

                    }
                }
                if (text.Count > 3)
                {
                    string prefix = text[0] + " " + text[1] + " " + text[2] + " ";
                    string shopText = action.Replace(prefix, "");
                    shop.potraitPersonDialogue = Game1.parseText(shopText, Game1.dialogueFont, 304);
                }
                Game1.activeClickableMenu = shop;
                return true;
            }

            return false;
        }

        public static void updateItemListAfterShop(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if (e.OldMenu is ShopMenu sm && lastInventoryId != null && itemLists.ContainsKey(lastInventoryId))
            {
                Dictionary<ISalable, ItemStockInformation> itemPriceAndStock = sm.itemPriceAndStock;
                foreach (Item item in itemLists[lastInventoryId])
                {
                    Item i = item.getOne();
                    int sold = 0;
                    var id = itemPriceAndStock.Keys.FirstOrDefault(k => k.QualifiedItemId == i.QualifiedItemId);
                    if (itemPriceAndStock.ContainsKey(i))
                    {
                        sold = item.Stack - itemPriceAndStock[id].Stock;
                        item.Stack = itemPriceAndStock[id].Stock;
                    }
                    else
                    {
                        sold = item.Stack;
                        item.Stack = 0;
                    }

                    if (sm.ShopData.CustomFields.ContainsKey("PlayerShopId"))
                    {
                        long umid = long.Parse(sm.ShopData.CustomFields["PlayerShopId"]);
                        if (Game1.getFarmer(umid) is Farmer f)
                            ShopMenu.chargePlayer(f, sm.currency, -(i.salePrice() * sold));
                    }
                }

                itemLists[lastInventoryId].RemoveAll(it => it.Stack <= 0);
                lastInventoryId = null;
            }
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
                        if (l != null)
                        {
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

                if (strings.Length >= 4 && strings[3].ToLower() == "persist")
                {
                    string lockid = layer + "_" + (int) tile.X + "_" + (int) tile.Y;

                    if (strings.Length >= 5 && strings[4].ToLower() == "recall")
                        lockid += "_recall";

                    string lname = location.isStructure.Value ? location.uniqueName.Value : location.Name;
                    if (!location.Map.Properties.ContainsKey("PersistentData"))
                        location.Map.Properties.Add("PersistentData", "lock:" + lname + ":" + lockid);
                    else
                        location.Map.Properties["PersistentData"] = location.Map.Properties["PersistentData"].ToString() + ";" + "lock:" + lname + ":" + lockid;

                    if (!location.Map.Properties.ContainsKey("Unlocked"))
                        location.Map.Properties.Add("Unlocked", lockid);
                    else
                        location.Map.Properties["Unlocked"] = location.Map.Properties["Unlocked"].ToString() + ";" + lockid;

                    try
                    {
                        Tile calledTile = location.Map.GetLayer(layer).Tiles[(int)tile.X, (int)tile.Y];

                        if (calledTile != null && (calledTile.Properties.ContainsKey("Action") && calledTile.Properties["Action"].ToString().ToLower().Contains("lock") && calledTile.Properties["Action"].ToString().Contains(action)))
                        {

                            if (strings.Length >= 5 && strings[4].ToLower() == "recall")
                            {
                                if (calledTile.Properties.ContainsKey("Recall"))
                                    calledTile.Properties["Action"] = calledTile.Properties["Recall"];
                                else
                                    calledTile.Properties["Action"] = calledTile.Properties["Success"];
                            }
                            else
                                calledTile.Properties.Remove("Action");
                        }
                    }
                    catch
                    {

                    }
            }

               

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

        public static Tile getTile(Map map, string layer, int x, int y)
        {
            if (map.GetLayer(layer) is Layer l)
                if (l.Tiles[x, y] is Tile t)
                    return t;

            return null;
        }

        public static void setTile(Map map, string layer, int x, int y, int index, string tilesheet = "")
        {
            if (map.GetLayer(layer) is Layer l)
                if (l.Tiles[x, y] is StaticTile t && tilesheet == "")
                    t.TileIndex = index;
                else if (tilesheet != "" && map.GetTileSheet(tilesheet) is TileSheet ts)
                    l.Tiles[x, y] = new StaticTile(l, ts, BlendMode.Alpha, index);
        }

        public static void setTile(Map map, string layer, int x, int y, Tile tile)
        {
            if (map.GetLayer(layer) is Layer l)
                l.Tiles[x, y] = tile;
        }

        public static StaticTile createStaticTile(Map map, string layer, string tilesheet, int index)
        {
            if (map.GetLayer(layer) is Layer l && map.GetTileSheet(tilesheet) is TileSheet ts)
                return new StaticTile(l, ts, BlendMode.Alpha, index);

            return null;
        }

        public static AnimatedTile createAnimatedTile(Map map, string layer, string tilesheet, long intervall, params int[] index)
        {
            List<StaticTile> tiles = new List<StaticTile>();
            if (map.GetLayer(layer) is Layer l && map.GetTileSheet(tilesheet) is TileSheet ts)
            {
                foreach (int i in index)
                    tiles.Add(new StaticTile(l, ts, BlendMode.Alpha, i));

                return new AnimatedTile(l, tiles.ToArray(), intervall);
            }

            return null;
        }
        public static Point GetSpouseRoomSpot(GameLocation location)
        {
            if (location is FarmHouse fh)
            {
                if (fh.Map.getMapProperty("SpouseRoom") is string srValue && srValue != "")
                {
                    List<int> srSpot = srValue.Split(' ').ToList().Select(s =>
                    {
                        if (int.TryParse(s, out int i))
                            return i;

                        return -1;
                    }).ToList();

                    if (srSpot.Count >= 2 && srSpot[0] != -1 && srSpot[1] != -1)
                        return new Point(srSpot[0], srSpot[1]);

                }

                if (fh.Map.getMapProperty("Bed") is string bedValue && bedValue != "")
                {
                    List<int> bedspot = bedValue.Split(' ').ToList().Select(s =>
                    {
                        if (int.TryParse(s, out int i))
                            return i;

                        return -1;
                    }).ToList();

                    if (bedspot.Count >= 2 && bedspot[0] != -1 && bedspot[1] != -1)
                    {
                        if (fh.upgradeLevel > 2 || fh is Cabin)
                            return new Point(bedspot[0] + 8, bedspot[1] - 3);
                        else
                            return new Point(bedspot[0] + 6, bedspot[1] - 3);
                    }

                }


                if (fh.upgradeLevel >= 2)
                    return new Point(35, 10);
                else
                    return new Point(29, 1);
            }

            return Point.Zero;
        }

        public static void SetSpouseRoom(GameLocation location)
        {
            SetSpouseRoom(location, GetSpouseRoomSpot(location));
        }

        public static void SetSpouseRoom(GameLocation location, int x, int y)
        {
            SetSpouseRoom(location, new Point(x, y));
        }

        public static void SetSpouseRoom(GameLocation location, Point p)
        {
            if (location is FarmHouse fh && fh.owner is Farmer f && p != Point.Zero)
            {
                int h = 9;
                int w = 6;
                int x1 = p.X;
                int y1 = p.Y;

                int x2 = x1 + w - 1;
                int y2 = y1 + h - 1;

                string pos = ":" + x1 + "-" + x2 + ":" + y1 + "-" + y2;
                string posf = ":" + x1 + "-" + x2 + ":" + y1 + "-" + (y2 - 1);
                string posid = x1 + "." + y1;

                string lastposid = location.map.getMapProperty("TMXL_SPOUSE_LASTPOS");
                string currentSpouse = location.map.getMapProperty("TMXL_SPOUSE_CURRENT");
                string lastpos = pos;
                string lastposf = posf;

                if (string.IsNullOrEmpty(lastposid))
                    lastposid = posid;
                else
                {
                    List<int> l = posid.Split('.').ToList().Select(s =>
                    {
                        if (int.TryParse(s, out int i))
                            return i;

                        return -1;
                    }).ToList();

                    if (l.Count >= 2 && l[0] != -1 && l[1] != -1)
                    {
                        lastpos = ":" + l[0] + "-" + (l[0] + w - 1) + ":" + l[1] + "-" + (l[1] + h - 1);
                        lastposf = ":" + l[0] + "-" + (l[0] + w - 1) + ":" + l[1] + "-" + (l[1] + h - 2);
                    }
                }


                if (currentSpouse != (f.spouse ?? "") || lastposid != posid)
                {
                    if (!string.IsNullOrEmpty(currentSpouse))
                        switchLayersAction("SwitchLayers AlwaysFront:AlwaysFront" + currentSpouse + lastposf + " Front:Front" + currentSpouse + lastposf + "  Buildings:Buildings" + currentSpouse + lastpos + "  Back:Back" + currentSpouse + lastpos, location);

                    if (string.IsNullOrEmpty(f.spouse))
                        return;

                    if (hasLayer(location.map, "Back" + currentSpouse))
                        switchLayersAction("SwitchLayers AlwaysFront:AlwaysFront" + f.spouse + posf + " Front:Front" + f.spouse + posf + "  Buildings:Buildings" + f.spouse + pos + "  Back:Back" + f.spouse + pos, location);
                }

                location.map.setMapProperty("TMXL_SPOUSE_CURRENT", f.spouse ?? "");
                location.map.setMapProperty("TMXL_SPOUSE_LASTPOS", posid);
            }
        }
    }
}
