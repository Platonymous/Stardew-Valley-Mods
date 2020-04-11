namespace CustomMusic
{
    public class SoundItem : MusicItem
    {
          public SoundItem()
        {
            Loop = false;
        }

        public SoundItem(string id, string file, bool ambient, bool loop, bool preload, string conditions)
        {
            this.Id = id;
            this.File = file;
            this.Loop = loop;
            this.Ambient = ambient;
            this.Preload = preload;
            this.Conditions = conditions;
        }
    }
}