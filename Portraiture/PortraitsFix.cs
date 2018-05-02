using System;
using System.Reflection;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace Portraiture
{

    internal class FixHelper
    {
        public static Type getTypeFullSDV(string type)
        {
            Type defaulSDV = Type.GetType(type + ", Stardew Valley");

            if (defaulSDV != null)
                return defaulSDV;
            else
                return Type.GetType(type + ", StardewValley");

        }
    }

    [HarmonyPatch]
    internal class DialogueBoxFix
    {
        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(FixHelper.getTypeFullSDV("StardewValley.Menus.DialogueBox"), "drawPortrait");
        }

        internal static bool Prefix(DialogueBox __instance, SpriteBatch b)
        {
            if (__instance.width < 642)
                return false;

            int x = PortraitureMod.helper.Reflection.GetField<int>(__instance, "x").GetValue();
            int y = PortraitureMod.helper.Reflection.GetField<int>(__instance, "y").GetValue();
            Dialogue characterDialogue = PortraitureMod.helper.Reflection.GetField<Dialogue>(__instance, "characterDialogue").GetValue();
            Rectangle friendshipJewel = PortraitureMod.helper.Reflection.GetField<Rectangle>(__instance, "friendshipJewel").GetValue();
            Texture2D texture = TextureLoader.getPortrait(characterDialogue.speaker.Name);
            if (texture == null)
                texture = characterDialogue.speaker.Portrait;

            Rectangle rectangle = TextureLoader.getSoureRectangle(texture, characterDialogue.getPortraitIndex());

            int num1 = x + __instance.width - 448 + 4;
            int num2 = x + __instance.width - num1;
            b.Draw(Game1.mouseCursors, new Rectangle(num1 - 40, y, 36, __instance.height), new Rectangle?(new Rectangle(278, 324, 9, 1)), Color.White);
            b.Draw(Game1.mouseCursors, new Vector2((float)(num1 - 40), (float)(y - 20)), new Rectangle?(new Rectangle(278, 313, 10, 7)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            b.Draw(Game1.mouseCursors, new Vector2((float)(num1 - 40), (float)(y + __instance.height)), new Rectangle?(new Rectangle(278, 328, 10, 8)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            int num3 = num1 + 76;
            int num4 = y + __instance.height / 2 - 148 - 36;
            b.Draw(Game1.mouseCursors, new Vector2((float)(num1 - 8), (float)y), new Rectangle?(new Rectangle(583, 411, 115, 97)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);

            if (!characterDialogue.speaker.Portrait.Bounds.Contains(rectangle))
                rectangle = TextureLoader.getSoureRectangle(texture, 0);

            int num5 = shouldPortraitShake(characterDialogue, __instance) ? Game1.random.Next(-1, 2) : 0;
            b.Draw(texture, new Rectangle(num3 + 16 + num5, num4 + 24, 64 * Game1.pixelZoom, 64 * Game1.pixelZoom), new Rectangle?(rectangle), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.88f);


            SpriteText.drawStringHorizontallyCenteredAt(b, characterDialogue.speaker.getName(), num1 + num2 / 2, num4 + 296 + 16, 999999, -1, 999999, 1f, 0.88f, false, -1);
            if (Game1.eventUp || friendshipJewel.Equals(Rectangle.Empty) || (characterDialogue == null || characterDialogue.speaker == null) || !Game1.player.friendshipData.ContainsKey(characterDialogue.speaker.Name))
                return false;

            b.Draw(Game1.mouseCursors, new Vector2((float)friendshipJewel.X, (float)friendshipJewel.Y), new Rectangle?(Game1.player.getFriendshipHeartLevelForNPC(characterDialogue.speaker.Name) >= 10 ? new Rectangle(269, 494, 11, 11) : new Rectangle(Math.Max(140, 140 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 / 250.0) * 11), Math.Max(532, 532 + Game1.player.getFriendshipHeartLevelForNPC(characterDialogue.speaker.Name) / 2 * 11), 11, 11)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            return false;

        }

        internal static bool shouldPortraitShake(Dialogue d, DialogueBox box)
        {
            int portraitIndex = d.getPortraitIndex();
            if (d.speaker.Name.Equals("Pam") && portraitIndex == 3 || d.speaker.Name.Equals("Abigail") && portraitIndex == 7 || (d.speaker.Name.Equals("Haley") && portraitIndex == 5 || d.speaker.Name.Equals("Maru") && portraitIndex == 9))
                return true;
            int newPortaitShakeTimer = PortraitureMod.helper.Reflection.GetField<int>(box, "newPortaitShakeTimer").GetValue();
            return newPortaitShakeTimer > 0;
        }
    }

    [HarmonyPatch]
    internal class ShopMenuFix
    {
        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(FixHelper.getTypeFullSDV("StardewValley.Menus.ShopMenu"), "draw");
        }

        internal static void Postfix(ShopMenu __instance, SpriteBatch b)
        {
            if (__instance.portraitPerson == null || !(Game1.viewport.Width > 800 && Game1.options.showMerchantPortraits))
                return;

            Texture2D texture = TextureLoader.getPortrait(__instance.portraitPerson.name);

            if (texture == null)
                texture = __instance.portraitPerson.Portrait;
            Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(__instance.xPositionOnScreen - 80 * Game1.pixelZoom), (float)__instance.yPositionOnScreen), new Rectangle(603, 414, 74, 74), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, false, 0.91f, -1, -1, 0.35f);
            Rectangle rectangle = TextureLoader.getSoureRectangle(texture);
            b.Draw(texture, new Rectangle(__instance.xPositionOnScreen - 80 * Game1.pixelZoom + Game1.pixelZoom * 5, __instance.yPositionOnScreen + Game1.pixelZoom * 5, 64 * Game1.pixelZoom, 64 * Game1.pixelZoom), new Rectangle?(rectangle), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.92f);

            IClickableMenu.drawHoverText(b, __instance.potraitPersonDialogue, Game1.dialogueFont, 0, 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, __instance.xPositionOnScreen - (int)Game1.dialogueFont.MeasureString(__instance.potraitPersonDialogue).X - Game1.tileSize, __instance.yPositionOnScreen + (__instance.portraitPerson != null ? 78 * Game1.pixelZoom : 0), 1f, (CraftingRecipe)null);
            __instance.drawMouse(b);
        }
        
    }


}
