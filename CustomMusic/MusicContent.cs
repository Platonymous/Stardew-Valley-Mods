using System.Collections.Generic;

namespace CustomMusic
{
    public class MusicContent
    {
        public List<MusicItem> Music { get; set; } = new List<MusicItem>();
        public List<LocationItem> Locations { get; set; } = new List<LocationItem>();
    }
}
