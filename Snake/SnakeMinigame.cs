using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PyTK.Extensions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;
using System;


namespace Snake
{
    public class SnakeMinigame : IMinigame
    {

        #region Enums

        public enum Direction
        {
            UP = 0,
            LEFT = 1,
            DOWN = 2,
            RIGHT = 3
        }
        #endregion

        #region Constructor

        public SnakeMinigame(IModHelper helper)
        {
            ShouldQuit = false;
            Initialize(helper);
            StartGame();
        }

        #endregion

        #region Properties

        public Random Random { get; set; }
        public Board Board { get; set; }
        public SnakesHead Player { get; set; }
        public Point TiledSize { get; set; }
        public Point SpriteSize { get; set; }
        public Texture2D BoardTexture { get; set; }
        public Texture2D BoardDebug { get; set; }
        public Texture2D SpriteSheet { get; set; }
        public float Scale { get; set; }
        public Color BoardColor { get; set; }
        public Color SnakeColor { get; set; }
        public float StepLength { get; set; }
        public bool ShouldQuit { get; set; }
        public bool debug = false;
        public bool hideObjects = false;
        public string LastMusic { get; set; }
        public Texture2D Background { get; set; }
        public Color BackgroundColor { get; set; }
        public int Highscore { get; set; }

        #endregion

        #region Initiation

        public void Initialize(IModHelper helper)
        {
            Highscore = 0;
            Random = new Random();
            BoardTexture = helper.Content.Load<Texture2D>(@"Assets/board.png");
            BoardDebug = helper.Content.Load<Texture2D>(@"Assets/board_debug.png");
            SpriteSheet = helper.Content.Load<Texture2D>(@"Assets/sprites.png");
            Background = helper.Content.Load<Texture2D>(@"Assets/background.png");
            TiledSize = new Point(16, 16);
            SpriteSize = new Point(69, 80);
            Scale = 0.8f;
            StepLength = 0.1f;
            BoardColor = Color.LawnGreen;
            SnakeColor = Color.White;
            BackgroundColor = Color.DarkSlateGray;
            SetupBoard();
        }



        #endregion

        #region Game Setup

        public void StartGame()
        {
            SetupBoard();
        }

        public void SetupBoard()
        {
            Game1.changeMusicTrack("tinymusicbox");
            Board = new Board(this);
            Board.AddPlayer();
            for (int i = 0; i < 4; i++)
                Player.AddNewTailSegment();

            Board.SpawnCollectible();
        }
        #endregion

        #region Menu

