using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using PyTK.Extensions;
using StardewValley;
using StardewValley.Minigames;

namespace ArcadeParachute
{
    public class GameParachute : IMinigame
    {
        public float pixelScale = 4f;
        protected Vector2 _shakeOffset = Vector2.Zero;
        protected string cutsceneText = "";
        public Point[] goalPositions = new Point[4] { new Point(83, 396), new Point(366, 396), new Point(410, 396), new Point(763, 399) };
        public bool[,] collisionMap = new bool[200, 250];
        public float frameTime = 0.03333334f;
        public GameParachute.GameStates gameState;
        public float shakeMagnitude;
        public Matrix transformMatrix;
        public bool gamePaused;
        public Vector2 upperLeft;
        private int screenWidth;
        private int screenHeight;
        private float screenDarkness;
        public float fadeDelta;
        public int currentLevel;
        private Texture2D texture;
        protected double _totalTime;
        private List<GameParachute.Entity> _entities;
        public float screenLeftBound;
        public float stateTimer;
        public Rectangle levelArtRect;
        private List<KeyValuePair<string, int>> _currentHighScores;
        private int currentHighScore;
        public GameParachute.Player player;
        public float frameAccumulator;
        public int moveLeft;
        public int moveRight;
        public int moveUp;
        public int moveDown;

        public double totalTime
        {
            get
            {
                return this._totalTime;
            }
        }

        public double totalTimeMS
        {
            get
            {
                return this._totalTime * 1000.0;
            }
        }

        public GameParachute()
        {
            this._entities = new List<GameParachute.Entity>();
            this.changeScreenSize();
            this.texture = ArcadeParachuteMod._instance.Helper.Content.Load<Texture2D>("assets\\Parachute.png");
            Game1.changeMusicTrack("movie_wumbus", false, Game1.MusicContext.MiniGame);
            this.player = new GameParachute.Player();
            this.SetupLevel(0);
        }

        public virtual void LevelComplete()
        {
            this.SetupLevel((this.currentLevel + 1) % 30);
        }

        public virtual void SetupLevel(int level)
        {
            this.currentLevel = level;
            this._entities.Clear();
            this.AddEntity<GameParachute.Player>(this.player);
            this.player.Reset();
            this.levelArtRect = new Rectangle(level % 10 * 200, level / 10 * 250 + 160, 200, 250);
            Color[] data = new Color[this.levelArtRect.Width * this.levelArtRect.Height];
            this.texture.GetData<Color>(0, new Rectangle?(this.levelArtRect), data, 0, data.Length);
            this.collisionMap = new bool[this.levelArtRect.Width, this.levelArtRect.Height];
            for (int index1 = 0; index1 < this.levelArtRect.Width; ++index1)
            {
                for (int index2 = 0; index2 < this.levelArtRect.Height; ++index2) {

                    Color c = data[index1 + index2 * this.levelArtRect.Width];
                    Utility.RGBtoHSL(c.R, c.G, c.B, out double h, out double s, out double l);
                    this.collisionMap[index1, index2] = c.A > (byte)100 && (h < 189 || h > 201) ;
                }
            }
            if (level >= this.goalPositions.Length)
                return;
            this.AddEntity<GameParachute.Goal>(new GameParachute.Goal(this.goalPositions[this.currentLevel].X - this.levelArtRect.Left, this.goalPositions[this.currentLevel].Y - this.levelArtRect.Top));
        }

        public virtual bool CollidesWithLevel(Rectangle rect)
        {
            for (int index1 = Math.Max(0, rect.Left); index1 <= Math.Min(rect.Right, this.levelArtRect.Width - 1); ++index1)
            {
                for (int index2 = Math.Max(0, rect.Top); index2 <= Math.Min(rect.Bottom, this.levelArtRect.Height - 1); ++index2)
                {
                    if (this.collisionMap[index1, index2])
                        return true;
                }
            }
            return false;
        }

        public void RefreshHighScore()
        {
            this._currentHighScores = Game1.player.team.junimoKartScores.GetScores();
            this.currentHighScore = 0;
            if (this._currentHighScores.Count <= 0)
                return;
            this.currentHighScore = this._currentHighScores[0].Value;
        }

        public virtual T AddEntity<T>(T new_entity) where T : GameParachute.Entity
        {
            this._entities.Add((GameParachute.Entity)new_entity);
            new_entity.Initialize(this);
            return new_entity;
        }

