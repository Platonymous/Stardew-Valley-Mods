using StardewModdingAPI;
using StardewValley.GameData.Movies;
using System.Collections.Generic;

namespace CustomMovies
{
    public class TranslatableMovieReactions
    {
        public MovieCharacterReaction Reaction { get; set; }

        public IContentPack _pack { get; set; }

        public TranslatableMovieReactions(MovieCharacterReaction reaction, IContentPack pack)
        {
            Reaction = reaction;
            _pack = pack;
        }
    }
}