        public void DrawMenu(SpriteBatch b)
        {
            SpriteFont font = Game1.dialogueFont;
            string text = Player.score.ToString();
            Vector2 size = font.MeasureString(text);
            float scale = 3;
            b.DrawString(font, text, new Vector2(Game1.viewport.Width - 20 - size.X * scale, 20), Color.White, 0f,Vector2.Zero,scale,SpriteEffects.None,1f);

            if (Board.Paused)
            {
                text = "SNAKE";
                size = font.MeasureString(text);
                scale = 6;
                float y = 0;

                string text1 = "by Platonymous";
                float y1 = 10 + y + size.Y * scale;
                Vector2 size1 = font.MeasureString(text1);
                float scale1 = 1.5f;

                string text2 = "Press SPACE to start";
                float y2 = 10 + y1 + size.Y * scale1;
                Vector2 size2 = font.MeasureString(text2);

                string text3 = "Control: LEFT and RIGHT ";
                float y3 = 10 + y2 + size.Y * scale1;
                Vector2 size3 = font.MeasureString(text3);

                string text4 = "Exit: ESCAPE";
                float y4 = 10 + y3 + size.Y * scale1;
                Vector2 size4 = font.MeasureString(text4);

                float height = y4 + size.Y * scale1;
                float offset = (Game1.viewport.Height - height) / 2;

                b.DrawString(font, text, new Vector2((Game1.viewport.Width - size.X * scale) / 2, offset + y), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
                b.DrawString(font, text1, new Vector2((Game1.viewport.Width - size1.X * scale1) / 2, offset + y1), Color.White, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text2, new Vector2((Game1.viewport.Width - size2.X * scale1) / 2, offset + y2), Color.White, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text3, new Vector2((Game1.viewport.Width - size3.X * scale1) / 2, offset + y3), Color.White, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text4, new Vector2((Game1.viewport.Width - size4.X * scale1) / 2, offset + y4), Color.White, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
            }
        }

        #endregion

        #region Debug

        public void DrawDebugBoard(SpriteBatch b)
        {
            int w = 25;
            int x1 = 10;
            int y1 = 10;
            int x2 = x1 + TiledSize.X * w;
            int y2 = y1 + TiledSize.Y * w;

            for(int i= 0; i <= TiledSize.Y; i++)
                Utility.drawLineWithScreenCoordinates(x1, y1 + i * w, x2, y1 + i * w, b, Color.White);

            for (int j = 0; j <= TiledSize.X; j++)
                Utility.drawLineWithScreenCoordinates(x1 + j * w, y1, x1 + j * w, y2, b, Color.White);

            int px = (int) (x1 + w / 2 + Player.GetBoxPosition().X * w);
            int py = (int) (y1 + w / 2 + Player.GetBoxPosition().Y * w);

            Utility.drawLineWithScreenCoordinates(px, py,(int) (Board.GetDrawPosition(Player.GetBoxPosition().toVector2(),Player.Size).X + (Player.Size.X / 2)), (int)(Board.GetDrawPosition(Player.GetBoxPosition().toVector2(), Player.Size).Y - (Player.Size.Y / 4) + Player.Size.Y), b, Color.White);

            if (Board.nextCollectible == null)
                return;

            int cx = (int)(x1 + w/2 + Board.nextCollectible.position.X * w);
            int cy = (int)(y1 + w/2 + Board.nextCollectible.position.Y * w);

            Utility.drawLineWithScreenCoordinates(cx, cy, (int)(Board.nextCollectible.Drawposition.X + Board.nextCollectible.Size.X / 2), (int)(Board.nextCollectible.Drawposition.Y - (Board.nextCollectible.Size.Y / 4) + Board.nextCollectible.Size.Y), b, Color.Yellow);


        }

        #endregion

        #region IMinigame Implementation

        public void changeScreenSize()
        {
            Board.Resize();
        }

        public void draw(SpriteBatch b)
        {
            b.Begin(SpriteSortMode.Immediate,debug ? BlendState.Opaque : BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
            
            b.Draw(Background, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), BackgroundColor);
            Board.Draw(b);
            if (Board.Paused)
                b.Draw(Background, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), BackgroundColor * 0.5f);
            DrawMenu(b);
            if (debug)
                DrawDebugBoard(b);
            b.End();
        }

        public void leftClickHeld(int x, int y)
        {

        }

        public string minigameId()
        {
            return "Platonymous.Snake";
        }

        public bool overrideFreeMouseMovement()
        {
           return false;
        }

        public void receiveEventPoke(int data)
        {

        }

        public void receiveKeyPress(Keys k)
        {
            if(k.Equals(Keys.Left))
                Player.Turn(Direction.LEFT);
            if (k.Equals(Keys.Right))
                Player.Turn(Direction.RIGHT);
            if (k.Equals(Keys.Space))
                if (Board.GameOver)
                {
                    StartGame();
                    Board.Paused = false;
                }
                else
                    Board.Paused = !Board.Paused;
            if (k.Equals(Keys.Escape))
                if (!Board.Paused)
                    Board.Paused = true;
                else
                    ShouldQuit = true;

            if (k.Equals(Keys.Up) && debug)
                SnakeMod.monitor.Log(Player.position.ToString() + ":" + Player.GetBoxPosition().ToString() + ":" + Board.nextCollectible.position.ToString());
            if (k.Equals(Keys.Down) && debug)
                Player.AddNewTailSegment();
            if (k.Equals(Keys.F5))
                debug = !debug;
            if (k.Equals(Keys.F6))
                hideObjects = !hideObjects;
        }

        public void receiveKeyRelease(Keys k)
        {

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
            if(!debug || Keys.Up.isDown())
                Board.Update(time.TotalGameTime.Milliseconds);

            if(ShouldQuit)
                Game1.changeMusicTrack("none");

            return ShouldQuit;
        }

        public void unload()
        {

        }

        public bool doMainGameUpdates()
        {
            return false;
        }

        #endregion
    }
}
