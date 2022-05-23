using ChessBoard.Pieces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PyTK;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessBoard
{
    public class ChessGame : IMinigame
    {
        public static bool Quit { get; set; } = false;
        public int TilesSize { get; set; } = 64;
        public Color WhiteTileColor { get; set; } = Color.LightGray;
        public Color BlackTileColor { get; set; } = Color.DarkGray;
        public Texture2D BoardImage { get; set; } = null;

        public List<ChessPiece> Pile { get; set; } = new List<ChessPiece>();
        public Rectangle BoardBounds { get; set; } = new Rectangle(0, 0, 256, 256);

        public int BoardSize { get; set; } = 256;
        public Texture2D Structure { get; set; } = null;
        public Point Position { get; set; } = Point.Zero;

        public int boardTextureSize = 1280;
        public int tileTextureSize => boardTextureSize / 8;
        public Texture2D BackgroundBorder;
        public Texture2D BackgroundShade;
        public Texture2D HoverTileImage;
        const int backGroundTileSize = 64;
        private int backgroundPos = 0;

        bool turnKingInDanger = false;

        bool WhitesTurn = true;

        public ChessPiece selectedPiece = null;

        private Vector2? hoverTile = null;

        public List<ChessPiece> Board = new List<ChessPiece>();
        private IModHelper Helper;
        internal ITranslationHelper i18n => Helper.Translation;

        public static string blackPlayerDefault = "Black";
        public static string whitePlayerDefault = "White";


        public string WhitePlayer { get; set; } = whitePlayerDefault;
        public string BlackPlayer { get; set; } = blackPlayerDefault;

        public string Id { get; set; }

        public static Texture2D Pixel = PyDraw.getWhitePixel();

        public static SaveData SavedGameData = new SaveData();

        public string Winner = null;
        public Session GameSession { get; set; } = null;

        public bool Open = false;

        public Dictionary<bool, ChessPiece> Kings = new Dictionary<bool, ChessPiece>() { { true, null }, { false, null } };

        public Question GameQuestion = null;

        public Dictionary<string, Question> QuestionPool = new Dictionary<string, Question>();

        public Point[] temp = new Point[2];

        public bool Surrendered = false;
        public string LastWinner = null;

        public bool SpectatorMode = false;

        #region start

        public ChessGame(string id, bool open, IModHelper helper)
        {
            Id = id;
            Helper = helper;
            Quit = false;
            Open = open;
            setupBoard();

            if (SavedGameData.Sessions.ContainsKey(id))
                GameSession = SavedGameData.Sessions[id];
            else
                GameSession = new Session(id, open);

            reset(true);
        }

        public string I18n(string key)
        {
            return Helper.Translation.Get(key);
        }

        public void updateSession()
        {
            if (SavedGameData.Sessions.ContainsKey(Id))
                GameSession = SavedGameData.Sessions[Id];

            reset(true);
        }

        internal void setupBoard()
        {
            calculateSizes();

            Texture2D whiteTile = PyDraw.getRectangle(tileTextureSize, tileTextureSize, WhiteTileColor);
            Texture2D blackTile = PyDraw.getRectangle(tileTextureSize, tileTextureSize, BlackTileColor);
            BoardImage = PyDraw.getPattern(boardTextureSize, boardTextureSize, whiteTile, blackTile);
            BoardBounds = new Rectangle(Position.X, Position.Y, BoardSize, BoardSize);
            Structure = PyDraw.getPattern(boardTextureSize, boardTextureSize, Game1.content.Load<Texture2D>(@"LooseSprites//Cursors").getArea(new Rectangle(527,0,100,96)));
            Color[] bgColors = new[] { Color.LightGray, Color.White };
            BackgroundShade = PyDraw.getPattern(backGroundTileSize * 8, backGroundTileSize * 8, PyDraw.getRectangle(backGroundTileSize, backGroundTileSize,bgColors[0]), PyDraw.getRectangle(backGroundTileSize, backGroundTileSize, bgColors[1]));
            BackgroundBorder = PyDraw.getRectangle(BoardBounds.Width + 20, BoardBounds.Height + 20, Color.White);
            HoverTileImage = PyDraw.getRectangle(TilesSize, TilesSize, Color.White);
        }

        public static void LoadTextures(IModHelper helper)
        {
            helper.GameContent.Load<Texture2D>("Characters/Monsters/Skeleton").getArea(new Rectangle(0, 64, 64, 32)).inject(@"PlatonymousChess/Rook_White");
            helper.GameContent.Load<Texture2D>("Characters/Monsters/Shadow Brute").getArea(new Rectangle(0, 0, 64, 32)).inject(@"PlatonymousChess/Rook_Black");

            helper.GameContent.Load<Texture2D>("Animals/White Cow").getArea(new Rectangle(0, 64, 128, 32)).inject(@"PlatonymousChess/Knight_White");
            helper.GameContent.Load<Texture2D>("Animals/Brown Cow").getArea(new Rectangle(0, 0, 128, 32)).inject(@"PlatonymousChess/Knight_Black");

            helper.GameContent.Load<Texture2D>("Characters/Junimo").getArea(new Rectangle(0, 64, 128, 16)).inject(@"PlatonymousChess/Pawn_White");
            helper.GameContent.Load<Texture2D>("Characters/Junimo").getArea(new Rectangle(0, 0, 128, 16)).inject(@"PlatonymousChess/Pawn_Black");

            helper.GameContent.Load<Texture2D>("Animals/White Chicken").getArea(new Rectangle(0, 32, 64, 16)).inject(@"PlatonymousChess/Bishop_White");
            helper.GameContent.Load<Texture2D>("Animals/Void Chicken").getArea(new Rectangle(0, 96, 64, 16)).inject(@"PlatonymousChess/Bishop_Black");

            helper.GameContent.Load<Texture2D>($"Characters/{ChessBoardMod.config.WhiteQueen}").getArea(new Rectangle(0, 64, 64, 32)).inject(@"PlatonymousChess/Queen_White");
            helper.GameContent.Load<Texture2D>($"Characters/{ChessBoardMod.config.BlackQueen}").getArea(new Rectangle(0, 0, 64, 32)).inject(@"PlatonymousChess/Queen_Black");

            helper.GameContent.Load<Texture2D>($"Characters/{ChessBoardMod.config.WhiteKing}").getArea(new Rectangle(0, 64, 64, 32)).inject(@"PlatonymousChess/King_White");
            helper.GameContent.Load<Texture2D>($"Characters/{ChessBoardMod.config.BlackKing}").getArea(new Rectangle(0, 0, 64, 32)).inject(@"PlatonymousChess/King_Black");
        }

        public void loadTurns()
        {
            foreach (var v in GameSession.Turns)
            {
                if(v.Key.X == -2)
                {
                    Surrendered = true;
                    Winner = v.Key.Y == 0 ? WhitePlayer : BlackPlayer;
                    SavedGameData.Sessions.Remove(GameSession.Id);
                    break;
                }

                var piece = Board.Find(c => c.Position == v.Key.toVector2());

                if (piece is King k)
                    k.canCastle = false;
                else if (piece is Rook r)
                    r.canCastle = false;

                if (piece is Rook && v.Value.X == -1)
                    performCastling(piece.Position);
                else
                {
                    if (piece is Pawn && v.Value.X == -1)
                    {
                        var p = new Point((int)piece.Position.X, (int)piece.Position.Y + (piece.White ? -1 : 1));
                        if (Board.Find(c => c.Position == v.Value.toVector2()) is ChessPiece pc)
                        {
                            Board.Remove(pc);
                            Pile.Add(pc);
                        }

                        piece.Position = p.toVector2();
                        promotePawn(piece.Position, v.Value.Y == 0);
                    }
                    else
                    {
                        if (Board.Find(c => c.Position == v.Value.toVector2()) is ChessPiece cp)
                        {
                            Board.Remove(cp);
                            Pile.Add(cp);
                        }
                        piece.Position = v.Value.toVector2();
                    }
                }
            }
        }

        public void reset(bool load)
        {
            GameQuestion = null;
            Winner = null;
            Surrendered = false;
            Board.Clear();
            Board.Add(new Rook(true, new Vector2(0, 7), Helper));
            Board.Add(new Knight(true, new Vector2(1, 7), Helper));
            Board.Add(new Bishop(true, new Vector2(2, 7), Helper));
            Board.Add(new Queen(true, new Vector2(3, 7), Helper));
            Kings[true] = new King(true, new Vector2(4, 7), Helper);
            Board.Add(Kings[true]);
            Board.Add(new Bishop(true, new Vector2(5, 7), Helper));
            Board.Add(new Knight(true, new Vector2(6, 7), Helper));
            Board.Add(new Rook(true, new Vector2(7, 7), Helper));

            Board.Add(new Rook(false, new Vector2(0, 0), Helper));
            Board.Add(new Knight(false, new Vector2(1, 0), Helper));
            Board.Add(new Bishop(false, new Vector2(2, 0), Helper));
            Board.Add(new Queen(false, new Vector2(3, 0), Helper));
            Kings[false] = new King(false, new Vector2(4, 0), Helper);
            Board.Add(Kings[false]);
            Board.Add(new Bishop(false, new Vector2(5, 0), Helper));
            Board.Add(new Knight(false, new Vector2(6, 0), Helper));
            Board.Add(new Rook(false, new Vector2(7, 0), Helper));

            for (int i = 0; i < 8; i++)
            {
                Board.Add(new Pawn(false, new Vector2(i, 1), Helper));
                Board.Add(new Pawn(true, new Vector2(i, 6), Helper));
            }

            if (load)
            {
                loadTurns();
                WhitesTurn = GameSession.NextTurnWhite;
                WhitePlayer = GameSession.WhitePlayer;
                BlackPlayer = GameSession.BlackPlayer;
                Open = GameSession.Open;

                if (!Open && !WhitesTurn && BlackPlayer == blackPlayerDefault && WhitePlayer != Game1.player.Name)
                    BlackPlayer = Game1.player.Name;

                if (!Open && BlackPlayer != blackPlayerDefault && BlackPlayer != Game1.player.Name && WhitePlayer != whitePlayerDefault && WhitePlayer != Game1.player.Name)
                    SpectatorMode = true;
            }
            setupQuestions();
            calculateMoves(Board);
            checkKing();
        }

        public void endGame() {
            SavedGameData.Sessions.Remove(GameSession.Id);
            quitGame();
        }

        public void surrender()
        {
            Winner = Game1.player.Name == WhitePlayer ? BlackPlayer : WhitePlayer;

            ChessBoardMod.SyncGameResult(new GameResult(WhitePlayer, BlackPlayer, Winner));
            nextTurn(new Point(-2, Winner == WhitePlayer ? 0 : 1),Point.Zero);
        }

        public void performCastling(Vector2 rookPos)
        {
            var rook = Board.Find(p => p.Position == rookPos);
            var king = Kings[rook.White];
            (king as King).canCastle = false;
            (rook as Rook).canCastle = false;
            int move = rookPos.X > king.Position.X ? 2 : -2;
            rook.Position = new Vector2(king.Position.X + move/2, king.Position.Y);
            king.Position = new Vector2(king.Position.X + move, king.Position.Y);
        }

        public bool isCastlingPossible(List<ChessPiece> board, Rook rook)
        {
            if (!rook.canCastle || !(Kings[rook.White] as King).canCastle || Kings[rook.White].Position.Y != rook.Position.Y)
                return false;

            int move = rook.Position.X > Kings[rook.White].Position.X ? 2 : -2;

            for (int x = (int)rook.Position.X; x != Kings[rook.White].Position.X; x-= move/2)
                if (board.Find(p => p.Position == new Vector2(x, rook.Position.Y)) is ChessPiece cp && cp != rook && cp != Kings[rook.White])
                    return false;

            ChessPiece tKing = null;
            if (getTestBoard(board, Kings[rook.White], Kings[rook.White].Position + new Vector2(move/2, 0), out tKing) is List<ChessPiece> tBoard && isTheKingInDanger(tBoard, rook.White, tKing))
                return false;

            if (getTestBoard(board, Kings[rook.White], Kings[rook.White].Position + new Vector2(move, 0), out tKing) is List<ChessPiece> teBoard && isTheKingInDanger(teBoard, rook.White, tKing))
                return false;

            return true;
        }

        public void pickCastling(string choice)
        {
            if (choice == "no")
                nextTurn(temp[0], temp[1]);
            else
                nextTurn(temp[0], new Point(-1, -1));
        }

        public void promotePawn(Vector2 pawnPos, bool queen)
        {
            var pawn = Board.Find(p => p.Position == pawnPos);
            if (queen)
                Board.Add(new Queen(pawn.White, pawn.Position, Helper));
            else
                Board.Add(new Knight(pawn.White, pawn.Position, Helper));
            Board.Remove(pawn);
        }

        public void pickPiece(string choice)
        {
            nextTurn(temp[0], new Point(-1, choice == "queen" ? 0 : 1));
        }

        public void setupQuestions()
        {
            LastWinner = Winner;
            QuestionPool = new Dictionary<string, Question>();
            QuestionPool.Add("waiting",new Question(I18n("waiting"), getClosingChoices()));
            QuestionPool.Add("spectator", new Question(I18n("spectator"), getClosingChoices()));

            QuestionPool.Add("surrender", new Question(I18n("surrenderWin") + " " + I18n("win").Replace("{Player}",Winner), getClosingChoices()));
            QuestionPool.Add("checkmate", new Question(I18n("checkmateWin") + " " + I18n("win").Replace("{Player}", Winner), getClosingChoices()));
            QuestionPool.Add("castling", new Question(I18n("castling"), new Choice("yes", I18n("yes"), pickCastling), new Choice("no", I18n("no"), pickCastling)));
            QuestionPool.Add("promotion", new Question(I18n("promotion"), new Choice("queen", I18n("queen"), pickPiece), new Choice("knight", I18n("knight"), pickPiece)));
        }

        public Choice[] getClosingChoices()
        {
            List<Choice> choices = new List<Choice>();
            choices.Add(new Choice("quit", I18n("close"), (s) => quitGame()));
            if (Winner == null)
            {
                if(!Open && BlackPlayer != blackPlayerDefault && !SpectatorMode)
                    choices.Add(new Choice("surrender", I18n("surrender"), (s) => surrender()));

                if (Open || BlackPlayer == blackPlayerDefault || (Game1.IsMasterGame && (!Game1.IsMultiplayer || Game1.otherFarmers.Where(f => f.Value.isActive() && f.Value.Name == WhitePlayer || f.Value.Name == BlackPlayer).Count() == 0)))
                    choices.Add(new Choice("abort", I18n("reset"), (s) => endGame()));
            }
            
            return choices.ToArray();
        }

        public bool hasMoves(List<ChessPiece>board, bool white)
        {
            foreach (var cp in board.Where(c => c.White == white))
                if (hasMoves(cp, board, white))
                    return true;

            return false;
        }

        public bool hasMoves(ChessPiece cp, List<ChessPiece> board, bool white)
        {
            ChessPiece testKing = null;
                if (cp.Moves.Count > 0 && cp.Moves.Where(p => p.X >= 0 && p.Y <= 7 && (getTestBoard(board, cp, p, out testKing) is List<ChessPiece> tBoard && !isTheKingInDanger(tBoard, white, testKing))).Count() > 0)
                    return true;

            return false;
        }

        public void nextTurn(Point from, Point to)
        {
            temp = new Point[2];
            selectedPiece = null;
            GameSession.WhitePlayer = WhitePlayer;
            GameSession.BlackPlayer = BlackPlayer;
            GameSession.Turns.Add(new ValueChangeRequest<Point,Point>(from, to, to));
            GameSession.NextTurnWhite = !WhitesTurn;
            SavedGameData.Sessions.AddOrReplace(GameSession.Id, GameSession);
            ChessBoardMod.SyncSession(GameSession);

            if (from.X == -2)
            {
                WhitesTurn = !WhitesTurn;
                reset(true);
            }
            else
            {
                Kings[WhitesTurn].Hightlight = Color.White;
                checkKing();
                reset(true);
            }
        }

        public void checkKing()
        {
            ChessPiece king = Kings[WhitesTurn];
            turnKingInDanger = isTheKingInDanger(Board, WhitesTurn);
            king.Hightlight = Color.White;
            if (turnKingInDanger)
            {
                bool movesAvailable = hasMoves(Board, WhitesTurn);
                if (movesAvailable)
                    king.Hightlight = Color.Yellow;
                else
                {
                    Winner = !king.White ? WhitePlayer : BlackPlayer;
                    king.Hightlight = Color.Red;

                    if (Winner == Game1.player.Name)
                        ChessBoardMod.SyncGameResult(new GameResult(WhitePlayer,BlackPlayer,Winner));

                    SavedGameData.Sessions.Remove(Id);
                }
            }
        }

        public void calculateMoves(List<ChessPiece> board)
        {
            foreach (var p in board)
                p.CalculatePossibleMoves(board);
        }

        #endregion

        #region screensize

        private void calculateSizes()
        {
            TilesSize = (Game1.viewport.Height / 12);
            BoardSize = TilesSize * 8;
            Position = new Point((Game1.viewport.Width - BoardSize) / 2, (Game1.viewport.Height - BoardSize) / 2);
        }


        #endregion

        #region drawing


        public void draw(SpriteBatch b)
        {
            Game1.mouseCursorTransparency = 0f;

            b.GraphicsDevice.Clear(Color.LightSkyBlue);
            b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            drawBackground(b);
            drawBoard(b);
            drawPieces(b, Open ? false : BlackPlayer == blackPlayerDefault);
            drawUI(b);
            b.End();
        }

        public void drawPieces(SpriteBatch b, bool white)
        {
            foreach (ChessPiece piece in Board.OrderBy(k => k.Position.Y).Where(cp => cp.White || cp.White == white))
            {
                Vector2 p = fromPositionToScreen(piece.Position);
                Rectangle r = new Rectangle((int)p.X, (int)(p.Y - ((piece.TileHeight - 1) * TilesSize)), TilesSize, TilesSize * piece.TileHeight);
                r.X += (int)((r.Width - (r.Width * piece.Scale)) / 2);
                r.Width = (int) (r.Width * piece.Scale);
                r.Height = (int)(r.Height * piece.Scale);
                r.Y += (int)(((r.Height - (r.Height * piece.Scale)) / 2) + (piece.OffsetY * TilesSize));
                Color color = piece.Color;

                if(piece.Hightlight != Color.White )
                    b.Draw(HoverTileImage, fromPositionToScreen(piece.Position), piece.Hightlight * 0.5f);

                b.Draw(piece.Texture.STexture, r, color);
            }
        }

        public void drawBackground(SpriteBatch b)
        {
            if (backgroundPos < 0 - BackgroundShade.Width)
                backgroundPos = 0;
            for (int x = backgroundPos; x < Game1.viewport.Width + BackgroundShade.Width * 2; x += BackgroundShade.Width)
                for (int y = backgroundPos; y < Game1.viewport.Height + BackgroundShade.Width * 2; y += BackgroundShade.Width)
                    b.Draw(BackgroundShade, new Vector2(x, y), Color.White * 0.2f);
        }

        public void drawBoard(SpriteBatch b)
        {
           
            b.Draw(BackgroundBorder, new Vector2(Position.X - 10, Position.Y - 10), Color.White);
            b.Draw(BoardImage, BoardBounds, Color.White);
            b.Draw(Structure, BoardBounds, Color.White * 0.6f);
            if (GameQuestion == null)
            {
                if (hoverTile.HasValue && (selectedPiece is ChessPiece p && p.Moves.Contains(hoverTile.Value)))
                    b.Draw(HoverTileImage, fromPositionToScreen(hoverTile.Value), Color.Green * 0.1f);

                if (hoverTile.HasValue && (selectedPiece == null || (selectedPiece is ChessPiece pc && !pc.Moves.Contains(hoverTile.Value))))
                    b.Draw(HoverTileImage, fromPositionToScreen(hoverTile.Value), Color.Yellow * 0.1f);
            }
            if (selectedPiece != null)
                foreach (Vector2 po in selectedPiece.Moves)
                    b.Draw(HoverTileImage, fromPositionToScreen(po), Color.Green * 0.1f);
        }

        public void drawUI(SpriteBatch b)
        {
            if (!isYourTurn() && !Open) {
                GameQuestion = QuestionPool["waiting"];
            }

            if (Winner != null)
            {
                if (LastWinner != Winner)
                    setupQuestions();

                GameQuestion = QuestionPool["checkmate"];
                if (Surrendered)
                    GameQuestion = QuestionPool["surrender"];
            }

            if (SpectatorMode)
                GameQuestion = QuestionPool["spectator"];

            SpriteFont font = Game1.smallFont;

            int margin = 20;

            Rectangle main = BoardBounds;
            main.X -= 10;
            main.Y -= 10;
            main.Width += 20;
            main.Height += 20;

            drawText(b, font, WhitePlayer, Color.Black, new Vector2(main.Left - 20,main.Bottom), margin, true, Color.White, true, true, WhitesTurn, Winner != null && Winner == WhitePlayer ? Color.Yellow : Color.Green, 5);
            drawText(b, font, BlackPlayer, Color.White, new Vector2(main.Right + 20, main.Top), margin, true, Color.Black, false,false,!WhitesTurn, Winner != null && Winner == BlackPlayer ? Color.Yellow : Color.Green, 5);

            string whiteScores = "0-0-0";
            string blackScores = "0-0-0";

            if (!Open)
            {
                if (SavedGameData.LeaderBoard.ContainsKey(WhitePlayer))
                {
                    var score = SavedGameData.LeaderBoard[WhitePlayer];
                    whiteScores = $"{score.Games}-{score.Wins}-{score.Losses}";
                }

                if (BlackPlayer != blackPlayerDefault && SavedGameData.LeaderBoard.ContainsKey(BlackPlayer))
                {
                    var score = SavedGameData.LeaderBoard[BlackPlayer];
                    blackScores = $"{score.Games}-{score.Wins}-{score.Losses}";
                }

                drawText(b, Game1.tinyFont, whiteScores, Color.Black, new Vector2(main.Left - 20, main.Bottom), 5, true, Color.LightGray, true, false, false, Color.Green, 5);
                if (BlackPlayer != blackPlayerDefault)
                    drawText(b, Game1.tinyFont, blackScores, Color.Black, new Vector2(main.Right + 20, main.Top), 5, true, Color.LightGray, false, true, false, Color.Green, 5);
            }


            if (GameQuestion != null)
            {
                var m = Game1.getMousePosition();
                Rectangle r = drawStatusText(b, font, GameQuestion.Text, Color.White, margin, true, Color.Black * 0.5f);
                var pos = new Vector2(r.Left, r.Bottom + 10);
                foreach (Choice choice in GameQuestion.Choices)
                {
                    bool hover = choice.Bounds.HasValue && choice.Bounds.Value.Contains(m);
                    choice.Bounds = drawText(b, font, choice.Text, Color.White, pos, 20, true, Color.DarkCyan * (hover ? 1f : 0.5f), false, false,hover , Color.White, 5);
                    pos.X = choice.Bounds.Value.Right + 10;
                }
                }

            if(!drawLastMatches(b))
                drawLeaderBoard(b);

            b.Draw(Game1.mouseCursors, new Vector2((float)Game1.getMouseX(), (float)Game1.getMouseY()), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.mouseCursor, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)(4.0 + (double)Game1.dialogueButtonScale / 150.0), SpriteEffects.None, 1f);

        }

        public Rectangle drawStatusText(SpriteBatch b, SpriteFont font, string text, Color textColor, int margin, bool boxed, Color boxColor)
        {
            Vector2 size = font.MeasureString(text);
            size.X += margin * 2;
            size.Y += margin * 2;

            Vector2 position = new Vector2(BoardBounds.Right + 20, BoardBounds.Bottom - size.Y * 2);

            if (boxed)
                b.Draw(Pixel, new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y), boxColor);

            b.DrawString(font, text, position + new Vector2(margin, margin), textColor);

            return new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
        }

        public void drawLeaderBoard(SpriteBatch b)
        {
            var font = Game1.smallFont;
            Color color = Color.Black * 0.7f;
            var results = SavedGameData.LeaderBoard.Values.ToList();
            Vector2 pos = new Vector2(50, 120);
            var bounds = drawText(b, font, I18n("leader"), Color.White, pos, 10, true, Color.DarkCyan, false, false, false, Color.DarkCyan, 5);

            if (!bounds.Contains(Game1.getMousePosition()))
                return;

            bounds.Y += 5;
            int i = 1;
            foreach(var result in results.OrderBy(r => int.MaxValue - r.Wins))
            {
                if (i > 10)
                    break;
                pos.Y = bounds.Bottom + 5;
                var r = result;
                bounds = drawText(b, font, $"{i}. {r.Name} [{r.Games}G/{r.Wins}W/{r.Losses}L]", Color.Black * 0.5f, pos, 5, true, Color.White * 0.5f, false, false, false, Color.DarkCyan, 5);
                i++;
            }
        }

        public bool drawLastMatches(SpriteBatch b)
        {
            var font = Game1.smallFont;
            Color color = Color.Black * 0.7f;
            var results = SavedGameData.LastSessions;
            Vector2 pos = new Vector2(50, 50);
            var bounds = drawText(b, font, I18n("last"), Color.White, pos, 10, true, Color.DarkCyan, false, false, false, Color.DarkCyan, 5);

            if (!bounds.Contains(Game1.getMousePosition()))
                return false;

            bounds.Y += 5;
            for (int i = results.Count() - 1; i >= 0; i--)
            {
                pos.Y = bounds.Bottom + 5;
                var r = results[i];
                bounds = drawText(b, font, $"{r.WhitePlayer} v. {r.BlackPlayer} @ {r.Winner} [{r.Time}]", Color.Black * 0.5f, pos, 5, true, Color.White * 0.5f, false, false, false, Color.DarkCyan, 5);
            }

            return true;
        }
        public Rectangle drawText(SpriteBatch b, SpriteFont font, string text, Color textColor, Vector2 position, int margin, bool boxed, Color boxColor, bool left, bool top, bool mark, Color markColor, int markHeight)
        {
            Vector2 size = font.MeasureString(text);
            size.X += margin*2;
            size.Y += margin*2;
            if (left)
                position.X -= size.X;

            if (top)
                position.Y -= size.Y;

            if (boxed)
                b.Draw(Pixel, new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y), boxColor);

            if(mark)
                b.Draw(Pixel, new Rectangle((int)position.X, (int)(position.Y + size.Y - markHeight), (int)size.X, (int)markHeight), markColor);

            b.DrawString(font, text, position + new Vector2(margin,margin), textColor);

            return new Rectangle((int)position.X, (int) position.Y, (int)size.X, (int)size.Y);
        }

        internal Vector2 fromPositionToScreen(Vector2 pos)
        {
            return new Vector2(Position.X + (int)pos.X * TilesSize, Position.Y + (int)pos.Y * TilesSize);
        }

        internal Vector2? fromScreenToPosition(Vector2 p)
        {
            if (BoardBounds.Contains(p.toPoint()))
            {
                p.X -= BoardBounds.X;
                p.Y -= BoardBounds.Y;
                return new Vector2((int)(p.X / TilesSize), (int)(p.Y / TilesSize));
            }
            else
                return null;
        }


        #endregion

        #region update

        public bool tick(GameTime time)
        {
            if (time.TotalGameTime.Ticks % 3 == 0)
                backgroundPos--;
            Vector2? p = fromScreenToPosition(Game1.getMousePosition().toVector2());

            if (p.HasValue)
                hoverTile = p.Value;
            else
                hoverTile = null;
           
            return Quit;
        }

        #endregion

        #region control

        public void quitGame()
        {
                Quit = true;
        }

        public void receiveKeyPress(Keys k)
        {
         
        }

        public void receiveKeyRelease(Keys k)
        {
            if (k == Keys.Escape)
                quitGame();
        }

        public void receiveLeftClick(int x, int y, bool playSound = true)
        {

        }

        public void receiveRightClick(int x, int y, bool playSound = true)
        {

        }

        public bool isYourTurn()
        {
            if (Open)
                return true;

            if (WhitesTurn && WhitePlayer != Game1.player.Name)
                return false;

            if (!WhitesTurn && BlackPlayer != Game1.player.Name)
                return false;

            return true;
        }

        public void releaseLeftClick(int x, int y)
        {
            if (GameQuestion is Question q && q.Choices.Find(c => c.Bounds.HasValue && c.Bounds.Value.Contains(new Point(x,y))) is Choice choice)
                choice.Pick(this);


            if (SpectatorMode)
                return;

                if (Winner != null)
                return;

            if (!isYourTurn())
                return;

            var p = fromScreenToPosition(new Vector2(x, y));

            if (!p.HasValue)
                return;

            ChessPiece testKing = null;

            if (Board.Find(c => c.Position == p.Value && c.White == WhitesTurn) is ChessPiece cp && cp.Moves.Count > 0 && hasMoves(cp, Board, WhitesTurn) && !(cp.Moves.Count == 1 && cp.Moves.First().Y is float yp && (yp < 0 || yp > 7)))
            {
                selectedPiece = cp;
                foreach (var piece in Board)
                {
                    piece.Texture.Paused = true;
                    piece.Texture.CurrentFrame = 0;
                }

                selectedPiece.Texture.Paused = false;
            }
            else if (selectedPiece != null && selectedPiece.Moves.Contains(p.Value) && (getTestBoard(Board, selectedPiece, p.Value, out testKing) is List<ChessPiece> tBoard && !isTheKingInDanger(tBoard, selectedPiece.White, testKing)))
            {
                Point from = selectedPiece.Position.toPoint();
                Point to = p.Value.toPoint();
                temp = new Point[2] { from, to };

                if (selectedPiece is Rook r && (to.X == Kings[r.White].Position.X -1 || to.X == Kings[r.White].Position.X +1) && isCastlingPossible(Board,r))
                {
                    selectedPiece.Texture.Paused = true;
                    selectedPiece.Texture.CurrentFrame = 0;
                    GameQuestion = QuestionPool["castling"];
                }
                else {
                    selectedPiece.Texture.Paused = true;
                    selectedPiece.Texture.CurrentFrame = 0;

                    if (selectedPiece is Pawn && (to.Y == 7 || to.Y == 0))
                        GameQuestion = QuestionPool["promotion"];
                    else
                        nextTurn(from, to);
                }

            }
        }

        public List<ChessPiece> getTestBoard(List<ChessPiece> board, ChessPiece testPiece, Vector2 testPosition, out ChessPiece king)
        {
            List<ChessPiece> testBoard = new List<ChessPiece>();
            ChessPiece findKing = null;

            foreach (ChessPiece cp in board.Where(c => c != testPiece))
            {
                var piece = cp.Clone(Helper);
                if (piece is King && piece.White == testPiece.White)
                    findKing = piece;
                testBoard.Add(cp.Clone(Helper));
            }

            ChessPiece test = testPiece.Clone(Helper);
            test.Position = testPosition;
            testBoard.RemoveAll(c => c.Position == testPosition);
            if (test is King)
                findKing = test;

            testBoard.Add(test);

            king = findKing;
            return testBoard;

        }

        public bool isTheKingInDanger(List<ChessPiece> board, bool white, ChessPiece king = null)
        {
            calculateMoves(board);
            king = king == null ? Kings[white] : king;
            foreach (ChessPiece cp in board.Where(c => c.White != white))
            {
                if (cp.Moves.Contains(king.Position))
                    return true;
            }

            return false;
        }

        public void releaseRightClick(int x, int y)
        {

        }
      
        public void unload()
        {
            Game1.mouseCursorTransparency = 1f;
        }

        public bool doMainGameUpdates()
        {
            return false;
        }

        public bool overrideFreeMouseMovement()
        {
            return false;
        }

        public void leftClickHeld(int x, int y)
        {

        }

        public void changeScreenSize()
        {
            setupBoard();
        }

        public void receiveEventPoke(int data)
        {

        }

        public string minigameId()
        {
            return "Platonymous.Chess";
        }

        public bool forceQuit()
        {
            Quit = true;
            return true;
        }

        #endregion
    }
}
