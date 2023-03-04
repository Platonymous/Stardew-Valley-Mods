using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Werewolf
{
    public class Config
    {
        public int MinPlayers { get; set; } = 8;

        public bool FillWithBots { get; set; } = true;

        public string[] MayorRoles { get; set; } = new[] { "Mayor","Judge" };

        public string[] VillagerRoles { get; set; } = new[] { "Witch", "Seer", "Amor" };

        public string[] WerwolfRoles { get; set; } = new string[] {};

        public SButton ActionButton { get; set; } = SButton.K;

        public SButton NotesButton { get; set; } = SButton.N;

        public string[] NPCs { get; set; } = new string[] { "Abigail", "Alex", "Caroline", "Clint", "Demetrius", "Elliot", "Evelyn", "George", "Gus", "Haley", "Harvey", "Jodi", "Leah", "Lewis", "Marnie", "Maru", "Pam", "Pierre", "Penny", "Robin", "Sam", "Sebastian", "Shane", "Willy" };
    }
}
