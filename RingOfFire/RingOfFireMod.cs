using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            ControlEvents.KeyReleased += ControlEvents_KeyReleased;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            GameEvents.UpdateTick += GameEvents_UpdateTick;
            GraphicsEvents.OnPostRenderEvent += GraphicsEvents_OnPostRenderEvent;
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (Game1.activeClickableMenu is ShopMenu)
            {
                ShopMenu shop = (ShopMenu)Game1.activeClickableMenu;
                Dictionary<Item, int[]> items = Helper.Reflection.GetPrivateValue<Dictionary<Item, int[]>>(shop, "itemPriceAndStock");
                List<Item> selling = Helper.Reflection.GetPrivateValue<List<Item>>(shop, "forSale");

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

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {

            if (e.KeyPressed == config.actionKey && (Game1.player.leftRing is RingOfFire || Game1.player.rightRing is RingOfFire))
            {
                RingOfFire.active = true;
            }

            if(e.KeyPressed == Keys.N)
            {
                Game1.player.addItemByMenuIfNecessary(new RingOfFire());
            }

        }
        

        private void ControlEvents_KeyReleased(object sender, EventArgsKeyPressed e)
        {

            if (e.KeyPressed == config.actionKey)
            {
                RingOfFire.active = false;
                StardewValley.Farmer f = Game1.player;
                f.stopJittering();
            }
            
        }

        private void GraphicsEvents_OnPostRenderEvent(object sender, System.EventArgs e)
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



        

        private void GameEvents_UpdateTick(object sender, System.EventArgs e)
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
