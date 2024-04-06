namespace Artista.Online
{
    public class OnlineArtpieceBase
    {
        public string id { get; set; }

        public string artwork { get; set; }

        public bool active { get; set; } = true;

        public string collection { get; set; }

        public int downloads { get; set; }

        public string author { get; set; }

        public bool won { get; set; } = false;
    }
}
