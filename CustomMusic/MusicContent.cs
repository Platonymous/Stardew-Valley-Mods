using System.Collections.Generic;

namespace CustomMusic
{
    public class MusicContent
    {
        public List<MusicItem> Music { get; set; } = new List<MusicItem>();

        public List<SoundItem> Sounds { get; set; } = new List<SoundItem>();

        public List<SoundEmitter> Emitters { get; set; } = new List<SoundEmitter>();
        public List<LocationItem> Locations { get; set; } = new List<LocationItem>();
    }
}
