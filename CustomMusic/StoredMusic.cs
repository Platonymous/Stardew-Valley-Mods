using Microsoft.Xna.Framework.Audio;

namespace CustomMusic
{
    public class StoredMusic
    {
        public string Id { get; set; }
        public bool Loop { get; set; } 
        public bool Ambient { get; set; }
        public string Path { get; set; }
        public SoundEffect _sound = null;

        public bool IsEmitter { get; set; } = false;

        public int Distance { get; set; } = -1;
        public SoundEffect Sound
        {
            get
            {
                if (_sound == null)
                    return CustomMusicMod.LoadSoundEffect(Path);
                else
                    return _sound;
            }
            set
            {
                _sound = value;
            }
        }
        public string Conditions { get; set; }

        public StoredMusic()
        {

        }

        public StoredMusic(string id, SoundEffect sound, bool ambient, bool loop, string conditions, string path)
        {
            this.Id = id;
            this.Sound = sound;
            this.Path = path;
            this.Ambient = ambient;
            this.Loop = loop;
            this.Conditions = conditions;
        }
    }
}
