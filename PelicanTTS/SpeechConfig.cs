
namespace PelicanTTS
{
    class SpeechConfig
    {
        public string name { get; set; }
        public string voicename { get; set; }
        public int age { get; set; }
        public int gender { get; set; }

        public SpeechConfig(string name, string voicename, int gender, int age)
        {
            this.name = name;
            this.voicename = voicename;
            this.age = age;
            this.gender = gender;
        }

    }
}
