using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RingOfFire
{
    public class RingOfFireMod : Mod
    {
        private ROFConfig config;
        private Random rnd;

        public static IModHelper helper;

        public override void Entry(IModHelper help)
        {
            helper = help;
            config = Helper.ReadConfig<ROFConfig>();
            rnd = new Random();
            List<Texture2D> flameTextures = new List<Texture2D>();
            flameTextures.Add(Helper.Content.Load<Texture2D>("assets/fire0.png"));
            flameTextures.Add(Helper.Content.Load<Texture2D>("assets/fire1.png"));
            flameTextures.Add(Helper.Content.Load<Texture2D>("assets/fire2.png"));
            flameTextures.Add(Helper.Content.Load<Texture2D>("assets/fire3.png"));

            RingOfFire.flameTextures = flameTextures;
            RingOfFire.ringTexture = Helper.Content.Load<Texture2D>("assets/ring.png");

            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.Display.Rendered += OnRendered;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Input.ButtonReleased += OnButtonReleased;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ShopMenu)
            {
                ShopMenu shop = (ShopMenu)Game1.activeClickableMenu;
                Dictionary<Item, int[]> items = Helper.Reflection.GetField<Dictionary<Item, int[]>>(shop, "itemPriceAndStock").GetValue();
                List<Item> selling = Helper.Reflection.GetField<List<Item>>(shop, "forSale").GetValue();

                if (shop.portraitPerson == Game1.getCharacterFromName("Marlon") )
                {
                    Dictionary<Item, int> newItemsToSell = new Dictionary<Item, int>();

                    newItemsToSell.Add(new RingOfFire(), config.price);

                    foreach (Item item in newItemsToSell.Keys)
                    {
                        items.Add(item, new int[] { newItemsToSell[item], int.MaxValue });
                        selling.Add(item);
                    }
                }
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {

            if (e.Button == config.actionKey && (Game1.player.leftRing is RingOfFire || Game1.player.rightRing is RingOfFire))
            {
                RingOfFire.active = true;
            }

            if(e.Button == SButton.N)
            {
                Game1.player.addItemByMenuIfNecessary(new RingOfFire());
            }
        }

        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button == config.actionKey)
            {
                RingOfFire.active = false;
                StardewValley.Farmer f = Game1.player;
                f.stopJittering();
            }
        }

        private void OnRendered(object sender, RenderedEventArgs e)
        {

            RingOfFire ring = null;
            if(Game1.player.leftRing is RingOfFire lr)
            {
                ring = lr;
            }

            if (Game1.player.rightRing is RingOfFire rr)
            {
                ring = rr;
            }

            if(ring != null)
            {
                ring.drawFlames();
            }
        }



        

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            RingOfFire ring = null;

            StardewValley.Farmer f = Game1.player;

            if (RingOfFire.active && f.health <= 5)
            {
                RingOfFire.active = false;
                f.stopJittering();
            }

            if (RingOfFire.active && rnd.NextDouble() < 0.03)
            {
                f.health--;
            }


            if (f.leftRing is RingOfFire lr)
            {
                ring = lr;
            }

            if (f.rightRing is RingOfFire rr)
            {
                ring = rr;
            }

            

            if (ring != null)
            {
               
                ring.update();
            }
        }

       
       
    }
}
