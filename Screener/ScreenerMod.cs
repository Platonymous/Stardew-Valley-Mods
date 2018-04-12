using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;

namespace Screener
{
    public class ScreenerMod : Mod
    {
        private static bool holding = false;
        internal static IModHelper helper;
        internal static IMonitor monitor;
        internal static string folder = "screenshots";
        internal static string prefix = "screenshot_";
        internal static string ext = ".png";
        internal static List<Texture2D> record;
        internal static bool isRecording;

        internal static string filepath
        {
            get
            {
                int i = 0;
                string path = Path.Combine(helper.DirectoryPath, folder, prefix + Game1.uniqueIDForThisGame + ext);
                while (File.Exists(path))
                {
                    i++;
                    path = Path.Combine(helper.DirectoryPath, folder, prefix + Game1.uniqueIDForThisGame + "_" + i + ext);
                }
                return path;
            }
        }

        public override void Entry(IModHelper helper)
        {
            ScreenerMod.helper = Helper;
            monitor = Monitor;
            var instance = HarmonyInstance.Create("Platonymous.Screener");
            instance.PatchAll(Assembly.GetExecutingAssembly());

            DirectoryInfo screenshots = new FileInfo(filepath).Directory;
            if (!screenshots.Exists)
                screenshots.Create();
        }

        internal static void takeScreenshot()
        {
            
            if (Game1.options.zoomLevel == 1.0f)
            {
                Game1.options.zoomLevel = 1.000001f;
                return;
            }

            if (helper == null)
                return;

            if (!holding && Keyboard.GetState().IsKeyDown(Keys.K) || isRecording)
            {
                holding = true;

                Game1.game1.GraphicsDevice.SetRenderTarget((RenderTarget2D)null);
                Game1.game1.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
                
                Texture2D screen = (Texture2D)helper.Reflection.GetField<RenderTarget2D>(Game1.game1, "_screen").GetValue();

                if (!isRecording)
                {
                    screen.SaveAsPng(new FileStream(filepath, FileMode.Create), Game1.viewport.Width, Game1.viewport.Height);
                    monitor.Log("Screenshot saved as" + filepath);
                }
                else
                    record.Add(screen);

                if (Game1.options.zoomLevel == 1.000001f)
                    Game1.options.zoomLevel = 1.0f;
            }

            if (Keyboard.GetState().IsKeyUp(Keys.K))
                holding = false;
            
        }


    }
}
