using StardewModdingAPI;
using StardewModdingAPI.Events;
using SObject = StardewValley.Object;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using StardewValley.TerrainFeatures;
using StardewValley;
using PyTK.Types;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace PyTK.Extensions
{
    public static class PyEvents
    {
        private static IModEvents Events { get; } = PyTKMod._events;
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        /* Basics */

        /// <summary>Wraps the the method in the predicate, so it only executes if the predicate returns true.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<TArgs> addPredicate<TArgs>(this EventHandler<TArgs> t, Func<TArgs, bool> p)
        {
            EventHandler<TArgs> d = delegate (object sender, TArgs e)
            {
                if (p.Invoke(e))
                    t.Invoke(sender, e);
            };

            return d;
        }

        /// <summary>Wraps the the method in the predicate, so it only executes if the predicate returns true.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<TArgs> addPredicate<TArgs>(this EventHandler<TArgs> t, string conditions)
        {
            EventHandler<TArgs> d = delegate (object sender, TArgs e)
            {
                if (PyUtils.checkEventConditions(conditions))
                    t.Invoke(sender, e);
            };

            return d;
        }

        /// <summary>Wraps the the method in the predicate, so it only executes if the predicate returns true.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static Action<TArgs> addPredicate<TArgs>(this Action<TArgs> t, Func<TArgs, bool> p)
        {
            Action<TArgs> d = delegate (TArgs e)
            {
                if (p.Invoke(e))
                    t.Invoke(e);
            };

            return d;
        }

        /// <summary>Wraps the the method in the predicate, so it only executes if the predicate returns true.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static Action<TArgs> addPredicate<TArgs>(this Action<TArgs> t, string conditions)
        {
            Action<TArgs> d = delegate (TArgs e)
            {
                if (PyUtils.checkEventConditions(conditions))
                    t.Invoke(e);
            };

            return d;
        }

        /// <summary>Wraps the the method so it only executes once.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<TArgs> once<TArgs>(this EventHandler<TArgs> t)
        {
            EventHandler<TArgs> r = delegate (object sender, TArgs e)
            {
                if (t != null)
                    t.Invoke(sender,e);
                t = null;
            };

            return r;
        }

        /// <summary>Wraps the the method so it only executes once.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static Action<T> once<T>(this Action<T> t)
        {
            Action<T> r = delegate (T e)
            {
                if(t != null)
                    t.Invoke(e);
                t = null;
            };

            return r;
        }


        /// <summary>Wraps the the method so it only executes until the predicate returns true.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<TArgs> until<TArgs>(this EventHandler<TArgs> t, Func<TArgs, bool> p, bool checkAfter = false)
        {
            EventHandler<TArgs> d = delegate (object sender, TArgs e)
            {
                if (!checkAfter && p.Invoke(e))
                    t = null;

                if(t != null)
                    t.Invoke(sender, e);

                if (checkAfter && p.Invoke(e))
                    t = null;
            };

            return d;
        }

        /// <summary>Wraps the the method so it only executes until the predicate returns true.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<TArgs> until<TArgs>(this EventHandler<TArgs> t, string conditions, bool checkAfter = false)
        {
            EventHandler<TArgs> d = delegate (object sender, TArgs e)
            {
                if (!checkAfter && PyUtils.checkEventConditions(conditions))
                    t = null;

                if (t != null)
                    t.Invoke(sender, e);

                if (checkAfter && PyUtils.checkEventConditions(conditions))
                    t = null;
            };

            return d;
        }

        /// <summary>Wraps the the method so it only executes until the predicate returns true.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static Action<T> until<T>(this Action<T> t, Func<bool> p, bool checkAfter = false)
        {
            Action<T> d = delegate (T e)
            {
                if (!checkAfter && p.Invoke())
                    t = null;

                if (t != null)
                    t.Invoke(e);

                if (checkAfter && p.Invoke())
                    t = null;
            };

            return d;
        }

        /// <summary>Wraps the the method so it only executes until the predicate returns true.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static Action<T> until<T>(this Action<T> t, string conditions, bool checkAfter = false)
        {
            Action<T> d = delegate (T e)
            {
                if (!checkAfter && PyUtils.checkEventConditions(conditions))
                    t = null;

                if (t != null)
                    t.Invoke(e);

                if (checkAfter && PyUtils.checkEventConditions(conditions))
                    t = null;
            };

            return d;
        }

        /* Menu */

        /// <summary>Wraps the the method so it only executes if a menu of the requested type opens and adds it to <see cref="IDisplayEvents.MenuChanged"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<MenuChangedEventArgs> onActivation<T>(this IClickableMenu t, EventHandler<MenuChangedEventArgs> handler) where T : IClickableMenu
        {
            EventHandler<MenuChangedEventArgs> d = delegate (object sender, MenuChangedEventArgs e)
            {
                if (e.NewMenu is T)
                    handler.Invoke(sender, e);
            };

            PyEvents.Events.Display.MenuChanged += d;

            return d;
        }

        /// <summary>Wraps the the method so it only executes if a menu of the requested type opens and adds it to <see cref="IDisplayEvents.MenuChanged"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<MenuChangedEventArgs> onActivation<T>(this IClickableMenu t, Action<T> action) where T : IClickableMenu
        {
            EventHandler<MenuChangedEventArgs> d = delegate (object sender, MenuChangedEventArgs e)
            {
                if (e.NewMenu is T)
                    action.Invoke((T) e.NewMenu);
            };

            PyEvents.Events.Display.MenuChanged += d;

            return d;
        }

        /// <summary>Wraps the the method so it only executes if a menu of the requested type closes and adds it to MenuEvents.MenuClosed.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<MenuChangedEventArgs> onClose<T>(this IClickableMenu t, EventHandler<MenuChangedEventArgs> handler) where T : IClickableMenu
        {
            EventHandler<MenuChangedEventArgs> d = delegate (object sender, MenuChangedEventArgs e)
            {
                if (e.OldMenu is T)
                    handler.Invoke(sender, e);
            };

            PyEvents.Events.Display.MenuChanged += d;

            return d;
        }

        /// <summary>Wraps the the method so it only executes if a menu of the requested type closes and adds it to MenuEvents.MenuClosed.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<MenuChangedEventArgs> onClose<T>(this IClickableMenu t, Action<T> action) where T : IClickableMenu
        {
            EventHandler<MenuChangedEventArgs> d = delegate (object sender, MenuChangedEventArgs e)
            {
                if (e.OldMenu is T oldMenu)
                    action.Invoke(oldMenu);
            };

            PyEvents.Events.Display.MenuChanged += d;

            return d;
        }

        /// <summary>Generates a method that adds this inventory to a shop that matches the defined conditions and adds it to <see cref="IDisplayEvents.MenuChanged"/>.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<MenuChangedEventArgs> addToShop(this List<InventoryItem> inventory, Func<ShopMenu, bool> predicate)
        {
            EventHandler<MenuChangedEventArgs> d = delegate (object sender, MenuChangedEventArgs e)
            {
                ShopMenu shop = (ShopMenu)e.NewMenu;
                List<ISalable> forSale = shop.getForSale();
                Dictionary<ISalable, int[]> priceAndStock = shop.getItemPriceAndStock();
                forSale = forSale.Union(inventory.forSale()).ToList();
                priceAndStock = priceAndStock.Union(inventory.priceAndStock()).ToDictionary(dict => dict.Key, dict => dict.Value);
            };

            d = d.addPredicate(e => predicate.Invoke(((ShopMenu) e.NewMenu)));

            return Game1.activeClickableMenu.onActivation<ShopMenu>(d);
        }

        /// <summary>Generates a method that adds this inventory to a shop of an NPC adds it to <see cref="IDisplayEvents.MenuChanged"/>.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<MenuChangedEventArgs> addToNPCShop(this List<InventoryItem> items, string shopkeeper)
        {
            return items.addToShop((shop) => shop.portraitPerson.Name == shopkeeper);
        }

        /// <summary>Generates a method that adds this inventory to the furniture catalogue and adds it to <see cref="IDisplayEvents.MenuChanged"/>.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<MenuChangedEventArgs> addToFurnitureCatalogue(this List<InventoryItem> items)
        {
            return items.addToShop(p => p.isFurnitureCataogue());
        }

        /// <summary>Generates a method that adds this inventory to the wallpaper catalogue and adds it to <see cref="IDisplayEvents.MenuChanged"/>.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<MenuChangedEventArgs> addToWallpaperCatalogue(this List<InventoryItem> items)
        {
            return items.addToShop(p => p.isWallpaperCatalogue());
        }


        /// <summary>Generates a method that adds this inventory to the hat shop and adds it to <see cref="IDisplayEvents.MenuChanged"/>.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<MenuChangedEventArgs> addToHatShop(this List<InventoryItem> items)
        {
            return items.addToShop(p => p.isHatShop());
        }

        /// <summary>Generates a method that adds this inventory to a shop that matches the defined conditions and adds it to <see cref="IDisplayEvents.MenuChanged"/>.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<MenuChangedEventArgs> addToShop(this InventoryItem inventory, Func<ShopMenu, bool> predicate)
        {
            EventHandler<MenuChangedEventArgs> d = delegate (object sender, MenuChangedEventArgs e)
            {
                ShopMenu shop = (ShopMenu)e.NewMenu;
                List<ISalable> forSale = shop.getForSale();
                Dictionary<ISalable, int[]> priceAndStock = shop.getItemPriceAndStock();
                forSale.AddOrReplace(inventory.item);
                priceAndStock.AddOrReplace(inventory.item, new int[] { inventory.price, inventory.stock });
            };

            d = d.addPredicate(e => predicate.Invoke(((ShopMenu) e.NewMenu)));

            return Game1.activeClickableMenu.onActivation<ShopMenu>(d);
        }

        /// <summary>Generates a method that adds this inventory to a shop of an NPC adds it to <see cref="IDisplayEvents.MenuChanged"/>.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<MenuChangedEventArgs> addToNPCShop(this InventoryItem item, string shopkeeper)
        {
            return item.addToShop((shop) => shop.portraitPerson is NPC npc && npc.Name == shopkeeper);
        }

        /// <summary>Generates a method that adds this inventory to a shop of an NPC adds it to <see cref="IDisplayEvents.MenuChanged"/>.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<MenuChangedEventArgs> addToNPCShop(this InventoryItem item, string shopkeeper, string conditions)
        {
            return item.addToShop((shop) => shop.portraitPerson is NPC npc && npc.Name == shopkeeper && PyUtils.CheckEventConditions(conditions));
        }

        /// <summary>Generates a method that adds this inventory to all shops in a specific location <see cref="IDisplayEvents.MenuChanged"/>.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<MenuChangedEventArgs> addToLocationShop(this InventoryItem item, GameLocation location, string conditions = "")
        {
            return item.addToShop((shop) => Game1.currentLocation is GameLocation l && l == location && PyUtils.CheckEventConditions(conditions));
        }

        /// <summary>Generates a method that adds this inventory to all shops in a specific location <see cref="IDisplayEvents.MenuChanged"/>.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<MenuChangedEventArgs> addToLocationShop(this InventoryItem item, string location, string conditions = "")
        {
            return item.addToShop((shop) => Game1.currentLocation is GameLocation l && l.Name == location && PyUtils.CheckEventConditions(conditions));
        }

       

        /// <summary>Generates a method that adds this inventory to the furniture catalogue and adds it to <see cref="IDisplayEvents.MenuChanged"/>.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<MenuChangedEventArgs> addToFurnitureCatalogue(this InventoryItem item)
        {
            return item.addToShop(p => p.isFurnitureCataogue());
        }

        /// <summary>Generates a method that adds this inventory to the wallpaper catalogue and adds it to <see cref="IDisplayEvents.MenuChanged"/>.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<MenuChangedEventArgs> addToWallpaperCatalogue(this InventoryItem item)
        {
            return item.addToShop(p => p.isWallpaperCatalogue());
        }

        /// <summary>Generates a method that adds this inventory to the hat shop and adds it to <see cref="IDisplayEvents.MenuChanged"/>.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<MenuChangedEventArgs> addToHatShop(this InventoryItem item)
        {
            return item.addToShop(p => p.isHatShop());
        }

        /* Logging */

        /// <summary>Generates a method that sends this text to the log.</summary>
        /// <returns>Returns method.</returns>
        public static Action toLogAction(this string text, LogLevel logLevel = LogLevel.Debug, IMonitor monitor = null)
        {
            if (monitor == null)
                monitor = Monitor;

            Action a = delegate ()
            {
                monitor.Log(text, logLevel);
            };

            return a;
        }

        /// <summary>Generates a method that sends the generated text to the log.</summary>
        /// <returns>Returns method.</returns>
        public static Action toLogAction(this Func<string> text, LogLevel logLevel = LogLevel.Debug, IMonitor monitor = null)
        {
            if (monitor == null)
                monitor = Monitor;

            Action a = delegate ()
            {
                monitor.Log(text.Invoke(), logLevel);
            };

            return a;
        }

        /// <summary>Wraps the the method so it sends a specified text to the log before or after execution.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<T> addLog<T>(this EventHandler<T> t, string text, bool before = false, LogLevel logLevel = LogLevel.Debug, IMonitor monitor = null)
        {
            EventHandler<T> d = delegate (object sender, T e)
            {
                Action log = text.toLogAction(logLevel, monitor);

                if (before)
                    log.Invoke();

                t.Invoke(sender, e);

                if (!before)
                    log.Invoke();
            };

            return d;
        }

        /// <summary>Wraps the the method so it sends a generated text to the log before or after execution.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<T> addLog<T>(this EventHandler<T> t, Func<T, string> text, bool before = false, LogLevel logLevel = LogLevel.Debug, IMonitor monitor = null)
        {
            EventHandler<T> d = delegate (object sender, T e)
            {
                Action log = text.Invoke(e).toLogAction(logLevel, monitor);

                if (before)
                    log.Invoke();

                t.Invoke(sender, e);

                if (!before)
                    log.Invoke();
            };

            return d;
        }

        /* Input */

        /// <summary>Wraps the the method so it only executes if a specific Key is pressed and adds it to <see cref="IInputEvents.ButtonPressed"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<ButtonPressedEventArgs> onPressed(this SButton k, EventHandler<ButtonPressedEventArgs> handler)
        {
            EventHandler<ButtonPressedEventArgs> d = delegate (object sender, ButtonPressedEventArgs e)
            {
                if (e.Button == k)
                    handler.Invoke(sender, e);
            };

            PyEvents.Events.Input.ButtonPressed += d;

            return d;
        }

        /// <summary>Wraps the the method so it only executes if a specific Key is pressed and adds it to <see cref="IInputEvents.ButtonPressed"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<ButtonPressedEventArgs> onPressed(this SButton k, Action action)
        {
            EventHandler<ButtonPressedEventArgs> d = delegate (object sender, ButtonPressedEventArgs e)
            {
                if (e.Button == k)
                    action.Invoke();
            };

            PyEvents.Events.Input.ButtonPressed += d;

            return d;
        }

        /// <summary>Wraps the the method so it only executes if the player clicks on the specified tileposition and adds it to <see cref="IInputEvents.ButtonPressed"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<ButtonPressedEventArgs> onClick(this ButtonClick t, TileLocationSelector tile, Action<Vector2> handler)
        {
            EventHandler<ButtonPressedEventArgs> d = delegate (object sender, ButtonPressedEventArgs e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.Button.IsUseToolButton() : e.Button.IsActionButton();
                if (isButton && Game1.currentLocation is GameLocation location && tile.predicate(location,location.getTileAtMousePosition()))
                    handler.Invoke(location.getTileAtMousePosition());
            };

            PyEvents.Events.Input.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if the player clicks on the specified on a position that matches the predicate adds it to <see cref="IInputEvents.ButtonPressed"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<ButtonPressedEventArgs> onClick(this ButtonClick t, Func<Point,bool> predicate, Action<Point> handler)
        {
            EventHandler<ButtonPressedEventArgs> d = delegate (object sender, ButtonPressedEventArgs e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.Button.IsUseToolButton() : e.Button.IsActionButton();
                Point mousePosition = Game1.getMousePosition();
                if (isButton && predicate.Invoke(mousePosition))
                    handler.Invoke(mousePosition);
            };

            PyEvents.Events.Input.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if the player clicks on the screen position and adds it to <see cref="IInputEvents.ButtonPressed"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<ButtonPressedEventArgs> onClick(this ButtonClick t, Point position, Action<Point> handler)
        {
            EventHandler<ButtonPressedEventArgs> d = delegate (object sender, ButtonPressedEventArgs e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.Button.IsUseToolButton() : e.Button.IsActionButton();
                Point mousePosition = Game1.getMousePosition();
                if (isButton && mousePosition == position)
                    handler.Invoke(position);
            };

            PyEvents.Events.Input.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if the player clicks on the screen area and adds it to <see cref="IInputEvents.ButtonPressed"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<ButtonPressedEventArgs> onClick(this ButtonClick t, Rectangle area, Action<Point> handler)
        {
            EventHandler<ButtonPressedEventArgs> d = delegate (object sender, ButtonPressedEventArgs e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.Button.IsUseToolButton() : e.Button.IsActionButton();
                Point mousePosition = Game1.getMousePosition();
                if (isButton && area.Contains(mousePosition))
                    handler.Invoke(mousePosition);
            };

            PyEvents.Events.Input.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if the player clicks on the specified tile and adds it to <see cref="IInputEvents.ButtonPressed"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<ButtonPressedEventArgs> onClick(this ButtonClick t, Vector2 tile, GameLocation location, Action<Vector2> handler)
        {
            EventHandler<ButtonPressedEventArgs> d = delegate (object sender, ButtonPressedEventArgs e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.Button.IsUseToolButton() : e.Button.IsActionButton();
                if (isButton && Game1.currentLocation == location && location.getTileAtMousePosition() == tile)
                    handler.Invoke(location.getTileAtMousePosition());
            };

            PyEvents.Events.Input.ButtonPressed += d;
            return d;
        }


        /// <summary>Wraps the the method so it only executes if an object of the requested type is clicked on and adds it to <see cref="IInputEvents.ButtonPressed"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<ButtonPressedEventArgs> onObjectClick<T>(this ButtonClick t, EventHandler<T> handler) where T : SObject
        {
            EventHandler<ButtonPressedEventArgs> d = delegate (object sender, ButtonPressedEventArgs e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.Button.IsUseToolButton() : e.Button.IsActionButton();
                if (isButton && Game1.currentLocation is GameLocation location && location.getTileAtMousePosition().sObjectOnMap<T>() is T obj)
                    handler.Invoke(sender, obj);
            };

            PyEvents.Events.Input.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if an object of the requested type is clicked on and adds it to <see cref="IInputEvents.ButtonPressed"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<ButtonPressedEventArgs> onObjectClick<T>(this ButtonClick t, Action<T> handler) where T : SObject
        {
            EventHandler<ButtonPressedEventArgs> d = delegate (object sender, ButtonPressedEventArgs e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.Button.IsUseToolButton() : e.Button.IsActionButton();
                if (isButton && Game1.currentLocation is GameLocation location && location.getTileAtMousePosition().sObjectOnMap<T>() is T obj)
                        handler.Invoke(obj);
            };

            PyEvents.Events.Input.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if an object of the requested type is clicked on and adds it to <see cref="IInputEvents.ButtonPressed"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<ButtonPressedEventArgs> onTerrainClick<T>(this ButtonClick t, Action<T> handler) where T : TerrainFeature
        {
            EventHandler<ButtonPressedEventArgs> d = delegate (object sender, ButtonPressedEventArgs e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.Button.IsUseToolButton() : e.Button.IsActionButton();
                if (isButton && Game1.currentLocation is GameLocation location && location.getTileAtMousePosition().terrainOnMap<T>() is T obj)
                    handler.Invoke(obj);
            };

            PyEvents.Events.Input.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if an object of the requested type is clicked on and adds it to <see cref="IInputEvents.ButtonPressed"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<ButtonPressedEventArgs> onTerrainClick<T>(this ButtonClick t, EventHandler<T> handler) where T : TerrainFeature
        {
            EventHandler<ButtonPressedEventArgs> d = delegate (object sender, ButtonPressedEventArgs e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.Button.IsUseToolButton() : e.Button.IsActionButton();
                if (isButton && Game1.currentLocation is GameLocation location && location.getTileAtMousePosition().terrainOnMap<T>() is T obj)
                    handler.Invoke(sender, obj);
            };

            PyEvents.Events.Input.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if an object of the requested type is clicked on and adds it to <see cref="IInputEvents.ButtonPressed"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<ButtonPressedEventArgs> onFurnitureClick<T>(this ButtonClick t, EventHandler<T> handler) where T : Furniture
        {
            EventHandler<ButtonPressedEventArgs> d = delegate (object sender, ButtonPressedEventArgs e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.Button.IsUseToolButton() : e.Button.IsActionButton();
                if (isButton && Game1.currentLocation is GameLocation location && location.getTileAtMousePosition().furnitureOnMap<T>() is T obj)
                    handler.Invoke(sender, obj);
            };

            PyEvents.Events.Input.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if an object of the requested type is clicked on and adds it to <see cref="IInputEvents.ButtonPressed"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<ButtonPressedEventArgs> onFurnitureClick<T>(this ButtonClick t, Action<T> handler) where T : Furniture
        {
            EventHandler<ButtonPressedEventArgs> d = delegate (object sender, ButtonPressedEventArgs e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.Button.IsUseToolButton() : e.Button.IsActionButton();
                if (isButton && Game1.currentLocation is GameLocation location && location.getTileAtMousePosition().furnitureOnMap<T>() is T obj)
                    handler.Invoke(obj);
            };

            PyEvents.Events.Input.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if this button is pressed and adds it to <see cref="IInputEvents.ButtonPressed"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<ButtonPressedEventArgs> onClick(this ButtonClick t, EventHandler<ButtonPressedEventArgs> handler)
        {
            EventHandler<ButtonPressedEventArgs> d = delegate (object sender, ButtonPressedEventArgs e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.Button.IsUseToolButton() : e.Button.IsActionButton();
                if (isButton)
                    handler.Invoke(sender, e);
            };

            PyEvents.Events.Input.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if this button is pressed and adds it to <see cref="IInputEvents.ButtonPressed"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<ButtonPressedEventArgs> onClick(this ButtonClick t, Action<ButtonPressedEventArgs> handler)
        {
            EventHandler<ButtonPressedEventArgs> d = delegate (object sender, ButtonPressedEventArgs e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.Button.IsUseToolButton() : e.Button.IsActionButton();
                if (isButton)
                    handler.Invoke(e);
            };

            PyEvents.Events.Input.ButtonPressed += d;
            return d;
        }


        /* Objects */

        /// <summary>Wraps the the method so it only executes if objects of the requested type are added to the players inventory and adds it to <see cref="IPlayerEvents.InventoryChanged"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<InventoryChangedEventArgs> whenAddedToInventory<TItem>(this ItemSelector<TItem> t, EventHandler<List<TItem>> handler) where TItem : Item
        {
            EventHandler<InventoryChangedEventArgs> d = delegate (object sender, InventoryChangedEventArgs e)
            {
                if (e.IsLocalPlayer)
                {
                    List<TItem> items = e.Added.OfType<TItem>().Where(p => t.predicate(p)).ToList();
                    if (items.Any())
                        handler.Invoke(sender, items);
                }
            };

            PyEvents.Events.Player.InventoryChanged += d;

            return d;
        }

        /// <summary>Wraps the the method so it only executes if objects of the requested type are added to the players inventory and adds it to <see cref="IPlayerEvents.InventoryChanged"/>.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<InventoryChangedEventArgs> whenAddedToInventory<TItem>(this ItemSelector<TItem> t, Action<List<TItem>> action) where TItem : Item
        {
            EventHandler<InventoryChangedEventArgs> d = delegate (object sender, InventoryChangedEventArgs e)
            {
                if (e.IsLocalPlayer)
                {
                    List<TItem> items = e.Added.OfType<TItem>().Where(p => t.predicate(p)).ToList();
                    if (items.Any())
                        action.Invoke(items);
                }
            };

            PyEvents.Events.Player.InventoryChanged += d;

            return d;
        }

        /*
        public static Dictionary<GameLocation, NotifyCollectionChangedEventHandler> whenAddedToLocation<T>(this ItemSelector<T> t, Action<GameLocation,List<Vector2>> action, GameLocation l = null) where T : SObject
        {
            List<GameLocation> gls = new List<GameLocation>();
            if (l == null)
                gls = PyUtils.getAllLocationsAndBuidlings();
            else
                gls.Add(l);

            Action<GameLocation, NotifyCollectionChangedEventArgs> a = delegate (GameLocation location, NotifyCollectionChangedEventArgs e)
             {
                 List<Vector2> keys = new List<Vector2>();

                 if (e.Action == NotifyCollectionChangedAction.Add)
                     foreach (Vector2 key in e.NewItems)
                         if (t.predicate(location.objects[key]))
                             keys.Add(key);
  
                 action.Invoke(location, keys);
             };

            Dictionary<GameLocation, NotifyCollectionChangedEventHandler> d = new Dictionary<GameLocation, NotifyCollectionChangedEventHandler>();

            foreach (GameLocation gl in gls)
                d.AddOrReplace(gl, (o, e) => a(gl, e));

            return d.useAll(p => p.Key.objects.CollectionChanged += p.Value);
        }*/

        /*
        public static Dictionary<GameLocation, NotifyCollectionChangedEventHandler> whenAddedToLocation<T>(this TerrainSelector<T> t, Action<GameLocation, List<Vector2>> action, GameLocation l = null) where T : TerrainFeature
        {
            List<GameLocation> gls = new List<GameLocation>();
            if (l == null)
                gls = PyUtils.getAllLocationsAndBuidlings();
            else
                gls.Add(l);

            Action<GameLocation, NotifyCollectionChangedEventArgs> a = delegate (GameLocation location, NotifyCollectionChangedEventArgs e)
            {
                List<Vector2> keys = new List<Vector2>();

                if (e.Action == NotifyCollectionChangedAction.Add)
                    foreach (Vector2 key in e.NewItems)
                        if (location.terrainFeatures.ContainsKey(key) && t.predicate(location.terrainFeatures[key]))
                            keys.Add(key);

                action.Invoke(location, keys);
            };

            Dictionary<GameLocation, NotifyCollectionChangedEventHandler> d = new Dictionary<GameLocation, NotifyCollectionChangedEventHandler>();

            foreach (GameLocation gl in gls)
                d.AddOrReplace(gl, (o, e) => a(gl, e));

            return d.useAll(p => p.Key.terrainFeatures.CollectionChanged += p.Value);
        } */
        /*
        public static Dictionary<GameLocation, NotifyCollectionChangedEventHandler> whenRemovedFromLocation(this TileLocationSelector t, Action<GameLocation, List<Vector2>> action)
        {
            List<GameLocation> gls = new List<GameLocation>();
            if (t.location == null)
                gls = PyUtils.getAllLocationsAndBuidlings();
            else
                gls.Add(t.location);

            Action<GameLocation, NotifyCollectionChangedEventArgs> a = delegate (GameLocation location, NotifyCollectionChangedEventArgs e)
            {
                List<Vector2> keys = new List<Vector2>();

                if (e.Action == NotifyCollectionChangedAction.Remove)
                    foreach (Vector2 key in e.OldItems)
                        if (t.predicate(location, key))
                            keys.Add(key);

                action.Invoke(location, keys);
            };

            Dictionary<GameLocation, NotifyCollectionChangedEventHandler> d = new Dictionary<GameLocation, NotifyCollectionChangedEventHandler>();

            foreach (GameLocation gl in gls)
                d.AddOrReplace(gl, (o, e) => a(gl, e));

            return d.useAll(p => p.Key.terrainFeatures.CollectionChanged += p.Value);
        }*/

        /* Locations */
        
        public static EventHandler<WarpedEventArgs> onEntry(this GameLocation location, EventHandler<WarpedEventArgs> handler)
        {
            EventHandler<WarpedEventArgs> d = delegate (object sender, WarpedEventArgs e)
            {
                if (e.IsLocalPlayer && e.NewLocation == location)
                    handler.Invoke(sender, e);
            };

            PyEvents.Events.Player.Warped += d;

            return d;
        }

        public static EventHandler<WarpedEventArgs> onExit(this GameLocation t, EventHandler<WarpedEventArgs> handler)
        {
            EventHandler<WarpedEventArgs> d = delegate (object sender, WarpedEventArgs e)
            {
                if (e.IsLocalPlayer && e.OldLocation == t)
                    handler.Invoke(sender, e);
            };

            PyEvents.Events.Player.Warped += d;

            return d;
        }

    }
}
