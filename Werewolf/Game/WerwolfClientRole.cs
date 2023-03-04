using System.Collections.Generic;

namespace Werewolf.Game
{
    public class WerwolfClientRole
    {
        public string Name { get; set; }

        public string ID { get; set; }

        public List<WerwolfClientAction> Actions { get; set; } = new List<WerwolfClientAction>();

        public string Description { get; set; }

        public WerwolfClientRole()
        {

        }

        public WerwolfClientRole(string name, string iD, List<WerwolfClientAction> actions, string description)
        {
            Name = name;
            ID = iD;
            Actions = actions;
            Description = description;
        }
    }
}
