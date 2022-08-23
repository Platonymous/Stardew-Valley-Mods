using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.GameData.Movies;
using System.Collections.Generic;
using PyTK.Types;

namespace CustomMovies
{
    public class CustomMovieData
    {
        public string Id { get; set; }

        public string FixedMovieID { get; set; } = null;
        public string Season { get; set; } = "spring";
        public string Conditions { get; set; } = "";
        public string Sheet { get; set; }
        public int SheetIndex { get; set; } = 0;
        public string Title { get; set; } = "A Movie";
        
        public string Description { get; set; } = "Moving Pictures";

        public List<string> Tags { get; set; } = new List<string>();

        public int Frames { get; set; } = 1;

        public bool Looped { get; set; } = true;

        public float Scale { get; set; } = 1;

        public int AnimationSpeed { get; set; } = 12;

        public List<MovieScene> Scenes { get; set; } = new List<MovieScene>();

        internal Texture2D _texture = null;

        internal IContentPack _pack = null;

        public string CranePrizeType { get; set; } = "None";

        public string CranePrizeName { get; set; } = "None";

        public void LoadTexture(IContentPack pack)
        {
            _pack = pack;
            _texture = _pack.ModContent.Load<Texture2D>(Sheet);

            if (Frames > 1)
                _texture = new AnimatedTexture2D(_texture, _texture.Width, _texture.Height / Frames, AnimationSpeed, Looped, Scale);
            else if (Scale > 1)
                _texture = new ScaledTexture2D(_texture, Scale);

            if (!Tags.Contains("CustomMovie"))
                Tags.Add("CustomMovie");

            if (!Tags.Contains("CMovieID:" + Id))
                Tags.Add("CMovieID:" + Id);
        }

        public MovieData GetData(int year)
        {
            var tMovie = CustomMoviesMod.translateMovie(this);

            MovieData data = new MovieData();
            data.ID = Season + "_movie_" + year;
            data.SheetIndex = SheetIndex;
            data.Tags = Tags;
            data.Title = tMovie.Title;
            data.Scenes = tMovie.Scenes;
            data.Description = tMovie.Description;

            return data;
        }

        public MovieData GetFixedData()
        {
            var tMovie = CustomMoviesMod.translateMovie(this);

            MovieData data = new MovieData();
            data.ID = FixedMovieID;
            data.SheetIndex = SheetIndex;
            data.Tags = Tags;
            data.Title = tMovie.Title;
            data.Scenes = tMovie.Scenes;
            data.Description = tMovie.Description;

            return data;
        }
    }
}
