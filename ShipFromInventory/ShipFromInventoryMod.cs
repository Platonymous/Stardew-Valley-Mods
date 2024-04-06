using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using System;
using System.Threading;

namespace ShipFromInventory
{
    public class Config
    {
        public bool LidAnimation { get; set; } = true;
        public bool LidSound { get; set; } = true;

        public SButton ShortcutKey { get; set; } = SButton.Add;
    }

    public class ShipFromInventoryMod : Mod
    {
        internal static ClickableTextureComponent shippingBin;
        internal static ClickableTextureComponent shippingBinLid;
        internal static Texture2D shippingBinTexture;
        internal static Rectangle shippingBinLidRectangle;
        internal static Config config;
        const int rate = 2;
        const int max = 12;
        internal static int frame = 0;
        internal static bool closing = false;

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<Config>();

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            shippingBinTexture = helper.GameContent.Load<Texture2D>("Buildings/Shipping Bin");
            shippingBinLidRectangle = new Rectangle(134, 226, 30, 25);
            var instance = new Harmony("Platonymous.ShipFromInventory");
            instance.Patch(typeof(InventoryPage).GetConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int) }), null, new HarmonyMethod(this.GetType().GetMethod("InventoryPageCon")));
            instance.Patch(typeof(InventoryPage).GetMethod("draw", new[] { typeof(SpriteBatch) }), null, new HarmonyMethod(this.GetType().GetMethod("InventoryPageDraw")));
            if (Type.GetType("BiggerBackpack.NewInventoryPage, BiggerBackpack") is Type bbpType)
                instance.Patch(bbpType.GetMethod("draw", new[] { typeof(SpriteBatch) }), null, new HarmonyMethod(this.GetType().GetMethod("InventoryPageDraw")));


            if (config.LidAnimation)
                instance.Patch(typeof(InventoryPage).GetMethod("performHoverAction"), null, new HarmonyMethod(this.GetType().GetMethod("InventoryPageHover")));

            instance.Patch(typeof(InventoryPage).GetMethod("receiveLeftClick"), new HarmonyMethod(this.GetType().GetMethod("InventoryPageLeftClick")));

            if (config.LidAnimation)
                helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

        }


        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            shippingBinTexture = Helper.GameContent.Load<Texture2D>("Buildings/Shipping Bin");
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if ((e.Button == config.ShortcutKey || (config.ShortcutKey == SButton.Add && e.Button == SButton.OemPlus) || (config.ShortcutKey == SButton.OemPlus && e.Button == SButton.Add)) && Game1.activeClickableMenu is GameMenu && Game1.player.CursorSlotItem is StardewValley.Object obj && obj.canBeShipped())
                ShipObject(obj);
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (frame > 0 && e.IsMultipleOf(rate))
                frame = Math.Min(frame + (closing ? -1 : 1), max);
        }

        public static void InventoryPageCon(InventoryPage __instance, int x, int y, int width, int height)
        {
            ClickableTextureComponent textureComponent1 = new ClickableTextureComponent(new Rectangle(__instance.xPositionOnScreen + width / 3 + 576 + 32, __instance.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192 + 64 + 130, shippingBinTexture.Width * 3, shippingBinTexture.Height * 3), shippingBinTexture, new Rectangle(0, 0, shippingBinTexture.Width, shippingBinTexture.Height), 3f, false);
            textureComponent1.myID = 201;
            textureComponent1.upNeighborID = 202;
            textureComponent1.leftNeighborID = 203;
            shippingBin = textureComponent1;
            ClickableTextureComponent textureComponent2 = new ClickableTextureComponent(new Rectangle(__instance.xPositionOnScreen + width / 3 + 576 + 32 + 4, __instance.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192 + 64 + 130 - ((shippingBinLidRectangle.Height * 3) / 4), shippingBinLidRectangle.Width * 3, shippingBinLidRectangle.Height * 3), Game1.mouseCursors, shippingBinLidRectangle, 3f, false);
            textureComponent2.myID = 201;
            textureComponent2.upNeighborID = 202;
            textureComponent2.leftNeighborID = 203;
            shippingBinLid = textureComponent2;
        }

        public static void InventoryPageDraw(SpriteBatch b)
        {
            shippingBin.draw(b);
            shippingBinLid.draw(b);

            if (Game1.player.CursorSlotItem != null && shippingBin.bounds.Intersects(new Rectangle(Game1.getOldMouseX(), Game1.getOldMouseY(), 80, 80)))
                Game1.player.CursorSlotItem?.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 16), (float)(Game1.getOldMouseY() + 16)), 1f);
        }

        public static void InventoryPageHover(InventoryPage __instance, int x, int y)
        {
            if (shippingBin.containsPoint(x, y) && Game1.player.CursorSlotItem is StardewValley.Object obj && obj.canBeShipped())
            {
                closing = false;
                if (frame == 0 && config.LidSound)
                    Game1.playSound("doorCreak");

                frame = Math.Max(frame, 1);
            }
            else
            {
                if (!closing && frame > 0 && config.LidSound)
                    Game1.playSound("doorCreakReverse");

                closing = frame > 0;
            }

            shippingBinLid.sourceRect.X = 134 + (frame * shippingBinLidRectangle.Width);
        }

        public static bool InventoryPageLeftClick(InventoryPage __instance, int x, int y, bool playSound = true)
        {
            if ((shippingBin.containsPoint(x, y) || shippingBinLid.containsPoint(x, y)) && Game1.player.CursorSlotItem is StardewValley.Object obj && obj.canBeShipped())
                return ShipObject(obj);

            return true;
        }

        public static bool ShipObject(StardewValley.Object obj)
        {
            StardewValley.Object shipment = obj;
            Farm farm = Game1.getFarm();
            farm.getShippingBin(Game1.player).Add(shipment);
            farm.lastItemShipped = shipment;
            Game1.playSound("Ship");
            if (obj == Game1.player.CursorSlotItem)
                Game1.player.CursorSlotItem = null;
            else if (Game1.player.Items.Contains(obj))
                Game1.player.Items.Remove(obj);
            return false;
        }
    }
}
