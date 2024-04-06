using Artista.Artpieces;
using Artista.Online;
using Microsoft.Xna.Framework;

namespace Artista.Menu
{
    public class OnlineArtPieceEntry
    {
        public Artpiece Art { get; set; }

        public Rectangle Rectangle { get; set; }

        public bool Visible { get; set; } = true;

        public OnlineArtpiece Orp { get; set; }

        public bool Highlighted { get; set; }

        public bool Winner => Orp?.won ?? false;

        public OnlineArtPieceEntry(Artpiece art, Rectangle rectangle, bool highlighted, OnlineArtpiece orp)
        {
            Art = art;
            Rectangle = rectangle;
            Highlighted = highlighted;
            Orp = orp;
        }
    }
}
