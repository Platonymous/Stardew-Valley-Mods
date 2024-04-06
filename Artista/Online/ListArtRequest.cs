using System.Collections.Generic;

namespace Artista.Online
{
    public class ListArtRequest
    {
        public int page { get; set; }
        public int perPage { get; set; }
        public int totalPages { get; set; }

        public int totalItems { get; set; }

        public List<OnlineArtpiece> items { get; set; }
    }

    public class Competiton
    {
        public string id { get; set; }

        public string title { get; set; }
        public string end { get; set; }
    }

    public class ListCompetitons
    {
        public int page { get; set; }
        public int perPage { get; set; }
        public int totalPages { get; set; }

        public int totalItems { get; set; }

        public List<Competiton> items { get; set; } = new List<Competiton>();
    }
}
