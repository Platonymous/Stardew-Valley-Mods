using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Portraiture.HDP;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Portraiture
{
    public class PortraitureMod : Mod
    {
        private float displayAlpha;
        private float fixDisplayAlpha;
        private float unfixDisplayAlpha;
        public static IModHelper helper;
        private static Mod instance;
        internal static Rectangle? portaitBox;
        internal static PConfig config;
        internal static int WindowHeight = 720;
        private static bool isPortraitBoxOrg = false;
        public static Dictionary<string, MetadataModel> hdpdata = new Dictionary<string, MetadataModel>();

        public override void Entry(IModHelper help)
        {
            helper = help;
            instance = this;
            config = Helper.ReadConfig<PConfig>();
            string customContentFolder = Path.Combine(helper.DirectoryPath, "Portraits");
            displayAlpha = 0;
            fixDisplayAlpha = 0;
            unfixDisplayAlpha = 0;

            if (!Directory.Exists(customContentFolder))
                Directory.CreateDirectory(customContentFolder);

            help.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            help.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            harmonyFix();

            Helper.ConsoleCommands.Add("pmenu", "", (s, p) =>
             {
                 MenuLoader.OpenMenu(Game1.activeClickableMenu);
             });

            helper.Events.GameLoop.UpdateTicked += (s, e) => AnimatedTexture2D.ticked = e.Ticks;
            OvSpritebatchNew.initializePatch(new Harmony("Platonymous.Portraiture"));
            OvHDPortraits.initializePatch(new Harmony("Platonymous.Portraiture.HDP"));
            helper.Events.Content.AssetRequested += LoadDefaultAsset; 

        }

        private void harmonyFix()
        {
            Harmony instance = new Harmony("Platonymous.Portraiture");
            instance.PatchAll(Assembly.GetExecutingAssembly());
            instance.Patch(AccessTools.Method(typeof(DialogueBox), "isPortraitBox"), null, new HarmonyMethod(this.GetType(), nameof(isPortraitBox)));
            instance.Patch(AccessTools.Method(typeof(DialogueBox), "drawBox"), new HarmonyMethod(this.GetType(), nameof(drawPortrait)));

        }

        private void LoadDefaultAsset(object _, AssetRequestedEventArgs ev)
        {
            if(!PortraitureMod.helper.ModRegistry.IsLoaded("tlitookilakin.HDPortraits"))
            if (ev.Name.IsEquivalentTo("Mods/HDPortraits"))
                ev.LoadFrom(() => hdpdata, AssetLoadPriority.Low);
        }

        public static void drawPortrait(DialogueBox __instance,SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
        {
            portaitBox = new Rectangle(xPos, yPos, boxWidth, boxHeight);

            if (config?.ShowPortraitsAboveBox == true && b is SpriteBatch && (__instance?.isPortraitBox() == true || isPortraitBoxOrg))
            {
                    __instance?.drawPortrait(b);
            }
        }

        public static void isPortraitBox(ref bool __result)
        {
            isPortraitBoxOrg = __result;
            if (__result == true && config.ShowPortraitsAboveBox)
                __result = false;
        }

        private void OnReturnedToTitle(object sender, EventArgs e)
        {
            helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            helper.Events.Display.MenuChanged -= OnMenuChanged;
            helper.Events.Input.ButtonPressed -= OnButtonPressed;
        }

        public static void log (string text)
        {
            instance.Monitor.Log(text, LogLevel.Info);
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            WindowHeight = Game1.uiViewport.Height;
            TextureLoader.loadTextures();
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            TextureLoader.loadPreset(Monitor);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (e.IsMultipleOf(15))
            {  // quarter second
                displayAlpha = Math.Max(displayAlpha - 0.1f, 0);
                fixDisplayAlpha = Math.Max(fixDisplayAlpha - 0.1f, 0);
                unfixDisplayAlpha = Math.Max(unfixDisplayAlpha - 0.1f, 0);
            }
        }

        private void drawInfoBox(string text, SpriteBatch b, int x, int y, float alpha)
        {
            if (alpha <= 0)
                return;

            text = text.Replace('_', ' ');

            int textlength = (int)Game1.smallFont.MeasureString(text).X;
            int textheight = (int)Game1.smallFont.MeasureString(text).Y;
            int padding = Game1.pixelZoom * 12;
            int displayBoxWidth = (int)textlength + padding;
            int displayBoxHeight = (int)textheight + padding / 2;

            Vector2 boxPos = new Vector2(x, y);
            Vector2 displayBoxPos = new Vector2(boxPos.X, boxPos.Y - (displayBoxHeight + padding));

            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), (int)displayBoxPos.X, (int)displayBoxPos.Y, displayBoxWidth, displayBoxHeight, Color.White * alpha, 1f, false);
            if (alpha >= 0.3)
                Utility.drawTextWithShadow(b, text, Game1.smallFont, new Vector2(displayBoxPos.X + ((displayBoxWidth - textlength) / 2), Game1.pixelZoom + displayBoxPos.Y + ((displayBoxHeight - textheight) / 2)), Game1.textColor);
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if ((e.Button == config.styleChangeKey ||e.Button == config.changeKey || e.Button == config.fixPortraitKey || e.Button == config.menuKey) && Game1.activeClickableMenu is DialogueBox d && isPortraitBoxOrg && Game1.currentSpeaker is NPC cs)
            {
                if (d.width < 107 * Game1.pixelZoom * 3 / 2 || Helper.Reflection.GetField<bool>(d, "transitioning").GetValue() || !isPortraitBoxOrg || Helper.Reflection.GetField<bool>(d, "isQuestion").GetValue())
                    return;

                if (e.Button == config.changeKey)
                {

                    if (TextureLoader.presets.Presets.Any(p => p.Character == cs.Name))
                    {
                        displayAlpha = 0;
                        unfixDisplayAlpha = 0;
                        fixDisplayAlpha = 2;
                    }
                    else
                    {
                        TextureLoader.nextFolder();
                        displayAlpha = 2;
                    }
                }
                else if (e.Button == config.styleChangeKey)
                {
                    config.ShowPortraitsAboveBox = !config.ShowPortraitsAboveBox;
                    PortraitureMod.helper.WriteConfig(PortraitureMod.config);

                }
                else if (e.Button == config.fixPortraitKey)
                {
                    if(TextureLoader.presets.Presets.FirstOrDefault(p => p.Character == cs.Name) is Preset preset)
                    {
                        TextureLoader.setPreset(cs.Name, null);
                        displayAlpha = 0;
                        fixDisplayAlpha = 0;
                        unfixDisplayAlpha = 2;
                    }
                    else
                    {
                            TextureLoader.setPreset(cs.Name, TextureLoader.getFolderName());
                            fixDisplayAlpha = 2;
                    }
                }
                else
                    MenuLoader.OpenMenu(Game1.activeClickableMenu);
            }

        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            switch (e.NewMenu)
            {
                case null:
                    displayAlpha = 0;
                    unfixDisplayAlpha=0;
                    fixDisplayAlpha=0;
                    Helper.Events.Display.RenderedActiveMenu -= OnRenderedActiveMenu;
                    break;

                case ShopMenu shopMenu when (shopMenu.portraitTexture is Texture2D t && Game1.options.showMerchantPortraits):
                    Helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
                    break;

                case DialogueBox box when ((box.isPortraitBox() || isPortraitBoxOrg) && Game1.currentSpeaker is NPC npc && npc.Portrait is Texture2D):
                    Helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
                    break;
            }
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is DialogueBox d && (d.isPortraitBox() || isPortraitBoxOrg) && Game1.currentSpeaker is NPC cs)
            {
                int x = Helper.Reflection.GetField<int>(d, "x").GetValue();
                int y = Helper.Reflection.GetField<int>(d, "y").GetValue();
                int width = Helper.Reflection.GetField<int>(d, "width").GetValue();
                portaitBox = new Rectangle(x,y,width,width);



                drawInfoBox(TextureLoader.getFolderName(), Game1.spriteBatch, x, y, displayAlpha);
                drawInfoBox("Fixed!", Game1.spriteBatch, x, y, fixDisplayAlpha);
                drawInfoBox("Unfixed!", Game1.spriteBatch, x, y, unfixDisplayAlpha);
            }
        }


    }
}
