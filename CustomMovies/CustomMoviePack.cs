using StardewValley.GameData.Movies;
using System.Collections.Generic;

namespace CustomMovies
{
    public class CustomMoviePack
    {
        public List<CustomMovieData> Movies { get; set; } = new List<CustomMovieData>();
        public List<MovieCharacterReaction> Reactions { get; set; } = new List<MovieCharacterReaction>();
    }
}
