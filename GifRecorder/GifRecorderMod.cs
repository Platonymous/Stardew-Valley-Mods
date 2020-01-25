using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.IO;
using System.Reflection;
using Harmony;
using System.Drawing;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GifRecorder
{
    public class GifRecorderMod : Mod
    {
        static Texture2D tex;
        static Texture2D transparentBackground;
        static Microsoft.Xna.Framework.Color[] colors;
        static ConcurrentQueue<Microsoft.Xna.Framework.Color[]> frames = new ConcurrentQueue<Microsoft.Xna.Framework.Color[]>();
        static bool recording = false;
        static Texture2D pixel;
        static bool frameMet = false;
        public static Config config;

        internal static IModHelper _helper;
        internal static IMonitor _monitor;
        internal static Thread gifThread;
        internal static int frameCounter = 0;
        internal static int maxFrames => config.MaxFrames;
        const string recString = "Recording";
        const string recExportString = "Exporting";
        internal static string recDots = "";
        static bool nextRecFrame = false;
        static float recAngle = 0f;
        static bool completedExport = false;
        static Rectangle? framing = null;
        static Texture2D framingTexture = null;
        static bool shouldFrame = false;
        static bool removeMapLayers = false;
        static Color tranparentColor = Color.FromArgb(255,10,10,10);
        static xTile.Dimensions.Rectangle? FixedViewport = null;
        public static Microsoft.Xna.Framework.Color transparentXNA => new Microsoft.Xna.Framework.Color(tranparentColor.R, tranparentColor.G, tranparentColor.B, tranparentColor.A);

        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<Config>();
            _helper = Helper;
            _monitor = Monitor;

            helper.Events.Display.Rendering += (s, e) =>
            {
                if (FixedViewport.HasValue)
                    Game1.viewport = FixedViewport.Value;
            }; 

            helper.Events.Display.RenderingWorld += (s, e) =>
            {
                if (removeMapLayers)
                    Game1.graphics.GraphicsDevice.Clear(transparentXNA);
            };

            helper.Events.GameLoop.GameLaunched += (s, e) =>
            {
                pixel = getRectangle(1, 1, Microsoft.Xna.Framework.Color.White);
                tex = new Texture2D(Game1.graphics.GraphicsDevice, Game1.game1.Window.ClientBounds.Width, Game1.game1.Window.ClientBounds.Height);
                colors = new Microsoft.Xna.Framework.Color[Game1.game1.Window.ClientBounds.Width * Game1.game1.Window.ClientBounds.Height];
                transparentBackground = getRectangle(Game1.game1.Window.ClientBounds.Width, Game1.game1.Window.ClientBounds.Height, transparentXNA);
            };

            helper.Events.GameLoop.UpdateTicked += (s, e) =>
            {
                uint frameM = (uint)(60 / config.FPS);
                frameMet = e.IsMultipleOf(frameM);
                nextRecFrame = e.IsMultipleOf(30);
            };

            helper.Events.Display.WindowResized += (s, e) =>
            {
                tex.Dispose();
                transparentBackground.Dispose();
                transparentBackground = getRectangle(Game1.game1.Window.ClientBounds.Width, Game1.game1.Window.ClientBounds.Height, transparentXNA);
                colors = null;
                tex = new Texture2D(Game1.graphics.GraphicsDevice, Game1.game1.Window.ClientBounds.Width, Game1.game1.Window.ClientBounds.Height);
                colors = new Microsoft.Xna.Framework.Color[Game1.game1.Window.ClientBounds.Width * Game1.game1.Window.ClientBounds.Height];
                setupFraming(true);
                if (recording)
                {
                    recording = false;
                    frameCounter = 0;
                }
            };

            helper.Events.Input.ButtonPressed += (s, e) =>
            {

                if (!config.WithCtrlButton || shouldFrame || Helper.Input.IsDown(SButton.LeftControl) || Helper.Input.IsDown(SButton.RightControl))
                {

                    if (e.Button == config.RecordButton && Helper.Input.IsDown(SButton.LeftControl) || Helper.Input.IsDown(SButton.RightControl))
                    {
                        if (!recording && gifThread == null)
                        {
                            frames = new ConcurrentQueue<Microsoft.Xna.Framework.Color[]>();
                            frameCounter = 0;
                            recording = true;
                            startExport();
                        }
                        else if (recording)
                            recording = false;
                    }

                    if (recording)
                        return;

                    if (e.Button == config.FrameButton && !recording)
                    {
                        if (!shouldFrame && !framing.HasValue)
                            setupFraming();
                        else if (!shouldFrame)
                            shouldFrame = true;
                        else
                            shouldFrame = false;
                    }

                }

                if (recording)
                    return;

                if (shouldFrame && !recording)
                {

                    if (shouldFrame && e.Button == config.FrameUp && !recording)
                    {
                        if (!framing.HasValue)
                            setupFraming();

                        Rectangle rect = framing.Value;
                        rect.Y = Math.Max(rect.Y - 20, 0);
                        framing = rect;
                        setupFraming();
                    }

                    if (shouldFrame && e.Button == config.FrameDown && !recording)
                    {
                        if (!framing.HasValue)
                            setupFraming();

                        Rectangle rect = framing.Value;
                        rect.Y = Math.Min(rect.Y + 20, Game1.game1.Window.ClientBounds.Height - rect.Height);
                        framing = rect;
                        setupFraming();
                    }

                    if (shouldFrame && e.Button == config.FrameRight && !recording)
                    {
                        if (!framing.HasValue)
                            setupFraming();

                        Rectangle rect = framing.Value;
                        rect.X = Math.Min(rect.X + 20, Game1.game1.Window.ClientBounds.Width - rect.Width);
                        framing = rect;
                        setupFraming();
                    }

                    if (shouldFrame && e.Button == config.FrameLeft && !recording)
                    {
                        if (!framing.HasValue)
                            setupFraming();

                        Rectangle rect = framing.Value;
                        rect.X = Math.Max(rect.X - 20, 0);
                        framing = rect;
                        setupFraming();
                    }


                    if (shouldFrame && e.Button == config.FrameWider && !recording)
                    {
                        if (!framing.HasValue)
                            setupFraming();

                        Rectangle rect = framing.Value;
                        rect.Width = Math.Min(rect.Width + 20, Game1.game1.Window.ClientBounds.Width - rect.X);
                        rect.X -= 10;
                        framing = rect;
                        setupFraming();
                    }

                    if (shouldFrame && e.Button == config.FrameTaller && !recording)
                    {
                        if (!framing.HasValue)
                            setupFraming();

                        Rectangle rect = framing.Value;
                        rect.Height = Math.Min(rect.Height + 20, Game1.game1.Window.ClientBounds.Height - rect.Y);
                        rect.Y -= 10;
                        framing = rect;
                        setupFraming();
                    }

                    if (shouldFrame && e.Button == config.FrameFlatter && !recording)
                    {
                        if (!framing.HasValue)
                            setupFraming();

                        Rectangle rect = framing.Value;
                        rect.Height = Math.Max(rect.Height - 20, 0);
                        rect.Y += 10;
                        framing = rect;
                        setupFraming();
                    }

                    if (shouldFrame && e.Button == config.FrameThiner && !recording)
                    {
                        if (!framing.HasValue)
                            setupFraming();

                        Rectangle rect = framing.Value;
                        rect.Width = Math.Max(rect.Width - 20, 0);
                        rect.X += 10;
                        framing = rect;
                        setupFraming();
                    }

                }

                if (shouldFrame || Helper.Input.IsDown(SButton.LeftControl) || Helper.Input.IsDown(SButton.RightControl))
                {

                    if (e.Button == config.FixViewport)
                        if (!FixedViewport.HasValue)
                            FixedViewport = new xTile.Dimensions.Rectangle(Game1.viewport);
                        else
                            FixedViewport = null;

                    if (e.Button == config.RemoveBackground)
                        removeMapLayers = !removeMapLayers;

                }

    
            };
            HarmonyInstance instance = HarmonyInstance.Create("GifRecorder");
            instance.Patch(typeof(xTile.Layers.Layer).GetMethod("Draw",BindingFlags.Public | BindingFlags.Instance), new HarmonyMethod(this.GetType().GetMethod("PreventMapDraw", BindingFlags.Public | BindingFlags.Static)));
            instance.Patch(typeof(Game1).GetMethod("renderScreenBuffer", BindingFlags.Instance | BindingFlags.NonPublic), new HarmonyMethod(this.GetType().GetMethod("Record", BindingFlags.Public | BindingFlags.Static)));
        }

        public static bool PreventMapDraw(xTile.Layers.Layer __instance)
        {
            return !removeMapLayers;
        }

        public static void setupFraming(bool reset = false)
        {
            int w = Game1.game1.Window.ClientBounds.Width;
            int h = Game1.game1.Window.ClientBounds.Height;
            int scale = config.Scale;
            if (framing == null || reset)
                framing = new Rectangle((w - (w / scale)) / 2, (h - (h/scale))/2, w/scale, h/scale);

            if (framing.HasValue)
            {
                framingTexture = new Texture2D(Game1.graphics.GraphicsDevice, w, h);
                Microsoft.Xna.Framework.Color[] data = new Microsoft.Xna.Framework.Color[w * h];
                for (int x = 0; x < w; x++)
                    for (int y = 0; y < h; y++)
                        data[x + (y * w)] = !framing.Value.Contains(new Point(x, y)) ? Microsoft.Xna.Framework.Color.White : Microsoft.Xna.Framework.Color.Transparent;

                framingTexture.SetData(data);
            }

            shouldFrame = !reset;
        }

        public static Texture2D getRectangle(int width, int height, Microsoft.Xna.Framework.Color color)
        {
            Texture2D rect = new Texture2D(Game1.graphics.GraphicsDevice, width, height);

            Microsoft.Xna.Framework.Color[] data = new Microsoft.Xna.Framework.Color[width * height];
            for (int i = 0; i < data.Length; ++i)
                data[i] = color;
            rect.SetData(data);
            return rect;
        }

        public static void startExport()
        {
            if (gifThread == null)
            {
                completedExport = false;
                gifThread = new Thread(() => exportGif(config.Scale, config.Delay, shouldFrame, config.DelayGifEncoderStep, config.DelayGifEncoder, framing.HasValue ? new Rectangle?(new Rectangle(framing.Value.Location,framing.Value.Size)) :null ));
                gifThread.Start();
            }

        }

        
    public static void exportGif(int scale, int delay, bool shouldFrame, int recDelay, int initialRecDelay, Rectangle? framing)
        {
            int i = 0;
            int tCol = tranparentColor.ToArgb();
            string name = "GeneratedGif" + i + ".gif";
            var path = Path.Combine(_helper.DirectoryPath, "Generated", name);
            while (File.Exists(path))
            {
                i++;
                name = "GeneratedGif" + i + ".gif";
                path = Path.Combine(_helper.DirectoryPath, "Generated", name);
            }

            int w = Game1.game1.Window.ClientBounds.Width;
            int h = Game1.game1.Window.ClientBounds.Height;
            using (FileStream f = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            using (var gifWriter = new AnimatedGif.AnimatedGifCreator(f, delay, 0))
            {
                if (initialRecDelay != 0)
                    Thread.Sleep(initialRecDelay);

                while ((frames != null && frames.Count > 0) || recording)
                {
                    Microsoft.Xna.Framework.Color[] c;
                    if (frames != null && frames.Count > 0 && frames.TryDequeue(out c))
                    {
                        using (MemoryStream m = new MemoryStream())
                        {
                            if (shouldFrame && framing.HasValue)
                                using (Bitmap framedImage = new Bitmap(framing.Value.Width, framing.Value.Height))
                                {
                                    for (int x = framing.Value.X; x < framing.Value.X + framing.Value.Width; x++)
                                        for (int y = framing.Value.Y; y < framing.Value.Y + framing.Value.Height; y++)
                                            if (c[x + (y * w)] is Microsoft.Xna.Framework.Color cXY)
                                                if (Color.FromArgb(cXY.A, cXY.R, cXY.G, cXY.B) is Color nc && !removeMapLayers || (nc.ToArgb() - tCol > 20))
                                                    framedImage.SetPixel(x - framing.Value.X, y - framing.Value.Y, nc);
                                    gifWriter.AddFrame(framedImage, quality: AnimatedGif.GifQuality.Bit8);
                                }
                            else
                                using (Bitmap image = new Bitmap(w / scale, h / scale))
                                {
                                    for (int x = 0; x < w; x++)
                                        if (x % scale == 0)
                                            for (int y = 0; y < h; y++)
                                                if (y % scale == 0)
                                                    if (c[x + (y * w)] is Microsoft.Xna.Framework.Color cXY)
                                                        if (Color.FromArgb(cXY.A, cXY.R, cXY.G, cXY.B) is Color nc && !removeMapLayers || (nc.ToArgb() - tCol > 20))
                                                            image.SetPixel(x / scale, y / scale, nc);

                                    gifWriter.AddFrame(image, quality: AnimatedGif.GifQuality.Bit8);
                                }
                        }
                    }
                    if (recDelay != 0)
                        Thread.Sleep(recDelay);
                }
            }

            gifThread = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            completedExport = true;
            frames = null;
            _monitor.Log("Exported: " + name);

        }

        public static void Record()
        {
            if (completedExport)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                completedExport = false;
            }

            if (recording && frameMet && tex != null)
            {
                frameCounter++;
                frameMet = false;
                Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
                Game1.graphics.GraphicsDevice.GetBackBufferData(colors);
                tex.SetData(colors);
                Task.Run( () => frames.Enqueue((Microsoft.Xna.Framework.Color[])colors.Clone()));

                Game1.spriteBatch.Draw(tex, new Microsoft.Xna.Framework.Rectangle(0, 0, tex.Width, tex.Height), Microsoft.Xna.Framework.Color.White);
                drawRecordingInfo();
                Game1.spriteBatch.End();
                if (frameCounter >= maxFrames)
                    recording = false;

            }
            else if (recording || gifThread != null)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
                drawRecordingInfo();
                Game1.spriteBatch.End();
            }else if (shouldFrame && framing.HasValue)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
                Game1.spriteBatch.Draw(framingTexture, Microsoft.Xna.Framework.Vector2.Zero, Microsoft.Xna.Framework.Color.Black * config.FrameOpacity);
                Game1.spriteBatch.End();
            }
        }


        public static void drawRecordingInfo()
        {
            if (nextRecFrame)
            {
                if (recDots != "...")
                    recDots += ".";
                else
                    recDots = "";

                nextRecFrame = false;
            }

            recAngle += recording ? 0.1f : -0.1f;

            if (recAngle == 1)
                recAngle = 0;

            if (recAngle < 0)
                recAngle = 0.9f;

            if (shouldFrame && framing.HasValue)
                Game1.spriteBatch.Draw(framingTexture, Microsoft.Xna.Framework.Vector2.Zero, Microsoft.Xna.Framework.Color.Black * config.FrameOpacity);

            Game1.spriteBatch.Draw(pixel, new Microsoft.Xna.Framework.Rectangle(15, 15, 140, 30), Microsoft.Xna.Framework.Color.Black * 0.5f);

            Game1.spriteBatch.Draw(pixel, new Microsoft.Xna.Framework.Rectangle(30, 30, 20, 20), null, (recording ? Microsoft.Xna.Framework.Color.Red : Microsoft.Xna.Framework.Color.Green) * 0.7f, recAngle, new Microsoft.Xna.Framework.Vector2(0.5f, 0.5f), SpriteEffects.None, 0);

            Game1.spriteBatch.DrawString(Game1.smallFont, (recording ? recString : recExportString) + recDots, new Microsoft.Xna.Framework.Vector2(50, 20), Microsoft.Xna.Framework.Color.White * 0.7f, 0, Microsoft.Xna.Framework.Vector2.Zero, 0.7f, SpriteEffects.None, 0);

        }

    }
}
