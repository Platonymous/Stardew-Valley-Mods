
namespace PelicanTTS
{
    class SpeechConfig
    {
        public string name { get; set; }
        public string voicename { get; set; }

        public SpeechConfig(string name, string voicename)
        {
            this.name = name;
            this.voicename = voicename;
        }

    }
}
