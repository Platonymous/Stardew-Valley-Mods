using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PyTK;
using PyTK.Extensions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public static HighscoreList HighscoreTable { get; set; } = new HighscoreList();
        public bool ShowHighscore { get; set; } = false;

        #endregion

        #region Initiation

        public void Initialize(IModHelper helper)
        {
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

        public static void setScore(string name, int score)
        {
            Highscore highscore = new Highscore(name, score);
            var oldEntry = HighscoreTable.Entries.Find(entry => entry.Name == name && entry.Value >= score);

            if (!(oldEntry is Highscore))
            {
                HighscoreTable.Entries.Add(highscore);
                SnakeMod.monitor.Log("New Highscore!");
                SnakeMod.monitor.Log(highscore.Name + ": " + highscore.Value);
                if (Game1.IsMasterGame)
                    PyNet.sendRequestToAllFarmers<bool>(SnakeMod.highscoreListReceiverName, HighscoreTable, null, serializationType: PyTK.Types.SerializationType.JSON);
                else
                    Task.Run(async () => await PyNet.sendRequestToFarmer<bool>(SnakeMod.highscoreReceiverName, highscore, Game1.MasterPlayer,serializationType: PyTK.Types.SerializationType.JSON));
            }
        }
        #endregion

        #region Menu

        public void DrawMenu(SpriteBatch b)
        {
            SpriteFont font = Game1.dialogueFont;
            string text = Player.score.ToString();
            Vector2 size = font.MeasureString(text);
            float scale = 3;
            float margin = 10;
            b.DrawString(font, text, new Vector2(Game1.viewport.Width - 20 - size.X * scale, 20), Color.White, 0f,Vector2.Zero,scale,SpriteEffects.None,1f);

            if (Board.Paused && !ShowHighscore)
            {
                text = "SNAKE";
                size = font.MeasureString(text);
                scale = 6;
                float y = 0;

                string text1 = "by Platonymous";
                float y1 = margin + y + size.Y * scale;
                Vector2 size1 = font.MeasureString(text1);
                float scale1 = 1.5f;

                string text2 = "Press SPACE to start";
                float y2 = margin + y1 + size.Y * scale1;
                Vector2 size2 = font.MeasureString(text2);

                string text3 = "Control: LEFT and RIGHT ";
                float y3 = margin + y2 + size.Y * scale1;
                Vector2 size3 = font.MeasureString(text3);

                string text4 = "Exit: ESCAPE";
                float y4 = margin + y3 + size.Y * scale1;
                Vector2 size4 = font.MeasureString(text4);;

                string text5 = "Toggle Highscores: H";
                float y5 = margin + y4 + size.Y * scale1;
                Vector2 size5 = font.MeasureString(text5); ;

                float height = y5 + size.Y * scale1;
                float offset = (Game1.viewport.Height - height) / 2;

                b.DrawString(font, text, new Vector2((Game1.viewport.Width - size.X * scale) / 2, offset + y), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
                b.DrawString(font, text1, new Vector2((Game1.viewport.Width - size1.X * scale1) / 2, offset + y1), Color.White, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text2, new Vector2((Game1.viewport.Width - size2.X * scale1) / 2, offset + y2), Color.White, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text3, new Vector2((Game1.viewport.Width - size3.X * scale1) / 2, offset + y3), Color.White, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text4, new Vector2((Game1.viewport.Width - size4.X * scale1) / 2, offset + y4), Color.White, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text5, new Vector2((Game1.viewport.Width - size5.X * scale1) / 2, offset + y5), Color.White, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
            }
            else if (Board.Paused && ShowHighscore)
            {
                string text1 = "Highscores";
                float y1 = 0;
                Vector2 size1 = font.MeasureString(text1);
                float scale1 = 1.5f;

                var scores = new List<string>();
                
                string text2 = "-----";
                string text3 = text2;
                string text4 = text2;
                string text5 = text2;
                string text6 = text2;
                string text7 = text2;
                List<Highscore> highs = new List<Highscore>(HighscoreTable.Entries);

                if (highs.Count > 0)
                {

                    var next = highs.Find(entry => entry.Value == highs.Max(ent => ent.Value) && !scores.Contains(entry.Name));

                    if (next is Highscore)
                    {
                        text2 = next.Name;
                        text3 = next.Value.ToString();
                        scores.AddOrReplace(next.Name);
                        highs.RemoveAll(ent => ent.Name == next.Name);
                    }

                    next = highs.Find(entry => entry.Value == highs.Max(ent => ent.Value) && !scores.Contains(entry.Name));

                    if (next is Highscore)
                    {
                        text4 = next.Name;
                        text5 = next.Value.ToString();
                        scores.AddOrReplace(next.Name);
                        highs.RemoveAll(ent => ent.Name == next.Name);
                    }

                    next = highs.Find(entry => entry.Value == highs.Max(ent => ent.Value) && !scores.Contains(entry.Name));

                    if (next is Highscore)
                    {
                        text6 = next.Name;
                        text7 = next.Value.ToString();
                        scores.AddOrReplace(next.Name);
                    }
                }

                Vector2 size2 = font.MeasureString(text2);
                Vector2 size3 = font.MeasureString(text3);
                Vector2 size4 = font.MeasureString(text4);
                Vector2 size5 = font.MeasureString(text5);
                Vector2 size6 = font.MeasureString(text6);
                Vector2 size7 = font.MeasureString(text7);

                float y2 = margin * 1.5f + y1 + size1.Y * scale1;
                float y3 = margin + y2 + size2.Y * scale1;
                float y4 = margin * 2 + y3 + size3.Y * scale1;
                float y5 = margin + y4 + size4.Y * scale1;
                float y6 = margin * 2 + y5 + size5.Y * scale1;
                float y7 = margin + y6 + size6.Y * scale1;

                string text8 = "Toggle Highscores: H";
                float y8 = margin * 1.5f + y7 + size7.Y * scale1;
                Vector2 size8 = font.MeasureString(text8); ;

                float height = y8 + size.Y * scale1;
                float offset = (Game1.viewport.Height - height) / 2;

                b.DrawString(font, text1, new Vector2((Game1.viewport.Width - size1.X * scale1) / 2, offset + y1), Color.White, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text2, new Vector2((Game1.viewport.Width - size2.X * scale1) / 2, offset + y2), Color.Yellow, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text3, new Vector2((Game1.viewport.Width - size3.X * scale1) / 2, offset + y3), Color.LightGray, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text4, new Vector2((Game1.viewport.Width - size4.X * scale1) / 2, offset + y4), Color.White, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text5, new Vector2((Game1.viewport.Width - size5.X * scale1) / 2, offset + y5), Color.LightGray, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text6, new Vector2((Game1.viewport.Width - size6.X * scale1) / 2, offset + y6), Color.White, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text7, new Vector2((Game1.viewport.Width - size7.X * scale1) / 2, offset + y7), Color.LightGray, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text8, new Vector2((Game1.viewport.Width - size8.X * scale1) / 2, offset + y8), Color.White, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);

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
                    ShowHighscore = false;
                }
                else
                    Board.Paused = !Board.Paused;

            if (k.Equals(Keys.H) && Board.Paused)
                ShowHighscore = !ShowHighscore;

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

        public bool forceQuit()
        {
            Game1.changeMusicTrack("none");
            ShouldQuit = true;
            return true;
        }

        #endregion
    }
}
