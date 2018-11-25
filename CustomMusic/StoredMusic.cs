using Microsoft.Xna.Framework.Audio;

namespace CustomMusic
{
    public class StoredMusic
    {
        public string Id { get; set; }
        public bool Loop { get; set; } 
        public bool Ambient { get; set; }
        public SoundEffect Sound { get; set; }
        public string Conditions { get; set; }

        public StoredMusic()
        {

        }

        public StoredMusic(string id, SoundEffect sound, bool ambient, bool loop, string conditions)
        {
            this.Id = id;
            this.Sound = sound;
            this.Ambient = ambient;
            this.Loop = loop;
            this.Conditions = conditions;
        }
    }
}
