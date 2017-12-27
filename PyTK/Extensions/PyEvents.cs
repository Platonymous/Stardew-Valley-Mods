using StardewModdingAPI;
using StardewModdingAPI.Events;
using SObject = StardewValley.Object;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;
using StardewValley;
using PyTK.Types;
using System.Collections.Generic;
using System.Linq;

namespace PyTK.Extensions
{
    public static class PyEvents
    {
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
        public static Action<TArgs> addPredicate<TArgs>(this Action<TArgs> t, Func<TArgs, bool> p)
        {
            Action<TArgs> d = delegate (TArgs e)
            {
                if (p.Invoke(e))
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

        /* Menu */

        /// <summary>Wraps the the method so it only executes if a menu of the requested type opens and adds it to MenuEvents.MenuChanged.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsClickableMenuChanged> onActivation<T>(this IClickableMenu t, EventHandler<EventArgsClickableMenuChanged> handler) where T : IClickableMenu
        {
            EventHandler<EventArgsClickableMenuChanged> d = delegate (object sender, EventArgsClickableMenuChanged e)
            {
                if (e.NewMenu is T)
                    handler.Invoke(sender, e);
            };

            MenuEvents.MenuChanged += d;

            return d;
        }

        /// <summary>Wraps the the method so it only executes if a menu of the requested type opens and adds it to MenuEvents.MenuChanged.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsClickableMenuChanged> onActivation<T>(this IClickableMenu t, Action<T> action) where T : IClickableMenu
        {
            EventHandler<EventArgsClickableMenuChanged> d = delegate (object sender, EventArgsClickableMenuChanged e)
            {
                if (e.NewMenu is T)
                    action.Invoke(e.NewMenu as T);
            };

            MenuEvents.MenuChanged += d;

            return d;
        }

        /// <summary>Wraps the the method so it only executes if a menu of the requested type closes and adds it to MenuEvents.MenuClosed.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsClickableMenuClosed> onClose<T>(this IClickableMenu t, EventHandler<EventArgsClickableMenuClosed> handler) where T : IClickableMenu
        {
            EventHandler<EventArgsClickableMenuClosed> d = delegate (object sender, EventArgsClickableMenuClosed e)
            {
                if (e.PriorMenu is T)
                    handler.Invoke(sender, e);
            };

            MenuEvents.MenuClosed += d;

            return d;
        }

        /// <summary>Wraps the the method so it only executes if a menu of the requested type closes and adds it to MenuEvents.MenuClosed.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsClickableMenuClosed> onClose<T>(this IClickableMenu t, Action<T> action) where T : IClickableMenu
        {
            EventHandler<EventArgsClickableMenuClosed> d = delegate (object sender, EventArgsClickableMenuClosed e)
            {
                if (e.PriorMenu is T)
                    action.Invoke(e.PriorMenu as T);
            };

            return d;
        }

        /// <summary>Generates a method that adds this inventory to a shop that matches the defined conditions and adds it to MenuEvents.MenuChanged.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<EventArgsClickableMenuChanged> addToShop(this List<InventoryItem> inventory, Func<ShopMenu, bool> predicate)
        {
            EventHandler<EventArgsClickableMenuChanged> d = delegate (object sender, EventArgsClickableMenuChanged e)
            {
                ShopMenu shop = (ShopMenu)e.NewMenu;
                List<Item> forSale = shop.getForSale();
                Dictionary<Item, int[]> priceAndStock = shop.getItemPriceAndStock();
                forSale = forSale.Union(inventory.forSale()).ToList();
                priceAndStock = priceAndStock.Union(inventory.priceAndStock()).ToDictionary(dict => dict.Key, dict => dict.Value);
            };

            d = d.addPredicate(e => predicate.Invoke((e.NewMenu as ShopMenu)));

            return Game1.activeClickableMenu.onActivation<ShopMenu>(d);
        }

        /// <summary>Generates a method that adds this inventory to a shop of an NPC adds it to MenuEvents.MenuChanged.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<EventArgsClickableMenuChanged> addToNPCShop(this List<InventoryItem> items, string shopkeeper)
        {
            return items.addToShop((shop) => shop.portraitPerson.name == shopkeeper);
        }

        /// <summary>Generates a method that adds this inventory to the furniture catalogue and adds it to MenuEvents.MenuChanged.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<EventArgsClickableMenuChanged> addToFurnitureCatalogue(this List<InventoryItem> items)
        {
            return items.addToShop(p => p.isFurnitureCataogue());
        }

        /// <summary>Generates a method that adds this inventory to the hat shop and adds it to MenuEvents.MenuChanged.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<EventArgsClickableMenuChanged> addToHatShop(this List<InventoryItem> items)
        {
            return items.addToShop(p => p.isHatShop());
        }

        //---

        /// <summary>Generates a method that adds this inventory to a shop that matches the defined conditions and adds it to MenuEvents.MenuChanged.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<EventArgsClickableMenuChanged> addToShop(this InventoryItem inventory, Func<ShopMenu, bool> predicate)
        {
            EventHandler<EventArgsClickableMenuChanged> d = delegate (object sender, EventArgsClickableMenuChanged e)
            {
                ShopMenu shop = (ShopMenu)e.NewMenu;
                List<Item> forSale = shop.getForSale();
                Dictionary<Item, int[]> priceAndStock = shop.getItemPriceAndStock();
                forSale.Add(inventory.item);
                priceAndStock.AddOrReplace(inventory.item, new int[] { inventory.price, inventory.stock });
            };

            d = d.addPredicate(e => predicate.Invoke((e.NewMenu as ShopMenu)));

            return Game1.activeClickableMenu.onActivation<ShopMenu>(d);
        }

        /// <summary>Generates a method that adds this inventory to a shop of an NPC adds it to MenuEvents.MenuChanged.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<EventArgsClickableMenuChanged> addToNPCShop(this InventoryItem item, string shopkeeper)
        {
            return item.addToShop((shop) => shop.portraitPerson.name == shopkeeper);
        }

        /// <summary>Generates a method that adds this inventory to the furniture catalogue and adds it to MenuEvents.MenuChanged.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<EventArgsClickableMenuChanged> addToFurnitureCatalogue(this InventoryItem item)
        {
            return item.addToShop(p => p.isFurnitureCataogue());
        }

        /// <summary>Generates a method that adds this inventory to the hat shop and adds it to MenuEvents.MenuChanged.</summary>
        /// <returns>Returns the method.</returns>
        public static EventHandler<EventArgsClickableMenuChanged> addToHatShop(this InventoryItem item)
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

        /// <summary>Wraps the the method so it only executes if a specific Key is pressed and adds it to ControlEvents.KeyPressed.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsKeyPressed> onPressed(this Keys k, EventHandler<EventArgsKeyPressed> handler)
        {
            EventHandler<EventArgsKeyPressed> d = delegate (object sender, EventArgsKeyPressed e)
            {
                if (e.KeyPressed == k)
                    handler.Invoke(sender, e);
            };

            ControlEvents.KeyPressed += d;

            return d;
        }

        /// <summary>Wraps the the method so it only executes if a specific Key is pressed and adds it to ControlEvents.KeyPressed.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsKeyPressed> onPressed(this Keys k, Action action)
        {
            EventHandler<EventArgsKeyPressed> d = delegate (object sender, EventArgsKeyPressed e)
            {
                if (e.KeyPressed == k)
                    action.Invoke();
            };

            ControlEvents.KeyPressed += d;

            return d;
        }

        /// <summary>Wraps the the method so it only executes if an object of the requested type is clicked on and adds it to InputEvents.ButtonPressed.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsInput> onObjectClick<T>(this ButtonClick t, EventHandler<T> handler) where T : SObject
        {
            EventHandler<EventArgsInput> d = delegate (object sender, EventArgsInput e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.IsUseToolButton : e.IsActionButton;
                if (isButton && Game1.currentLocation is GameLocation location && location.getTileAtMousePosition().sObjectOnMap<T>() is T obj)
                    handler.Invoke(sender, obj);
            };

            InputEvents.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if an object of the requested type is clicked on and adds it to InputEvents.ButtonPressed.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsInput> onObjectClick<T>(this ButtonClick t, Action<T> handler) where T : SObject
        {
            EventHandler<EventArgsInput> d = delegate (object sender, EventArgsInput e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.IsUseToolButton : e.IsActionButton;
                if (isButton && Game1.currentLocation is GameLocation location && location.getTileAtMousePosition().sObjectOnMap<T>() is T obj)
                        handler.Invoke(obj);
            };

            InputEvents.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if an object of the requested type is clicked on and adds it to InputEvents.ButtonPressed.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsInput> onTerrainClick<T>(this ButtonClick t, Action<T> handler) where T : TerrainFeature
        {
            EventHandler<EventArgsInput> d = delegate (object sender, EventArgsInput e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.IsUseToolButton : e.IsActionButton;
                if (isButton && Game1.currentLocation is GameLocation location && location.getTileAtMousePosition().terrainOnMap<T>() is T obj)
                    handler.Invoke(obj);
            };

            InputEvents.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if an object of the requested type is clicked on and adds it to InputEvents.ButtonPressed.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsInput> onTerrainClick<T>(this ButtonClick t, EventHandler<T> handler) where T : TerrainFeature
        {
            EventHandler<EventArgsInput> d = delegate (object sender, EventArgsInput e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.IsUseToolButton : e.IsActionButton;
                if (isButton && Game1.currentLocation is GameLocation location && location.getTileAtMousePosition().terrainOnMap<T>() is T obj)
                    handler.Invoke(sender, obj);
            };

            InputEvents.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if an object of the requested type is clicked on and adds it to InputEvents.ButtonPressed.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsInput> onFurnitureClick<T>(this ButtonClick t, EventHandler<T> handler) where T : Furniture
        {
            EventHandler<EventArgsInput> d = delegate (object sender, EventArgsInput e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.IsUseToolButton : e.IsActionButton;
                if (isButton && Game1.currentLocation is GameLocation location && location.getTileAtMousePosition().furnitureOnMap<T>() is T obj)
                    handler.Invoke(sender, obj);
            };

            InputEvents.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if an object of the requested type is clicked on and adds it to InputEvents.ButtonPressed.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsInput> onFurnitureClick<T>(this ButtonClick t, Action<T> handler) where T : Furniture
        {
            EventHandler<EventArgsInput> d = delegate (object sender, EventArgsInput e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.IsUseToolButton : e.IsActionButton;
                if (isButton && Game1.currentLocation is GameLocation location && location.getTileAtMousePosition().furnitureOnMap<T>() is T obj)
                    handler.Invoke(obj);
            };

            InputEvents.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if this button is pressed and adds it to InputEvents.ButtonPressed.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsInput> onClick(this ButtonClick t, EventHandler<EventArgsInput> handler)
        {
            EventHandler<EventArgsInput> d = delegate (object sender, EventArgsInput e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.IsUseToolButton : e.IsActionButton;
                if (isButton)
                    handler.Invoke(sender, e);
            };

            InputEvents.ButtonPressed += d;
            return d;
        }

        /// <summary>Wraps the the method so it only executes if this button is pressed and adds it to InputEvents.ButtonPressed.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsInput> onClick(this ButtonClick t, Action<EventArgsInput> handler)
        {
            EventHandler<EventArgsInput> d = delegate (object sender, EventArgsInput e)
            {
                bool isButton = t.Equals(ButtonClick.UseToolButton) ? e.IsUseToolButton : e.IsActionButton;
                if (isButton)
                    handler.Invoke(e);
            };

            InputEvents.ButtonPressed += d;
            return d;
        }


        /* Objects */

        /// <summary>Wraps the the method so it only executes if objects of the requested type are added to the players inventory and adds it to PlayerEvents.InventoryChanged.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsInventoryChanged> whenAddedToInventory<T>(this ItemSelector<T> t, EventHandler<List<T>> handler) where T : Item
        {
            EventHandler<EventArgsInventoryChanged> d = delegate (object sender, EventArgsInventoryChanged e)
            {
                if (e.Added.Exists(p => p.Item is T && t.predicate(p.Item as T)))
                    handler.Invoke(sender, e.Added.FindAll(p => p.Item is T && t.predicate(p.Item as T)).ConvertAll(p => p.Item as T));
            };

            PlayerEvents.InventoryChanged += d;

            return d;
        }

        /// <summary>Wraps the the method so it only executes if objects of the requested type are added to the players inventory and adds it to PlayerEvents.InventoryChanged.</summary>
        /// <returns>Returns the wrapped method.</returns>
        public static EventHandler<EventArgsInventoryChanged> whenAddedToInventory<T>(this ItemSelector<T> t, Action<List<T>> action) where T : Item
        {
            EventHandler<EventArgsInventoryChanged> d = delegate (object sender, EventArgsInventoryChanged e)
            {
                if (e.Added.Exists(p => p.Item is T && t.predicate(p.Item as T)))
                    action.Invoke(e.Added.FindAll(p => p.Item is T && t.predicate(p.Item as T)).ConvertAll(p => p.Item as T));
            };

            PlayerEvents.InventoryChanged += d;

            return d;
        }    
    }
}
