using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PlatoWarpMenu
{
    public class PlatoWarpMenuMod : Mod
    {
        internal Config config;
        internal ITranslationHelper i18n => Helper.Translation;

        internal static bool intercept = false;

        internal static IModHelper _helper;

        internal static GameLocation CurrentLocation;

        internal static Action Callback;

        public static Texture2D LastScreen = null;

        public override void Entry(IModHelper helper)
        {
            _helper = helper;
            WarpMenu.Helper = Helper;

            config = helper.ReadConfig<Config>();

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            helper.Events.GameLoop.GameLaunched += (s, e) => SetUpConfigMenu();

            var harmony = Harmony.HarmonyInstance.Create("Platonymous.PlatoWarpMenu");
            harmony.Patch(typeof(Image).GetMethod("Save", new Type[] { typeof(string), typeof(ImageFormat) }), prefix: new Harmony.HarmonyMethod(this.GetType().GetMethod("InterceptScreenshot",System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)));
        }

        public static void GetLocationShot(GameLocation location, Action callback)
        {
            CurrentLocation = location;
            Callback = callback;
            PyTK.PyUtils.setDelayedAction(1, () => _helper.Events.Display.RenderedActiveMenu += Display_Rendered);
        }


        public static void Display_Rendered(object sender, RenderedActiveMenuEventArgs e)
        {
            intercept = true;
            var g = Game1.currentLocation;

                Game1.currentLocation = CurrentLocation;
                try
                {
                    Game1.spriteBatch.End();
                    Game1.game1.takeMapScreenshot(0.25f, CurrentLocation.Name);
                    Game1.spriteBatch.Begin();
                }
                catch
                {

                }

            Game1.currentLocation = g;
            _helper.Events.Display.RenderedActiveMenu -= Display_Rendered;
            intercept = false;
            Callback?.Invoke();
        }

        public static bool InterceptScreenshot(Image __instance, ref string filename)
        {
            if (!intercept)
                return true;

            if (!Directory.Exists(Path.Combine(_helper.DirectoryPath, "Temp")))
                Directory.CreateDirectory(Path.Combine(_helper.DirectoryPath, "Temp"));

            filename = Path.Combine(_helper.DirectoryPath, "Temp", Path.GetFileName(filename));

            using (var mem = new MemoryStream())
            {
                (__instance as Bitmap).Save(mem, ImageFormat.Png);
                LastScreen = Texture2D.FromStream(Game1.graphics.GraphicsDevice, mem);
            }

            return false;
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            e.Button.TryGetKeyboard(out Keys keyPressed);

            if (e.Button != config.MenuButton)
                return;


            if(Context.IsWorldReady)
                WarpMenu.Open();
        }

        private void SetUpConfigMenu()
        {
            if (!Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
                return;

            var api = Helper.ModRegistry.GetApi<IGMCMAPI>("spacechase0.GenericModConfigMenu");


            api.RegisterModConfig(ModManifest, () =>
            {
                config.MenuButton = SButton.J;
            }, () =>
            {
                Helper.WriteConfig<Config>(config);
            });

            api.RegisterLabel(ModManifest, ModManifest.Name, ModManifest.Description);
            api.RegisterSimpleOption(ModManifest, i18n.Get("MenuButton"), "", () => config.MenuButton, (SButton b) => config.MenuButton = b);
        }
    }
}
