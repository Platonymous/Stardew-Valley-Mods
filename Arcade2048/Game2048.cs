using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PyTK;
using PyTK.Extensions;
using StardewValley;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;

namespace Arcade2048
{
    class Game2048 : IMinigame
    {
        public const int tileNumber = 4;
        const float margin = 0.02f;
        const float drawscale = 0.8f;
        const float textscale = 0.6f;
        const float backgroundScale = 0.1f;
        const float movementScale = 0.1f;
        const int backgroundSteps = 1;

        Vector2 backgoundPosition = Vector2.Zero;
        
        public int score = 0;
        bool quit = false;

        Color boardColor = Color.Beige.setLight(60);
        Color backGroundColor = Color.DarkSlateGray;
        Color backGroundColor2 = Color.DarkSlateGray.setLight(120);
        int backgroundTileSize = 0;
        int backgroundW = 0;
        int backgroundH = 0;
        Texture2D background;

        Color textColor = Color.White;
        Color tileSpaceBackground = Color.White * 0.5f;
        float minfontScale = 1f;
        SpriteFont font = Game1.dialogueFont;
        internal List<Tile> Tiles { get; set; }
        List<Point> tileBackgrounds;
        Rectangle boardSquare;
        Rectangle viewport;
        Random random = new Random();
        bool paused = true;
        bool gameover = false;

        internal int drawBoardSize;
        internal int drawTileSize;
        internal int drawMarginSize;
        internal int drawTextSize;
        internal Point drawPosition;

        float moveX = 0;
        float moveY = 0;

        bool inMoveCycle = false;

        #region start

        public Game2048()
        {
            setupBoard();
        }

        private void setupBoard()
        {
            score = 0;
            Tiles = new List<Tile>();
            tileBackgrounds = new List<Point>();
            for (int x = 0; x < tileNumber; x++)
                for (int y = 0; y < tileNumber; y++)
                    tileBackgrounds.Add(new Point(x, y));
            if (spawnRandomTile())
            spawnRandomTile();

            calculateSizes();
        }

        private bool spawnRandomTile()
        {
            if (Tiles.Count < tileNumber * tileNumber)
            {
                List<Point> free = tileBackgrounds.FindAll(b => !Tiles.Exists(t => t.GridPosition == b.toVector2()));

                int r = random.Next(0, free.Count - 1);
                  
                Tiles.Add(new Tile(free[r].toVector2(), 1));

                return true;
            }

            return false;
        }

        #endregion

        #region screensize

        private void calculateSizes()
        {
            backgoundPosition = Vector2.Zero;
            viewport = new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height);
            drawBoardSize = (int)(viewport.Height * drawscale);
            drawPosition = new Point((viewport.Width - drawBoardSize) / 2, (viewport.Height - drawBoardSize) / 2);
            drawMarginSize = (int)(drawBoardSize * margin);
            drawTileSize = (drawBoardSize - (drawMarginSize * (tileNumber + 1))) / tileNumber;
            boardSquare = new Rectangle(drawPosition.X, drawPosition.Y, drawBoardSize, drawBoardSize);
            drawTextSize = (int)(textscale * drawTileSize);

            backgroundTileSize = (int)(viewport.Width * backgroundScale);
            backgroundW = 1 + (int)Math.Ceiling((float)viewport.Width / backgroundTileSize);
            backgroundH = 1 + (int)Math.Ceiling((float)viewport.Height / backgroundTileSize);

            string text = "2048";
            Vector2 fontSize = font.MeasureString(text);
            minfontScale = drawTileSize / fontSize.X * textscale;
            background = null;
        }

        internal Rectangle getTileSquare(Vector2 position)
        {
            Point tilePosition = getTileDrawPosition(position);
            return new Rectangle(tilePosition.X, tilePosition.Y, drawTileSize, drawTileSize);
        }

        internal Point getTileDrawPosition(Vector2 position)
        {
            float x = position.X;
            float y = position.Y;
            return new Point((int)(x * drawTileSize + ((x + 1) * drawMarginSize)) + drawPosition.X, (int)(y * drawTileSize + ((y + 1) * drawMarginSize)) + drawPosition.Y);
        }

