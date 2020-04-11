namespace CustomMusic
{
    public class SoundEmitter : MusicItem
    {
        public SoundEmitter()
        {
            Loop = true;
            Ambient = true;
        }

        public SoundEmitter(string id, string file, bool ambient, bool loop, bool preload, string conditions)
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