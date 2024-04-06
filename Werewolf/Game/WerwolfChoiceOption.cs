namespace LandGrants.Game
{
    public class WerwolfChoiceOption
    {
        public string Name { get; set; }
        public string ID { get; set; }

        public WerwolfChoiceOption()
        {

        }

        public WerwolfChoiceOption(string name, string id)
        {
            Name = name;
            ID = id;
        }
    }
}
