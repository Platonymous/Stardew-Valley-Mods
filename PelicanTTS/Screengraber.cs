using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace PelicanTTS
{
    internal class Screengraber
    {
        Harmony instance;
        static bool shouldRead = false;
        static Point target = Point.Zero;
        static string capture = "";
        static string capturedTitle = "";
        static string capturedContent = "";
        static bool readFullScreen = false;

        public Screengraber()
        {
            instance = new Harmony("PelicanTTS.Screengrabber");
            foreach (MethodInfo drawMethod in typeof(SpriteBatch).GetMethods().Where(m => m.GetParameters().Any(p => p.ParameterType == typeof(string) && p.Name == "text")))
                if (drawMethod.GetParameters().FirstOrDefault(p => p.Name == "scale") is ParameterInfo pa)
                    if (pa.ParameterType == typeof(Vector2))
                        instance.Patch(drawMethod, new HarmonyMethod(AccessTools.Method(typeof(Screengraber), nameof(drawStringPatchN))));
                    else if (pa.ParameterType == typeof(float))
                        instance.Patch(drawMethod, new HarmonyMethod(AccessTools.Method(typeof(Screengraber), nameof(drawStringPatchF))));
                    else
                        instance.Patch(drawMethod, new HarmonyMethod(AccessTools.Method(typeof(Screengraber), nameof(drawStringPatchN))));
                else
                    instance.Patch(drawMethod, new HarmonyMethod(AccessTools.Method(typeof(Screengraber), nameof(drawStringPatchN))));

            instance.Patch(AccessTools.Method(typeof(SpriteText), nameof(SpriteText.drawString)), new HarmonyMethod(AccessTools.Method(typeof(Screengraber), nameof(drawFontPatch))));
            instance.Patch(AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawToolTip)), new HarmonyMethod(AccessTools.Method(typeof(Screengraber), nameof(drawToolTip))));
            if (typeof(IClickableMenu).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => m.Name.Contains(nameof(IClickableMenu.drawHoverText)) && m.GetParameters().Any(p => p.ParameterType == typeof(string) && p.Name == "text")).FirstOrDefault() is MethodInfo hoverMethod)
                instance.Patch(hoverMethod, new HarmonyMethod(AccessTools.Method(typeof(Screengraber), nameof(drawHoverText))));

            if (Type.GetType("Pathoschild.Stardew.LookupAnything.DrawTextHelper, LookupAnything") is Type drawTextHelper)
            {
                foreach (MethodInfo laMethod in drawTextHelper.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    if (laMethod.GetParameters().Any(p => p.ParameterType == typeof(string) && p.Name == "text"))
                        instance.Patch(laMethod, new HarmonyMethod(AccessTools.Method(typeof(Screengraber), nameof(drawTextBlock))));
                    else
                        instance.Patch(laMethod, new HarmonyMethod(AccessTools.Method(typeof(Screengraber), nameof(drawTextBlock2))));
            }
        }

        public static void drawTextBlock(string text, Vector2 position, SpriteFont font, float scale)
        {
            drawStringPatchF(text, position, font, scale);
        }

        public static void drawTextBlock2(IEnumerable<object> text, Vector2 position, SpriteFont font, float scale)
        {
            drawStringPatchF(string.Join(" ",text.Select(t => t.GetType().GetProperty("Text").GetValue(t))), position, font, scale);
        }

        public static void drawToolTip(string hoverText, string hoverTitle)
        {
            capturedTitle = hoverTitle;
            capturedContent = hoverText;
        }
        public static void drawHoverText(string text)
        {
            capturedContent = text;
        }


        public void read(Point position)
        {
            capture = null;
            target = position;
            capturedTitle = "";
            capturedContent = "";
            PelicanTTSMod._helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }

        public void readFull()
        {
            readFullScreen = true;
            capture = null;
            PelicanTTSMod._helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            PelicanTTSMod._helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
            PelicanTTSMod._helper.Events.Display.Rendering += Display_Rendering;
        }

        private void Display_Rendering(object sender, StardewModdingAPI.Events.RenderingEventArgs e)
        {
            shouldRead = true;
            PelicanTTSMod._helper.Events.Display.Rendering -= Display_Rendering;
            PelicanTTSMod._helper.Events.Display.Rendered += Display_Rendered;
        }

        private void Display_Rendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            PelicanTTSMod._helper.Events.Display.Rendered -= Display_Rendered;
            shouldRead = false;

            if (capture == null || capture.Length == 0)
                if (capturedTitle.Length > 0)
                    capture = capturedTitle + ": " + capturedContent;
                else
                    capture = capturedContent;

            if (capture != null && capture != "")
                SpeechHandlerPolly.configSay("Default", PelicanTTSMod.config.Voices["Default"]?.Voice ?? "Salli", capture);
        }

        public static Rectangle TextBounds(string text, SpriteFont spriteFont, Vector2 scale, Vector2 position)
        {
            var p = spriteFont.MeasureString(text);
            return new Rectangle((int)position.X, (int)position.Y, (int)(p.X * scale.X), (int)(p.Y * scale.Y));
        }

        public static void drawStringPatchN(string text, Vector2 position, SpriteFont spriteFont)
        {
            drawStringPatch(text, position, spriteFont, Vector2.One);
        }


        public static void drawStringPatchF(string text, Vector2 position, SpriteFont spriteFont, float scale)
        {
            drawStringPatch(text, position, spriteFont, new Vector2(scale, scale));
        }

        public static void drawStringPatchV(string text, Vector2 position, SpriteFont spriteFont, Vector2 scale)
        {
            drawStringPatch(text, position, spriteFont, scale);
        }

        public static void drawStringPatch(string text, Vector2 position, SpriteFont spriteFont, Vector2 scale)
        {
            if (shouldRead && (readFullScreen || TextBounds(text, spriteFont, scale, position).Contains(target)))
            {
                capture = text;
                shouldRead = false;
            }
        }

        public static void drawFontPatch(string s, int x, int y)
        {
            int width = SpriteText.getWidthOfString(s);
            int height = SpriteText.getHeightOfString(s);

            if (shouldRead && (readFullScreen || new Rectangle(x, y, width, height).Contains(target)))
            {
                capture = s;
                shouldRead = false;
            }
        }

    }
}