        public void changeScreenSize()
        {
            calculateSizes();
        }

        #endregion

        #region drawing

        private void drawRectangle(SpriteBatch b, Rectangle rect, Color color)
        {
            b.Draw(PyUtils.getWhitePixel(), rect, color);
        }

        private Texture2D getBackground(SpriteBatch b)
        {
            RenderTarget2D target = new RenderTarget2D(Game1.graphics.GraphicsDevice, viewport.Width + backgroundTileSize, viewport.Height + backgroundTileSize);
            Game1.graphics.GraphicsDevice.SetRenderTarget(target);

            b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            bool alternate = false;
            for (int x = 0; x < backgroundW; x++)
            {
                for (int y = 0; y < backgroundH; y++)
                {
                    drawRectangle(b, new Rectangle((int)(x * backgroundTileSize + backgoundPosition.X * backgroundSteps), (int)(y * backgroundTileSize + backgoundPosition.Y * backgroundSteps), backgroundTileSize, backgroundTileSize), alternate ? backGroundColor2 : backGroundColor);
                    alternate = !alternate;
                }
                if (backgroundW % 2 != 0)
                    alternate = !alternate;
            }
            b.End();
            Game1.game1.GraphicsDevice.SetRenderTarget(null);
            Game1.game1.GraphicsDevice.Clear(Color.Black);

            return target;
        }

        private void drawBackground(SpriteBatch b)
        {
            b.Draw(background, new Rectangle((int) backgoundPosition.X * backgroundSteps, (int) backgoundPosition.Y * backgroundSteps, background.Width, background.Height),Color.White);
        }

        private void drawBoard(SpriteBatch b)
        {
            drawRectangle(b, boardSquare, boardColor);

            foreach (Point tile in tileBackgrounds)
                drawTile(b, tile.toVector2(), tileSpaceBackground);
        }

        private Rectangle drawTile(SpriteBatch b, Vector2 position, Color color)
        {
            Rectangle tileSquare = getTileSquare(position);
            drawRectangle(b, tileSquare, color);
            return tileSquare;
        }

        private void drawTiles(SpriteBatch b)
        {
            foreach (Tile tile in Tiles)
            {
                if (tile.value > 0 && drawTile(b, tile.position, tile.Color) is Rectangle rec)
                    drawNumber(b, tile.PowValue, rec);
            }
        }


        private void drawNumber(SpriteBatch b, int number, Rectangle tileSquare)
        {
            string text = number.ToString();
            Vector2 fontSize = font.MeasureString(text);
            float fontScale = Math.Min(minfontScale, drawTileSize / fontSize.X * textscale);

            int fontWidth = (int)(fontScale * fontSize.X);
            int fontHeight = (int)(fontScale * fontSize.Y);

            int x = ((drawTileSize - fontWidth) / 2) + tileSquare.X;
            int y = ((drawTileSize - fontHeight) / 2) + tileSquare.Y;

            b.DrawString(Game1.dialogueFont, text, new Vector2(x, y), textColor, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 1f);
        }