        public bool overrideFreeMouseMovement()
        {
            return Game1.options.SnappyMenus;
        }

        public void UpdateInput()
        {
            if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton))
            {
                if ((double)this.frameTime < 0.0)
                    this.moveLeft = 1;
                else if ((double)this.frameAccumulator >= (double)this.frameTime)
                    ++this.moveLeft;
            }
            else
                this.moveLeft = this.moveLeft <= 0 ? 0 : -1;
            if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton))
            {
                if ((double)this.frameTime < 0.0)
                    this.moveRight = 1;
                else if ((double)this.frameAccumulator >= (double)this.frameTime)
                    ++this.moveRight;
            }
            else
                this.moveRight = this.moveRight <= 0 ? 0 : -1;
            if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton))
            {
                if ((double)this.frameTime < 0.0)
                    this.moveUp = 1;
                else if ((double)this.frameAccumulator >= (double)this.frameTime)
                    ++this.moveUp;
            }
            else
                this.moveUp = this.moveUp <= 0 ? 0 : -1;
            if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton))
            {
                if ((double)this.frameTime < 0.0)
                {
                    this.moveDown = 1;
                }
                else
                {
                    if ((double)this.frameAccumulator < (double)this.frameTime)
                        return;
                    ++this.moveDown;
                }
            }
            else if (this.moveDown > 0)
                this.moveDown = -1;
            else
                this.moveDown = 0;
        }

        public virtual bool CanPause()
        {
            return this.gameState == GameParachute.GameStates.Ingame || this.gameState == GameParachute.GameStates.FruitsSummary || (this.gameState == GameParachute.GameStates.Cutscene || this.gameState == GameParachute.GameStates.Map);
        }

        public bool tick(GameTime time)
        {
            float num = (float)time.ElapsedGameTime.TotalSeconds;
            if (this.gamePaused)
                num = 0.0f;
            if (!this.CanPause())
                this.gamePaused = false;
            this.shakeMagnitude = Utility.MoveTowards(this.shakeMagnitude, 0.0f, num * 3f);
            this._totalTime += (double)num;
            this.screenDarkness += this.fadeDelta * num;
            if ((double)this.screenDarkness < 0.0)
                this.screenDarkness = 0.0f;
            if ((double)this.screenDarkness > 1.0)
                this.screenDarkness = 1f;
            this.frameAccumulator += num;
            this.UpdateInput();
            for (; (double)this.frameAccumulator >= (double)this.frameTime; this.frameAccumulator -= this.frameTime)
            {
                for (int index = 0; index < this._entities.Count; ++index)
                {
                    if (this._entities[index] != null && this._entities[index].IsActive())
                        this._entities[index].Update(this.frameTime);
                }
            }
            for (int index = 0; index < this._entities.Count; ++index)
            {
                if (this._entities[index] != null && this._entities[index].ShouldReap())
                {
                    this._entities.RemoveAt(index);
                    --index;
                }
            }
            return false;
        }

        public void UpdateScoreState()
        {
        }

        public void Die()
        {
        }

        public void receiveLeftClick(int x, int y, bool playSound = true)
        {
        }

        public void releaseLeftClick(int x, int y)
        {
        }

        public void releaseRightClick(int x, int y)
        {
        }

        public void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public void receiveKeyPress(Keys k)
        {
            var input = (InputState) typeof(Game1).GetField("input",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
            if (input.GetGamePadState().IsButtonDown(Buttons.Back) || k.Equals((object)Keys.Escape))
            {
                this.QuitGame();
            }
            else
            {
                if (!k.Equals((object)Keys.P) && !k.Equals((object)Keys.Enter) && (!Game1.options.gamepadControls || (!input.GetGamePadState().IsButtonDown(Buttons.Start) || !this.CanPause())))
                    return;
                this.gamePaused = !this.gamePaused;
                if (this.gamePaused)
                    Game1.playSound("bigSelect");
                else
                    Game1.playSound("bigDeSelect");
            }
        }

        public void receiveKeyRelease(Keys k)
        {
        }

        public void ResetState()
        {
            this.screenLeftBound = 0.0f;
        }

        public void QuitGame()
        {
            this.unload();
            Game1.playSound("bigDeSelect");
            Game1.currentMinigame = (IMinigame)null;
        }

        public T GetOverlap<T>(GameParachute.ICollideable source) where T : GameParachute.Entity
        {
            List<T> objList = new List<T>();
            Rectangle bounds1 = source.GetBounds();
            foreach (GameParachute.Entity entity in this._entities)
            {
                if (entity.IsActive() && entity is GameParachute.ICollideable && entity is T)
                {
                    Rectangle bounds2 = (entity as GameParachute.ICollideable).GetBounds();
                    if (bounds1.Intersects(bounds2))
                        return entity as T;
                }
            }
            return default(T);
        }

        public List<T> GetOverlaps<T>(GameParachute.ICollideable source) where T : GameParachute.Entity
        {
            List<T> objList = new List<T>();
            Rectangle bounds1 = source.GetBounds();
            foreach (GameParachute.Entity entity in this._entities)
            {
                if (entity.IsActive() && entity is GameParachute.ICollideable && entity is T)
                {
                    Rectangle bounds2 = (entity as GameParachute.ICollideable).GetBounds();
                    if (bounds1.Intersects(bounds2))
                        objList.Add(entity as T);
                }
            }
            return objList;
        }

        public void draw(SpriteBatch b)
        {
            this._shakeOffset = new Vector2(Utility.Lerp(-this.shakeMagnitude, this.shakeMagnitude, (float)Game1.random.NextDouble()), Utility.Lerp(-this.shakeMagnitude, this.shakeMagnitude, (float)Game1.random.NextDouble()));
            if (this.gamePaused)
                this._shakeOffset = Vector2.Zero;
            Rectangle scissorRectangle = b.GraphicsDevice.ScissorRectangle;
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, new RasterizerState()
            {
                ScissorTestEnable = true
            });
            Rectangle screen = Utility.ConstrainScissorRectToScreen(new Rectangle((int)this.upperLeft.X, (int)this.upperLeft.Y, (int)((double)this.screenWidth * (double)this.pixelScale), (int)((double)this.screenHeight * (double)this.pixelScale)));
            b.GraphicsDevice.ScissorRectangle = screen;
            b.Draw(Game1.staminaRect, this.TransformDraw(new Rectangle(0, 0, this.screenWidth, this.screenHeight)), new Rectangle?(), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            b.Draw(this.texture, this.TransformDraw(new Rectangle(0, 0, this.screenWidth, this.screenHeight)), new Rectangle?(this.levelArtRect), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.01f);
            foreach (GameParachute.Entity entity in this._entities)
                entity.Draw(b);
            if (!this.gamePaused && (this.gameState == GameParachute.GameStates.Ingame || this.gameState == GameParachute.GameStates.Cutscene || (this.gameState == GameParachute.GameStates.FruitsSummary || this.gameState == GameParachute.GameStates.Map)))
            {
                this._shakeOffset = Vector2.Zero;
                //new Vector2(4f, 4f) { X = 4f }.Y += 18f;
            }
            if ((double)this.screenDarkness > 0.0)
                b.Draw(Game1.staminaRect, this.TransformDraw(new Rectangle(0, 0, this.screenWidth, this.screenHeight)), new Rectangle?(), Color.Black * this.screenDarkness, 0.0f, Vector2.Zero, SpriteEffects.None, 0.145f);
            if (!Game1.options.hardwareCursor && !Game1.options.gamepadControls)
                b.Draw(Game1.mouseCursors, new Vector2((float)Game1.getOldMouseX(), (float)Game1.getOldMouseY()), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.gamepadControls ? 44 : 0, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)(4.0 + (double)Game1.dialogueButtonScale / 150.0), SpriteEffects.None, 0.0001f);
            b.End();
            b.GraphicsDevice.ScissorRectangle = scissorRectangle;
        }

        public float GetPixelScale()
        {
            return this.pixelScale;
        }

        public Rectangle TransformDraw(Rectangle dest)
        {
            dest.X = (int)Math.Round(((double)dest.X + (double)this._shakeOffset.X) * (double)this.pixelScale) + (int)this.upperLeft.X;
            dest.Y = (int)Math.Round(((double)dest.Y + (double)this._shakeOffset.Y) * (double)this.pixelScale) + (int)this.upperLeft.Y;
            dest.Width = (int)((double)dest.Width * (double)this.pixelScale);
            dest.Height = (int)((double)dest.Height * (double)this.pixelScale);
            return dest;
        }

        public static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public Vector2 TransformDraw(Vector2 dest)
        {
            dest.X = (float)((int)Math.Round(((double)dest.X + (double)this._shakeOffset.X) * (double)this.pixelScale) + (int)this.upperLeft.X);
            dest.Y = (float)((int)Math.Round(((double)dest.Y + (double)this._shakeOffset.Y) * (double)this.pixelScale) + (int)this.upperLeft.Y);
            return dest;
        }

        public void changeScreenSize()
        {
            this.screenWidth = 200;
            this.screenHeight = 250;
            float num1 = 1f / Game1.options.zoomLevel;
            Viewport viewport1 = Game1.graphics.GraphicsDevice.Viewport;
            double num2 = (double)(viewport1.Width / this.screenWidth) * (double)num1;
            viewport1 = Game1.graphics.GraphicsDevice.Viewport;
            double num3 = (double)(viewport1.Height / this.screenHeight) * (double)num1;
            this.pixelScale = (float)Math.Min(5, (int)Math.Floor((double)Math.Min((float)num2, (float)num3)));
            Viewport viewport2 = Game1.graphics.GraphicsDevice.Viewport;
            double num4 = (double)(viewport2.Width / 2) * (double)num1;
            viewport2 = Game1.graphics.GraphicsDevice.Viewport;
            double num5 = (double)(viewport2.Height / 2) * (double)num1;
            this.upperLeft = new Vector2((float)num4, (float)num5);
            this.upperLeft.X -= (float)(this.screenWidth / 2) * this.pixelScale;
            this.upperLeft.Y -= (float)(this.screenHeight / 2) * this.pixelScale;
        }

        public void unload()
        {
            Game1.stopMusicTrack(Game1.MusicContext.MiniGame);
            Game1.player.team.junimoKartStatus.WithdrawState();
            Game1.player.faceDirection(0);
        }

        public bool forceQuit()
        {
            this.unload();
            return true;
        }

        public void leftClickHeld(int x, int y)
        {
        }

        public void receiveEventPoke(int data)
        {
            throw new NotImplementedException();
        }

        public string minigameId()
        {
            return "MineCart";
        }

        public bool doMainGameUpdates()
        {
            return false;
        }

        public enum GameStates
        {
            Title,
            Ingame,
            FruitsSummary,
            Map,
            Cutscene,
        }

        public class Entity
        {
            public bool visible = true;
            public bool enabled = true;
            public Vector2 position;
            protected GameParachute _game;
            protected bool _destroyed;

            public Vector2 drawnPosition
            {
                get
                {
                    return this.position - new Vector2(this._game.screenLeftBound, 0.0f);
                }
            }

            public virtual void OnPlayerReset()
            {
            }

            public bool IsActive()
            {
                return !this._destroyed && this.enabled;
            }

            public void Initialize(GameParachute game)
            {
                this._game = game;
                this._Initialize();
            }

            public void Destroy()
            {
                this._destroyed = true;
            }

            protected virtual void _Initialize()
            {
            }

            public virtual bool ShouldReap()
            {
                return this._destroyed;
            }

            public void Draw(SpriteBatch b)
            {
                if (this._destroyed || !this.visible || !this.enabled)
                    return;
                this._Draw(b);
            }

            public virtual void _Draw(SpriteBatch b)
            {
            }

            public void Update(float time)
            {
                if (this._destroyed || !this.enabled)
                    return;
                this._Update(time);
            }

            protected virtual void _Update(float time)
            {
            }
        }

        public class Goal : GameParachute.Entity, GameParachute.ICollideable
        {
            public override void _Draw(SpriteBatch b)
            {
                b.Draw(this._game.texture, this._game.TransformDraw(this.drawnPosition), new Rectangle?(new Rectangle(0, 38, 31, 10)), Color.White, 0.0f, new Vector2(0.0f, 0.0f), this._game.GetPixelScale(), SpriteEffects.None, 1f);
            }

            public Goal(int x, int y)
            {
                this.position.X = (float)x;
                this.position.Y = (float)y;
            }

            public Rectangle GetLocalBounds()
            {
                return new Rectangle(0, 0, 38, 10);
            }

            public Rectangle GetBounds()
            {
                Rectangle localBounds = this.GetLocalBounds();
                localBounds.X += (int)this.position.X;
                localBounds.Y += (int)this.position.Y;
                return localBounds;
            }
        }

        public class Player : GameParachute.Entity, GameParachute.ICollideable
        {
            public float freefallAccel = 0.3f;
            public float parachuteTopSpeed = 2f;
            public float terminalVelocity = 10f;
            public float maxHorizontalSpeed = 5f;
            public bool startedMoving;
            public bool parachuteDeployed;
            public int currentFrame;
            public Vector2 velocity;
            private float rotation;

            protected override void _Initialize()
            {
                base._Initialize();
                this.Reset();
            }

            protected override void _Update(float time)
            {
                base._Update(time);
                if (!this.startedMoving)
                {
                    if (this._game.moveLeft != 1 && this._game.moveRight != 1 && (this._game.moveUp != 1 && this._game.moveDown != 1))
                        return;
                    this.startedMoving = true;
                }
                if (this._game.moveUp == 1 && !this.parachuteDeployed)
                {
                    Game1.playSound("trashcan");
                    this.parachuteDeployed = true;
                }
                if (this.parachuteDeployed)
                {
                    if (this.currentFrame < 6)
                    {
                        ++this.currentFrame;
                        if (this.currentFrame >= 6)
                            this.currentFrame = 6;
                    }
                    if ((double)this.velocity.Y > (double)this.parachuteTopSpeed)
                        this.velocity.Y -= this.freefallAccel;
                    else if ((double)this.velocity.Y < (double)this.parachuteTopSpeed)
                        this.velocity.Y += this.freefallAccel;
                }
                else if ((double)this.velocity.Y < (double)this.terminalVelocity)
                    this.velocity.Y += this.freefallAccel;
                if (this._game.moveDown > 0 && (double)this.velocity.Y <= 5.0)
                    ++this.velocity.Y;
                if (this._game.moveLeft > 0)
                {
                    if (this.parachuteDeployed)
                    {
                        if ((double)this.velocity.X >= -(double)this.maxHorizontalSpeed)
                            this.velocity.X -= 0.5f;
                        this.rotation = Math.Max(this.rotation - 0.5f, -9f);
                    }
                    else if ((double)this.velocity.X >= -(double)this.maxHorizontalSpeed + 2.0)
                        --this.velocity.X;
                }
                if (this._game.moveRight > 0)
                {
                    if (this.parachuteDeployed)
                    {
                        if ((double)this.velocity.X <= (double)this.maxHorizontalSpeed)
                            this.velocity.X += 0.5f;
                        this.rotation = Math.Min(this.rotation + 0.5f, 9f);
                    }
                    else if ((double)this.velocity.X <= (double)this.maxHorizontalSpeed - 2.0)
                        ++this.velocity.X;
                }
                if (this._game.GetOverlap<GameParachute.Goal>((GameParachute.ICollideable)this) != null)
                {
                    this._game.LevelComplete();
                }
                else
                {
                    if (this._game.CollidesWithLevel(this.GetBounds()))
                        this.Reset();
                    this.position = this.position + this.velocity * 0.5f;
                    this.position.X = Utility.Clamp(this.position.X, (float)(this.GetLocalBounds().Width / 2), (float)(this._game.screenWidth - this.GetLocalBounds().Width / 2));
                    if ((double)this.position.Y <= (double)(this._game.screenHeight + 34))
                        return;
                    this.Reset();
                }
            }

            public virtual void Reset()
            {
                this.velocity = Vector2.Zero;
                this.parachuteDeployed = false;
                this.startedMoving = false;
                this.currentFrame = 0;
                this.rotation = 0.0f;
                this.position = new Vector2(100f, (float)(-this.GetLocalBounds().Height / 2));
            }

            public override void _Draw(SpriteBatch b)
            {
                b.Draw(this._game.texture, this._game.TransformDraw(this.drawnPosition), new Rectangle?(new Rectangle(32 * this.currentFrame, 0, 32, 35)), Color.White, this.rotation * ((float)Math.PI / 180f), new Vector2(16f, 16f), this._game.GetPixelScale(), SpriteEffects.None, 1f);
            }

            public Rectangle GetLocalBounds()
            {
                if (this.parachuteDeployed)
                    return new Rectangle(-4, -10, 8, 28);
                return new Rectangle(-4, 8, 8, 10);
            }

            public Rectangle GetBounds()
            {
                Rectangle localBounds = this.GetLocalBounds();
                localBounds.X += (int)this.position.X;
                localBounds.Y += (int)this.position.Y;
                return localBounds;
            }
        }

        public interface ICollideable
        {
            Rectangle GetLocalBounds();

            Rectangle GetBounds();
        }
    }
}
