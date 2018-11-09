using Microsoft.Xna.Framework.Audio;

namespace CustomMusic
{
    public class MusicItem
    {
        public string Id { get; set; }
        public string File { get; set; }
        public bool Loop { get; set; } = true;
        public bool Ambient { get; set; } = false;

        public MusicItem()
        {

        }

        public MusicItem(string id, string file, bool ambient, bool loop)
        {
            this.Id = id;
            this.File = file;
            this.Loop = loop;
            this.Ambient = ambient;
        }
    }
}
