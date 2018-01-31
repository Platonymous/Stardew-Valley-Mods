using System;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using Visualize;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;
using System.Collections.Generic;

namespace Capitalism.Components.CapitalismMoneyDialPatch
{
    internal class CapitalismMoneyDial
    {
        internal static Texture2D pointTex;
        internal static int drawCounter = -1;
        internal static bool showZero = false;
        internal static int currentValue = 0;
        internal static decimal targetValue = 0;
        internal static bool maxed = false;

        internal static void onEntry()
        {
            pointTex = CapitalismMod._helper.Content.Load<Texture2D>("point.png");
        }
    }

    [HarmonyPatch]
    internal class MoneyDialFix
    {
        internal static MethodInfo TargetMethod()
        {
            if (Type.GetType("StardewValley.BellsAndWhistles.MoneyDial, Stardew Valley") != null)
                return AccessTools.Method(Type.GetType("StardewValley.BellsAndWhistles.MoneyDial, Stardew Valley"), "draw");
            else
                return AccessTools.Method(Type.GetType("StardewValley.BellsAndWhistles.MoneyDial, StardewValley"), "draw");
        }

        internal static void Prefix(ref MoneyDial __instance, ref int target)
        {
            CapitalismMoneyDial.drawCounter = 0;
            CapitalismMoneyDial.currentValue = __instance.currentValue;
            CapitalismMoneyDial.targetValue = Convert.ToDecimal(target);

            int maxDigits = 8;
            if (Game1.activeClickableMenu is ShippingMenu)
                maxDigits = 6;

            int maxValue = (int) Math.Pow(10, maxDigits - 1) - 1;

            if (target > maxValue)
            {
                CapitalismMoneyDial.maxed = true;
                target /= 1000;
            }
               
            if (target > maxValue)
                target = maxValue;

            if (target < 10)
                target *= 10;

            if (target < 100)
                target *= 10;

            target *= 10;
            

                  
        }

        internal static void Postfix(ref MoneyDial __instance, ref int target)
        {
            CapitalismMoneyDial.maxed = false;
            CapitalismMoneyDial.drawCounter = -1;
        }
    }

    internal class MoneyDialVisualizeHandler : IVisualizeHandler
    {

        public bool Draw(ref SpriteBatch __instance, ref Texture2D texture, ref Vector4 destination, ref bool scaleDestination, ref Rectangle? sourceRectangle, ref Color color, ref float rotation, ref Vector2 origin, ref SpriteEffects effects, ref float depth)
        {
            if (texture == Game1.mouseCursors && sourceRectangle.Value is Rectangle r && r.X == 286 && CapitalismMoneyDial.drawCounter >= 0)
            {
                if (CapitalismMoneyDial.maxed)
                    color = Color.Purple;

                int index = CapitalismMoneyDial.drawCounter;

                int revert = 1000;

                if (CapitalismMoneyDial.targetValue < 10)
                    revert *= 10;

                if (CapitalismMoneyDial.targetValue < 100)
                    revert *= 10;

                decimal cash = Convert.ToDecimal(CapitalismMoneyDial.currentValue) / revert;           

                string money = string.Format("{0:0.00}", cash) ;

                if (money.Length <= 0 || index < 0 || index >= money.Length)
                    return true;

                int num = -1;

                if (money[index] != '.')
                    int.TryParse(money[index].ToString(), out num);


                if ((num == 0 && !CapitalismMoneyDial.showZero))
                {
                    CapitalismMoneyDial.drawCounter++;
                    return false;
                }
                else
                    CapitalismMoneyDial.showZero = true;

                r.Y = num < 0 ? 0 : 502 - num * 8;
                r.X = num < 0 ? 0 : r.X;

                texture = num < 0 ? CapitalismMoneyDial.pointTex : texture;

                sourceRectangle = new Rectangle?(r);
                CapitalismMoneyDial.drawCounter++;
            }

            return true;
        }
    }


}
