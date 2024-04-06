using Artista.Furniture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Artista.Artpieces
{

    public abstract class Artpiece
    {
        public string Name { get; private set; }
        public Color[] Canvas { get; private set; }
        public string Author { get; set; }

        public List<Action> Reversals { get; private set; } = new List<Action>();
        public ArtType Type { get; private set; }

        public Material Material { get; private set; }

        public string Description => $"by {Author}";

        public Vector2 Tilesize => new Vector2((Width / Scale) / 16, (Height / Scale) / 16);

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int TileHeight { get => (Height / Scale) / 16; }
        public int TileWidth { get => (Width / Scale) / 16; }

        public int Depth { get; private set; }

        public bool CanRotate { get; private set; }

        public Color CanvasColor { get; private set; }

        protected Texture2D Texture { get; set; }

        protected Texture2D FinishedTexture { get; set; }
        protected Texture2D CPFTexture { get; set; }

        protected int Rotation { get; set; }

        public int Scale { get; set; }

        public string OnlineId { get; set; } = "";

        internal string GetCategoryName => GetString(Type);

        public SavedArtpiece Save()
        {
            return new SavedArtpiece(Name,Author,Width,Height,Depth,(int) Type, (int) Material, CanvasColor, CanRotate, Scale, Canvas, OnlineId);
        }

        public virtual void Reverse()
        {
            if(Reversals.Count > 0)
            {
                Reversals.Last().Invoke();
                Reversals.Remove(Reversals.Last());

                Refresh();
            }
        }

        public virtual void AddReversal(Action action)
        {
            Reversals.Add(action);
            if (Reversals.Count() > 10)
                Reversals.Remove(Reversals.First());
        }

        public virtual void Refresh()
        {
            Texture = null;
            FinishedTexture = null;
            CPFTexture = null;
        }

        public Artpiece(SavedArtpiece art)
        {
            OnlineId = art.OnlineId;
            Scale = art.Scale;
            CanvasColor = SavedArtpiece.TryParseColorFromString(art.CanvasColor, out Color color) ? color : Color.AntiqueWhite;
            Material = (Material) art.Material;
            Type = (ArtType) art.ArtType;
            Width = art.Width;
            Height = art.Height;
            Depth = art.Depth;
            Canvas = art.Colors.Select(c => SavedArtpiece.TryParseColorFromString(c, out Color col) ? col : Color.Transparent).ToArray();
            Author = art.Author;
            Name = art.Name;
            CanRotate = art.CanRotate;
        }

        private string GetString(ArtType type)
        {
            if (type == ArtType.Painting)
                return "Painting";

            if (type == ArtType.Sculpture)
                return "Sculpture";

            return type.ToString();
        }

        protected void Update()
        {
            Refresh();
        }

        public void Rotate()
        {
            if (CanRotate)
                RotatePiece();
        }

        public virtual Item GetItem()
        {
            return new PaintingFurniture(this);
        }

        public virtual void Reset()
        {
            Color[] reversed = new Color[Canvas.Length];

            for (int i = 0; i < Canvas.Length; i++)
                reversed[i] = Canvas[i];

            AddReversal(() => Canvas = reversed);

            for (int i = 0; i < Canvas.Length; i++)
                Canvas[i] = Color.Transparent;

            Update();
        }

        protected virtual void RotatePiece()
        {
            Rotation++;
            if (Rotation > 3)
                Rotation = 0;
        }

        public virtual void Paint(int x, int y, Color color, bool update = true)
        {
            if (Canvas.Length > ((y * Width) + x) && Canvas[(y * Width) + x] != color)
            {
                Color org = Canvas[(y * Width) + x];
                AddReversal(() => Canvas[(y * Width) + x] = org);
                Canvas[(y * Width) + x] = color;
                if(update)
                Update();
            }
        }

        public IEnumerable<int> GetConnectedColors(int x, int y, Color org, List<int> idx)
        {
            var o = (y * Width) + x;

            if(!idx.Contains(o))
            idx.Add(o);

            for (int y1 = y - 1; y1 <= y +1; y1++)
            {
                for (int x1 = x - 1; x1 <= x + 1; x1++)
                {
                    var j = (y1 * Width) + x1;

                    if (j < 0 || j >= Canvas.Length)
                        continue;


                    Color check = Canvas[j];

                    if(check.PackedValue == org.PackedValue && !idx.Contains(j) && j >= 0 && j < Canvas.Length && x1 >= 0 && x1 < Width && y1 >= 0 && y1 < Height)
                    {
                        yield return j;
                        foreach (var i in GetConnectedColors(x1, y1, org, idx))
                        {
                                yield return i;
                        }
                    }
                }
            }

        }

        public virtual void DrawLine(Point start, Point end, Color color, bool update = true)
        {
            Color[] reversed = new Color[Canvas.Length];

            for (int i = 0; i < Canvas.Length; i++)
                reversed[i] = Canvas[i];

            AddReversal(() => Canvas = reversed);

            List<Point> points = new List<Point>();
            int x = start.X;
            int y = start.Y;

            while(x != end.X || y != end.Y)
            {
                points.Add(new Point(x,y));

                if (x < end.X)
                    x++;
                else if (x > end.X)
                    x--;

                if (y < end.Y)
                    y++;
                else if (y > end.Y)
                    y--;
            }

            points.Add(end);

            foreach(var p in points)
                Canvas[(p.Y * Width) + p.X] = color;

            if (update)
                Update();
        }

        public virtual void Fill(int x, int y, Color color, bool update = true)
        {
            if (Canvas.Length > ((y * Width) + x) && Canvas[(y * Width) + x] != color)
            {
                Color org = Canvas[(y * Width) + x];

                Color[] reversed = new Color[Canvas.Length];

                for (int i = 0; i < Canvas.Length; i++)
                    reversed[i] = Canvas[i];

                AddReversal(() => Canvas = reversed);

                List<int> idx = new List<int>();

                foreach (var i in GetConnectedColors(x, y, org, new List<int>()))
                    if(!idx.Contains(i))
                    idx.Add(i);

                foreach (var i in idx)
                    Canvas[i] = color;

                Canvas[(y * Width) + x] = color;
                if (update)
                    Update();
            }
        }

        public void SetName(string name)
        {
            Name = name;
            Save();
        }

        public void SetAuthor(string author)
        {
            Author = author;
            Save();
        }

        public Texture2D GetTexture()
        {
            if (Texture == null)
            {
                Texture = DrawTexture();
                FinishedTexture = null;
            }

            return Texture;
        }

        public Texture2D GetFinishedTextureForMenu()
        {
            if (FinishedTexture == null)
                FinishedTexture = FinishTextureForMenu();

            return FinishedTexture;
        }


        public Texture2D GetFullTexture()
        {
            if (CPFTexture == null)
                CPFTexture = FinishFullTexture();

            return CPFTexture;
        }


        protected virtual Texture2D DrawTexture()
        {
            var texture = new Texture2D(Game1.graphics.GraphicsDevice, Width, Height);
            texture.SetData(Canvas);
            return texture;
        }

        protected virtual Texture2D FinishFullTexture()
        {
            var texture = GetTexture();
            var finishedTexture = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);
            var canvas2 = new Color[texture.Width * texture.Height];

            for (int y = 0; y < texture.Height; y++)
                for (int x = 0; x < texture.Width; x++)
                {
                    var i = (y * texture.Width) + x;
                    if (Canvas[i] == Color.Transparent)
                        canvas2[i] = CanvasColor;
                    else
                        canvas2[i] = Canvas[i];
                }


            finishedTexture.SetData(canvas2);
            return finishedTexture;
        }

        protected virtual Texture2D FinishTextureForMenu()
        {
            var texture = GetTexture();
            var finishedTexture = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);
            var canvas2 = new Color[texture.Width * texture.Height];

            for (int y = 0; y < texture.Height; y++)
                for (int x = 0; x < texture.Width; x++)
                {
                    var i = (y * texture.Width) + x;
                    if(x <= Scale * 2 || x >= texture.Width - Scale * 2 || y <= Scale || y >= texture.Height - Scale)
                        canvas2[i] = Color.Transparent;
                    else if (Canvas[i] == Color.Transparent)
                        canvas2[i] = CanvasColor;
                    else
                        canvas2[i] = Canvas[i];
                }


            finishedTexture.SetData(canvas2);
            return finishedTexture;
        }

        public Artpiece(int width, int height, int depth, int scale, ArtType type, Material material, Color canvasColor, bool canRotate)
        {
            Scale = scale;
            CanvasColor = canvasColor;
            Material = material;
            Type = type;
            Width = width * 16 * Scale;
            Height = height * 16 * Scale;
            Depth = depth * 16;
            Canvas = new Color[Width * Height * depth];
            Author = Game1.player?.Name ?? "Unknown";
            Name = GetString(Type);
            CanRotate = canRotate;
        }

    }
}