        public void drawMenu(SpriteBatch b)
        {

            string text = score.ToString();
            Vector2 size = font.MeasureString(text);
            float scale = 3;
            b.DrawString(font, text, new Vector2(Game1.viewport.Width - 20 - size.X * scale, 20), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);

            if (paused)
            {
                drawRectangle(b, viewport, Color.Black * 0.5f);

                text = "2048";
                size = font.MeasureString(text);
                scale = 8;
                float y = 0;

                string text1 = "by Platonymous";
                float y1 = 10 + y + size.Y * scale;
                Vector2 size1 = font.MeasureString(text1);
                float scale1 = 1f;

                string text2 = "Start/Pause: SPACE Restart: ESC";
                float y2 = 10 + y1 + size.Y * scale1;
                Vector2 size2 = font.MeasureString(text2);

                string text3 = "Control: ARROW KEYS";
                float y3 = 10 + y2 + size.Y * scale1;
                Vector2 size3 = font.MeasureString(text3);

                string text4 = "Exit: Q";
                float y4 = 10 + y3 + size.Y * scale1;
                Vector2 size4 = font.MeasureString(text4);

                float height = y4 + size.Y * scale1;
                float offset = (Game1.viewport.Height - height) / 2;

                b.DrawString(font, text, new Vector2((Game1.viewport.Width - size.X * scale) / 2, offset + y), Color.GhostWhite, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
                b.DrawString(font, text1, new Vector2((Game1.viewport.Width - size1.X * scale1) / 2, offset + y1), Color.GhostWhite, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text2, new Vector2((Game1.viewport.Width - size2.X * scale1) / 2, offset + y2), Color.GhostWhite, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text3, new Vector2((Game1.viewport.Width - size3.X * scale1) / 2, offset + y3), Color.GhostWhite, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
                b.DrawString(font, text4, new Vector2((Game1.viewport.Width - size4.X * scale1) / 2, offset + y4), Color.GhostWhite, 0f, Vector2.Zero, scale1, SpriteEffects.None, 1f);
            }
        }

        public void draw(SpriteBatch b)
        {
            if(background == null)
                background = getBackground(b);

            b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            drawBackground(b);
            drawBoard(b);
            drawTiles(b);
            drawMenu(b);
            b.End();
        }


        #endregion

        #region update

        public bool tick(GameTime time)
        {
            backgoundPosition -= Vector2.One;
            if (Math.Abs(backgoundPosition.X * backgroundSteps) >= backgroundTileSize)
                backgoundPosition = Vector2.Zero;
            
            if (inMoveCycle && (moveX != 0 || moveY != 0))
            {
                if (moveX != 0)
                    Tiles.Sort((x, y) => -1 * (int) moveX * x.position.X.CompareTo(y.position.X));

                if (moveY != 0)
                    Tiles.Sort((x, y) => -1 * (int)moveY * x.position.Y.CompareTo(y.position.Y));


                switch (moveY)
                {
                    case 1: Tiles.Sort((x, y) => x.position.Y.CompareTo(y.position.Y)); break;
                    case -1: Tiles.Sort((x, y) => -1 * x.position.Y.CompareTo(y.position.Y)); break;
                    default:break;
                }

                inMoveCycle = false;
                foreach (Tile tile in Tiles)
                {
                    tile.SetMovement(moveX, moveY);
                    inMoveCycle =  tile.Move(this) || inMoveCycle;
                }

                if (!inMoveCycle)
                {
                    Tiles.useAll(t => { t.isMoving = true; t.hasmerged = false; });
                    spawnRandomTile();
                }
            }
            else
            {
                foreach (Tile tile in Tiles)
                {
                    tile.SetMovement(0, 0);
                    tile.Move(this);
                }
            }

            Tiles.RemoveAll(t => t.value == 0);
            
            return quit;
        }

        #endregion

        #region control

        private void pause()
        {
            paused = !paused;
        }

        public void leftClickHeld(int x, int y)
        {
        }

        public string minigameId()
        {
            return "Platonymous.2048";
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
            if(!inMoveCycle && !paused)
                switch (k)
                {
                    case Keys.Left: inMoveCycle = true; moveX = -1f * movementScale; moveY = 0; break;
                    case Keys.Right: inMoveCycle = true; moveX = 1f * movementScale; moveY = 0; break;
                    case Keys.Down: inMoveCycle = true; moveX = 0; moveY = 1f * movementScale; break;
                    case Keys.Up: inMoveCycle = true; moveX = 0; moveY = -1f * movementScale; break;
                    default:break;
                }
        }

        public void receiveKeyRelease(Keys k)
        {
            if (k.Equals(Keys.Escape))
                setupBoard();

            if (k.Equals(Keys.Space))
                if (gameover)
                    setupBoard();
                else
                    pause();

            if (k.Equals(Keys.Q))
                quit = true;

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
      
        public void unload()
        {

        }

        #endregion
    }
}
