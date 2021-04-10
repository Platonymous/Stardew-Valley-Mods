using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using static Snake.SnakeMinigame;

namespace Snake
{
    public class Board : SnakeObject
    {
        public List<SnakeObject> Objects { get; set; }
        public Point Origin { get; set; }
        public Point Center { get; set; }
        public Dictionary<Point, Direction> Turns { get; set; }
        public Collectible nextCollectible;
        public bool Paused { get; set; }
        public bool GameOver { get; set; }

        public Board(SnakeMinigame gameInstance)
            :base(Vector2.Zero, gameInstance)
        {
            Objects = new List<SnakeObject>();
            Turns = new Dictionary<Point, Direction>();
            DrawColor = GameInstance.BoardColor;
            DrawTexture = GameInstance.BoardTexture;
            Paused = true;
            GameOver = false;
        }

        public override void Resize() {
            int height = (int)Math.Floor(Game1.viewport.Height * GameInstance.Scale);
            int width = (int)Math.Floor((float)height / GameInstance.BoardTexture.Height * GameInstance.BoardTexture.Width);
            Size = new Point(width, height);
            position = new Vector2(0, 0);
            
            Origin = new Point((int) Math.Floor(((Game1.viewport.Width - Size.X) / 2f) + Size.X / 2), (int)Math.Floor((Game1.viewport.Height - Size.Y) / 2f));
            Center =  new Point(Origin.X, Origin.Y + (int) Math.Floor(Size.Y / 2f));
            Drawposition = new Point(Origin.X - (int)Math.Floor(Size.X / 2f), Origin.Y);

            if(Objects != null && Objects.Count > 0)
                foreach (SnakeObject obj in Objects)
                    obj.Resize();
        }

        public void Add(SnakeObject obj)
        {
            Objects.Add(obj);
        }

        public void Remove(SnakeObject obj)
        {
            Objects.Remove(obj);
        }

        public override void Next()
        {
            if (!Paused)
                GameInstance.Player.Next();
        }

        public void AddPlayer()
        {
            GameInstance.Player = new SnakesHead(GameInstance);
            GameInstance.Player.position = new Vector2(GameInstance.TiledSize.X / 2, GameInstance.TiledSize.Y / 2);
            GameInstance.Player.Facing = Direction.UP;
            Add(GameInstance.Player);
            Child = GameInstance.Player;
        }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(GameInstance.debug ? GameInstance.BoardDebug : DrawTexture, new Rectangle(Drawposition.X, Drawposition.Y, Size.X, Size.Y), DrawColor);
            
            Objects.Sort((x, y) => getDistanceFromOrigin(x).CompareTo(getDistanceFromOrigin(y)));

            foreach (SnakeObject obj in Objects)
                obj.Draw(b);

            base.Draw(b);
        }

        public Point GetDrawPosition(Vector2 pos, Point size)
        {
            float th = Size.Y / (GameInstance.TiledSize.Y + 1);
            float tw = Size.X / GameInstance.TiledSize.X;

            int y = (int)Math.Round((Origin.Y - size.Y / 2) + pos.Y * (th/2) + pos.X * (th/2) , 0);
            int x = (int)Math.Round((Origin.X - size.X / 2) - pos.Y * (tw/2) + pos.X * (tw/2), 0);
            return new Point(x, y);
        }

        public void SpawnCollectible()
        {
            List<int> indexes = new List<KeyValuePair<int, string>>(((Dictionary<int,string>)Game1.objectInformation).Where(o => o.Value.Contains("Basic - 75") || o.Value.Contains("Basic -79"))).Select(i => i.Key).ToList();

            int index = indexes[GameInstance.Random.Next(0,indexes.Count-1)];

            Vector2 pos = Vector2.Zero;
            while (pos == Vector2.Zero || Objects.Exists(o => getDistance(o.position, pos) < 2) || getDistance(GameInstance.Player.position, pos) < 6)
                pos = new Vector2(GameInstance.Random.Next(2, GameInstance.TiledSize.X - 2), GameInstance.Random.Next(2, GameInstance.TiledSize.Y - 2));

            nextCollectible = new Collectible(pos, index, false, GameInstance);
            Add(nextCollectible);

        }

        public double getDistanceFromOrigin(SnakeObject s)
        {
            return getDistance(Vector2.Zero, s.position);
        }

        public double getDistance(Vector2 p1, Vector2 p2)
        {
            float distX = Math.Abs(p1.X - p2.X);
            float distY = Math.Abs(p1.Y - p2.Y);
            double dist = Math.Sqrt((distX * distX) + (distY * distY));
            return dist;
        }

        public override void Reposition()
        {

        }
    }
}
