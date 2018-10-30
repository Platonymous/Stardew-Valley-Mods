using LeBoyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Minigames;
using System.Threading;

namespace SDVGameBoy
{
    public class GBMinigame : IMinigame
    {
        GBZ80 emulator = new GBZ80();
        Thread emulatorThread;
        Texture2D emulatorBackbuffer;
        Rectangle bounds;
        bool color = true;
        bool quit;

        static GamePadState gamePadState => GamePad.GetState(PlayerIndex.One);
        static bool DPadRight => (gamePadState.DPad.Right == ButtonState.Pressed);
        static bool DPadLeft => (gamePadState.DPad.Left == ButtonState.Pressed);
        static bool DPadUp => (gamePadState.DPad.Up == ButtonState.Pressed);
        static bool DPadDown => (gamePadState.DPad.Down == ButtonState.Pressed);
        static bool GPadB => (gamePadState.Buttons.B == ButtonState.Pressed);
        static bool GPadA => (gamePadState.Buttons.A == ButtonState.Pressed);
        static bool GPadBack => (gamePadState.Buttons.Back == ButtonState.Pressed);
        static bool GPadStart => (gamePadState.Buttons.Start == ButtonState.Pressed);
        static bool GPadAny => (DPadRight || DPadLeft || DPadUp || DPadDown || GPadB || GPadA || GPadBack || GPadStart);
        static bool keyMode = true;

        public GBMinigame(byte[] game)
        {
            quit = false;
            changeScreenSize();
            emulatorBackbuffer = new Texture2D(Game1.graphics.GraphicsDevice, 160, 144);
            emulator.Load(game);
            emulatorThread = new Thread(EmulatorWork);
            emulatorThread.Start();
        }

        private void EmulatorWork()
        {
            double cpuSecondsElapsed = 0.0f;

            MicroStopwatch s = new MicroStopwatch();
            s.Start();

            while (true)
            {
                uint cycles = emulator.DecodeAndDispatch();

                cpuSecondsElapsed += cycles / GBZ80.ClockSpeed;

                double realSecondsElapsed = s.ElapsedMicroseconds * 1000000;

                if (realSecondsElapsed - cpuSecondsElapsed > 0.0)
                    realSecondsElapsed = s.ElapsedMicroseconds * 1000000;

                if (s.ElapsedMicroseconds > 1000000)
                {
                    s.Restart();
                    cpuSecondsElapsed -= 1.0;
                }
            }
        }

        public void changeScreenSize()
        {
            Rectangle viewport = Game1.spriteBatch.GraphicsDevice.Viewport.Bounds;
            bounds = new Rectangle(0, (int)((viewport.Height * 0.2f) / 2f), viewport.Width, (int)(viewport.Height * 0.8f));

            float aspectRatio = bounds.Width / (float)bounds.Height;
            float targetAspectRatio = 160.0f / 144.0f;

            if (aspectRatio > targetAspectRatio)
            {
                int targetWidth = (int)(bounds.Height * targetAspectRatio);
                bounds.X = (bounds.Width - targetWidth) / 2;
                bounds.Width = targetWidth;
            }
            else if (aspectRatio < targetAspectRatio)
            {
                int targetHeight = (int)(bounds.Width / targetAspectRatio);
                bounds.Y = (bounds.Height - targetHeight) / 2;
                bounds.Height = targetHeight;
            }
        }


        public void draw(SpriteBatch b)
        {
            b.GraphicsDevice.Clear(Color.Black);
            b.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);
            b.Draw(emulatorBackbuffer, bounds, color ? Color.DarkSeaGreen : Color.White);
            b.End();
        }

        public void leftClickHeld(int x, int y)
        {

        }

        public string minigameId()
        {
            return "GBMinigame";
        }

        public bool overrideFreeMouseMovement()
        {
            return true;
        }

        public void receiveEventPoke(int data)
        {

        }

        public void receiveKeyPress(Keys k)
        {
            if (k == Keys.Escape)
                quit = true;

            if (k == Keys.Enter)
                keyMode = true;

            if (k == Keys.C)
                color = !color;

            if (!keyMode)
                return;

            if (k == Keys.Right || k == Keys.D)
                emulator.JoypadState[0] = true; //Right
            if (k == Keys.Left || k == Keys.A)
                emulator.JoypadState[1] = true; //Left
            if (k == Keys.Up || k == Keys.W)
                emulator.JoypadState[2] = true; //Up
            if (k == Keys.Down || k == Keys.S)
                emulator.JoypadState[3] = true; //Down
            if (k == Keys.OemComma)
                emulator.JoypadState[4] = true; //A
            if (k == Keys.OemPeriod)
                emulator.JoypadState[5] = true; //B
            if (k == Keys.Back)
                emulator.JoypadState[6] = true; //Select
            if (k == Keys.Enter)
                emulator.JoypadState[7] = true; //Start
        }

        private void readGamePad()
        {
            if (GPadStart)
                keyMode = false;

            if (keyMode)
                return;

            emulator.JoypadState[0] = DPadRight; //Right
            emulator.JoypadState[1] = DPadLeft; //Left
            emulator.JoypadState[2] = DPadUp; //Up
            emulator.JoypadState[3] = DPadDown; //Down
            emulator.JoypadState[4] = GPadB; //A
            emulator.JoypadState[5] = GPadA; //B
            emulator.JoypadState[6] = GPadBack; //Select
            emulator.JoypadState[7] = GPadStart; //Start
        }

        public void receiveKeyRelease(Keys k)
        {
            if (!keyMode)
                return;

            if (k == Keys.Right || k == Keys.D)
                emulator.JoypadState[0] = false; //Right
            if (k == Keys.Left || k == Keys.A)
                emulator.JoypadState[1] = false; //Left
            if (k == Keys.Up || k == Keys.W)
                emulator.JoypadState[2] = false; //Up
            if (k == Keys.Down || k == Keys.S)
                emulator.JoypadState[3] = false; //Down
            if (k == Keys.OemComma)
                emulator.JoypadState[4] = false; //A
            if (k == Keys.OemPeriod)
                emulator.JoypadState[5] = false; //B
            if (k == Keys.Back)
                emulator.JoypadState[6] = false; //Select
            if (k == Keys.Enter)
                emulator.JoypadState[7] = false; //Start
        }

        public void receiveLeftClick(int x, int y, bool playSound = true)
        {

        }

        public void receiveRightClick(int x, int y, bool playSound = true)
        {

        }

        public void releaseLeftClick(int x, int y)
        {

        }

        public void releaseRightClick(int x, int y)
        {

        }

        public bool tick(GameTime time)
        {
            readGamePad();

            byte[] backbuffer = emulator.GetScreenBuffer();
            if (backbuffer != null)
                emulatorBackbuffer.SetData<byte>(backbuffer);

            return quit;
        }

        public void unload()
        {
            if (emulatorThread != null && emulatorThread.IsAlive)
                emulatorThread.Abort();
        }

        public bool doMainGameUpdates()
        {
            return false;
        }
    }

    public class MicroStopwatch : System.Diagnostics.Stopwatch
    {
        readonly double _microSecPerTick = 1000000D / Frequency;

        public MicroStopwatch()
        {

        }

        public long ElapsedMicroseconds => (long)(ElapsedTicks * _microSecPerTick);
    }
}
